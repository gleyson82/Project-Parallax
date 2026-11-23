using System;
using System.Collections;
using System.Collections.Generic;
using Parallax.Gameplay.Audio;
using Parallax.Gameplay.Combat;
using Parallax.Gameplay.Players;
using Parallax.Gameplay.World;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Parallax.Gameplay.Enemies
{
    /// <summary>
    /// Soldado das Esquadras de Contenção do Sindicato. Contém sem calor: espuma criogênica que fere e desacelera.
    /// Doutrina do Fogo Mínimo: a arma trava (LED amarelo, clique audível) quando a mira passa perto de uma Linha Viva.
    /// IA no servidor em ciclos curtos (não em Update); o NavMeshAgent move.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class ContainmentTrooper : NetworkBehaviour, IDamageable
    {
        private static readonly List<ContainmentTrooper> Registry = new List<ContainmentTrooper>();

        [SerializeField] private CharacterAnimator characterAnimator;
        [SerializeField] private Transform muzzle;
        [SerializeField] private Renderer lockLed;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private NetworkObject thermalCorePrefab;
        [SerializeField, Min(1f)] private float maxHealth = 70f;
        [SerializeField, Min(0.2f)] private float fireInterval = 1.6f;
        [SerializeField, Min(1f)] private float range = 24f;
        [SerializeField, Min(0f)] private float damage = 9f;
        [SerializeField, Range(0f, 1f)] private float chill = 0.22f;
        [SerializeField, Range(0f, 1f)] private float accuracy = 0.7f;
        [SerializeField, Min(1f)] private float preferredDistance = 11f;
        [SerializeField, Min(0.05f)] private float thinkInterval = 0.25f;
        [SerializeField, Min(1f), Tooltip("Guardas só reagem a operadores dentro deste raio até serem alertados.")]
        private float aggroRange = 16f;
        [SerializeField] private LayerMask sightMask = ~0;

        private readonly NetworkVariable<float> _health = new NetworkVariable<float>(70f);
        private readonly NetworkVariable<bool> _weaponLocked = new NetworkVariable<bool>();
        private readonly NetworkVariable<bool> _dead = new NetworkVariable<bool>();

        private NavMeshAgent _agent;
        private bool _alerted;
        private MaterialPropertyBlock _ledBlock;
        private double _nextShot;
        private PlayerVitals _target;

        public static IReadOnlyList<ContainmentTrooper> All => Registry;

        /// <summary>Servidor: soldado abatido (posição para o Núcleo Térmico).</summary>
        public static event Action<ContainmentTrooper> ServerKilled;

        public bool IsAlive => !_dead.Value;
        public bool WeaponLocked => _weaponLocked.Value;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Registry.Clear();
            ServerKilled = null;
        }

        public static int CountAlive()
        {
            int count = 0;
            foreach (ContainmentTrooper trooper in Registry)
            {
                if (trooper != null && trooper.IsAlive)
                {
                    count++;
                }
            }

            return count;
        }

        private void Awake() => _agent = GetComponent<NavMeshAgent>();

        public override void OnNetworkSpawn()
        {
            Registry.Add(this);
            _weaponLocked.OnValueChanged += OnWeaponLockedChanged;
            _dead.OnValueChanged += OnDeadChanged;
            _agent.enabled = IsServer;
            if (characterAnimator != null)
            {
                characterAnimator.UseSet(0);
            }

            ApplyLed(false);
            if (IsServer)
            {
                _health.Value = maxHealth;
                StartCoroutine(Think());
            }
        }

        public override void OnNetworkDespawn()
        {
            Registry.Remove(this);
            _weaponLocked.OnValueChanged -= OnWeaponLockedChanged;
            _dead.OnValueChanged -= OnDeadChanged;
        }

        /// <summary>Servidor: aplica o multiplicador de densidade/vida da sessão (escalabilidade-1-a-3).</summary>
        public void ServerConfigure(float healthMultiplier) => _health.Value = maxHealth * Mathf.Max(0.1f, healthMultiplier);

        /// <summary>Servidor: ondas de reforço entram já caçando, sem depender do raio de percepção.</summary>
        public void ServerAlert() => _alerted = true;

        public void ServerApplyDamage(float amount, float chillAmount, ulong attackerClientId, Vector3 hitPoint)
        {
            if (!IsServer || _dead.Value)
            {
                return;
            }

            _alerted = true;
            _health.Value = Mathf.Max(0f, _health.Value - amount);
            if (chillAmount > 0f && _agent.enabled)
            {
                _agent.speed = Mathf.Max(0.8f, _agent.speed * (1f - chillAmount * 0.5f));
            }

            if (_health.Value <= 0f)
            {
                _dead.Value = true;
                _weaponLocked.Value = false;
                if (_agent.enabled && _agent.isOnNavMesh)
                {
                    _agent.isStopped = true;
                }

                if (thermalCorePrefab != null)
                {
                    thermalCorePrefab.InstantiateAndSpawn(NetworkManager, position: transform.position + Vector3.up * 0.6f);
                }

                ServerKilled?.Invoke(this);
                StartCoroutine(DespawnLater(3f));
            }
        }

        /// <summary>Servidor: recuo ao fim da defesa (some sem soltar Núcleo).</summary>
        public void ServerRetreat()
        {
            if (IsServer && !_dead.Value)
            {
                StartCoroutine(DespawnLater(0.1f));
            }
        }

        private IEnumerator Think()
        {
            var wait = new WaitForSeconds(thinkInterval);
            while (!_dead.Value)
            {
                ThinkOnce();
                yield return wait;
            }
        }

        private void ThinkOnce()
        {
            _target = NearestStanding();
            if (_target == null || !_agent.isOnNavMesh)
            {
                _weaponLocked.Value = false;
                return;
            }

            Vector3 targetChest = _target.transform.position + Vector3.up * 1.3f;
            float distance = Vector3.Distance(transform.position, _target.transform.position);
            if (distance > preferredDistance + 2f)
            {
                _agent.isStopped = false;
                _agent.SetDestination(_target.transform.position);
            }
            else
            {
                _agent.isStopped = true;
                Vector3 flat = _target.transform.position - transform.position;
                flat.y = 0f;
                if (flat.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(flat), 240f * thinkInterval);
                }
            }

            Vector3 origin = muzzle != null ? muzzle.position : transform.position + Vector3.up * 1.4f;
            bool locked = LiveLine.IsInForbiddenFireZone(origin) || LiveLine.IsInForbiddenFireZone(targetChest) ||
                          LiveLine.IsInForbiddenFireZone(Vector3.Lerp(origin, targetChest, 0.5f));
            _weaponLocked.Value = locked && distance <= range;
            if (locked || distance > range || NetworkManager.ServerTime.Time < _nextShot || !HasLineOfSight(origin, targetChest))
            {
                return;
            }

            _nextShot = NetworkManager.ServerTime.Time + fireInterval * UnityEngine.Random.Range(0.85f, 1.2f);
            bool hit = UnityEngine.Random.value <= accuracy;
            Vector3 end = hit ? targetChest : targetChest + UnityEngine.Random.insideUnitSphere * 1.6f;
            if (hit)
            {
                _target.ServerApplyDamage(damage, chill, NetworkManager.ServerClientId, end);
            }

            FireFxRpc(origin, end);
        }

        private bool HasLineOfSight(Vector3 origin, Vector3 target)
        {
            Vector3 direction = target - origin;
            if (!Physics.Raycast(origin, direction.normalized, out RaycastHit hit, direction.magnitude, sightMask, QueryTriggerInteraction.Ignore))
            {
                return true;
            }

            return hit.collider.GetComponentInParent<PlayerVitals>() != null;
        }

        private PlayerVitals NearestStanding()
        {
            PlayerVitals best = null;
            float bestDistance = float.MaxValue;
            foreach (PlayerVitals vitals in PlayerVitals.All)
            {
                if (vitals == null || !vitals.IsAlive)
                {
                    continue;
                }

                float distance = (vitals.transform.position - transform.position).sqrMagnitude;
                if (!_alerted && distance > aggroRange * aggroRange)
                {
                    continue;
                }

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = vitals;
                }
            }

            if (best != null)
            {
                _alerted = true;
            }

            return best;
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void FireFxRpc(Vector3 origin, Vector3 end)
        {
            CombatFx.Tracer(origin, end, new Color(0.55f, 0.9f, 1f));
            CombatFx.Impact(end, (origin - end).normalized, ImpactKind.Cryo);
            AudioCues.Play(AudioCue.TrooperShot, origin);
            if (characterAnimator != null)
            {
                characterAnimator.PlayAction(CharacterAction.Aim);
            }
        }

        private IEnumerator DespawnLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn(true);
            }
        }

        private void OnWeaponLockedChanged(bool previous, bool locked)
        {
            ApplyLed(locked);
            if (characterAnimator == null)
            {
                return;
            }

            if (locked)
            {
                characterAnimator.PlayAction(CharacterAction.WeaponLock);
                AudioCues.Play(AudioCue.WeaponLockClick, transform.position + Vector3.up * 1.4f);
            }
            else
            {
                characterAnimator.StopAction();
            }
        }

        private void OnDeadChanged(bool previous, bool dead)
        {
            if (dead && visualRoot != null)
            {
                StartCoroutine(Collapse());
            }
        }

        private IEnumerator Collapse()
        {
            Quaternion start = visualRoot.localRotation;
            Quaternion end = start * Quaternion.Euler(-85f, 0f, 0f);
            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                visualRoot.localRotation = Quaternion.Slerp(start, end, Mathf.SmoothStep(0f, 1f, t / 0.6f));
                yield return null;
            }
        }

        private void ApplyLed(bool locked)
        {
            if (lockLed == null)
            {
                return;
            }

            _ledBlock ??= new MaterialPropertyBlock();
            Color color = locked ? new Color(1f, 0.78f, 0.1f) * 6f : new Color(0.1f, 0.6f, 1f) * 1.5f;
            _ledBlock.SetColor("_BaseColor", color);
            _ledBlock.SetColor("_EmissionColor", color);
            lockLed.SetPropertyBlock(_ledBlock);
        }
    }
}
