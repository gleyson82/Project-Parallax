using System;
using Parallax.Core.Combat;
using Parallax.Core.Definitions;
using Parallax.Gameplay.Audio;
using Parallax.Gameplay.Input;
using Parallax.Gameplay.Players;
using Parallax.Gameplay.World;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Combat
{
    /// <summary>
    /// Arma da classe (Therma-Tek, Kinesys ou Omni-Cripto). O dono mira e controla o calor do cano para
    /// resposta imediata; o servidor valida cadência e distância, faz o raycast e aplica o dano.
    /// Acertar uma Linha Viva rompe o duto e apaga o bônus Fogo Mínimo.
    /// </summary>
    public sealed class PlayerWeapon : NetworkBehaviour
    {
        private static readonly RaycastHit[] Hits = new RaycastHit[16];

        [SerializeField] private ParallaxInput input;
        [SerializeField] private PlayerVitals vitals;
        [SerializeField] private CharacterAnimator characterAnimator;
        [SerializeField] private Transform muzzle;
        [SerializeField] private LayerMask hitMask = ~0;

        private WeaponDefinition _weapon;
        private WeaponHeat _heat;
        private float _nextShotTime;
        private double _serverNextShot;
        private Transform _camera;

        /// <summary>Servidor: uma Linha Viva foi atingida no ponto informado.</summary>
        public static event Action<Vector3> ServerLiveLineHit;

        public event Action Overheated;

        public WeaponDefinition Weapon => _weapon;
        public float Heat01 => _heat?.Heat01 ?? 0f;
        public bool IsOverheated => _heat != null && _heat.Overheated;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetEvents() => ServerLiveLineHit = null;

        public void Equip(WeaponDefinition weapon)
        {
            _weapon = weapon;
            _heat = weapon != null ? new WeaponHeat(weapon.HeatPerShot, weapon.CoolingPerSecond) : null;
        }

        // Cadência e mira do dono (entrada); dano e validação ficam no servidor.
        private void Update()
        {
            if (!IsSpawned || !IsOwner || _heat == null)
            {
                return;
            }

            _heat.Cool(Time.deltaTime);
            if (vitals != null && vitals.IsDowned)
            {
                return;
            }

            if (!input.Fire.IsPressed() || Time.time < _nextShotTime || Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            if (!_heat.TryFire())
            {
                return;
            }

            _nextShotTime = Time.time + _weapon.FireIntervalSeconds;
            Vector3 origin = muzzle != null ? muzzle.position : transform.position + Vector3.up * 1.4f;
            Vector3 direction = AimDirection(origin);
            Vector3 end = LocalTrace(origin, direction, out Vector3 normal, out bool hit);
            CombatFx.Tracer(origin, end, _weapon.TracerColor);
            if (hit)
            {
                CombatFx.Impact(end, normal, _weapon.ChillPerHit > 0f ? ImpactKind.Cryo : ImpactKind.Sparks);
            }

            AudioCues.Play(ShotCue(), origin);
            if (characterAnimator != null)
            {
                characterAnimator.PlayAction(CharacterAction.Fire);
            }

            FireRpc(origin, direction);
            if (_heat.Overheated)
            {
                AudioCues.Play(AudioCue.Overheat, origin);
                Overheated?.Invoke();
            }
        }

        private Vector3 AimDirection(Vector3 origin)
        {
            if (_camera == null && UnityEngine.Camera.main != null)
            {
                _camera = UnityEngine.Camera.main.transform;
            }

            Vector3 aimPoint;
            if (_camera != null)
            {
                Vector3 end = LocalTrace(_camera.position + _camera.forward * 0.5f, _camera.forward, out _, out _);
                aimPoint = end;
            }
            else
            {
                aimPoint = origin + transform.forward * 30f;
            }

            Vector3 direction = (aimPoint - origin).normalized;
            float spread = _weapon.SpreadDegrees;
            return Quaternion.Euler(UnityEngine.Random.Range(-spread, spread), UnityEngine.Random.Range(-spread, spread), 0f) * direction;
        }

        private Vector3 LocalTrace(Vector3 origin, Vector3 direction, out Vector3 normal, out bool hit)
        {
            float range = _weapon != null ? _weapon.RangeMeters : 60f;
            if (TryTrace(origin, direction, range, out RaycastHit result))
            {
                normal = result.normal;
                hit = true;
                return result.point;
            }

            normal = Vector3.up;
            hit = false;
            return origin + direction * range;
        }

        private bool TryTrace(Vector3 origin, Vector3 direction, float range, out RaycastHit closest)
        {
            int count = Physics.RaycastNonAlloc(origin, direction, Hits, range, hitMask, QueryTriggerInteraction.Ignore);
            closest = default;
            float best = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                if (Hits[i].collider.transform.IsChildOf(transform) || Hits[i].distance >= best)
                {
                    continue;
                }

                best = Hits[i].distance;
                closest = Hits[i];
            }

            return best < float.MaxValue;
        }

        [Rpc(SendTo.Server)]
        private void FireRpc(Vector3 origin, Vector3 direction)
        {
            if (_weapon == null || (vitals != null && vitals.IsDowned))
            {
                return;
            }

            double now = NetworkManager.ServerTime.Time;
            if (now < _serverNextShot || (origin - transform.position).sqrMagnitude > 9f)
            {
                return;
            }

            _serverNextShot = now + _weapon.FireIntervalSeconds * 0.7f;
            Vector3 end = origin + direction.normalized * _weapon.RangeMeters;
            bool hitSomething = TryTrace(origin, direction.normalized, _weapon.RangeMeters, out RaycastHit hit);
            ImpactKind impact = _weapon.ChillPerHit > 0f ? ImpactKind.Cryo : ImpactKind.Sparks;
            if (hitSomething)
            {
                end = hit.point;
                IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
                if (target != null && target.IsAlive)
                {
                    target.ServerApplyDamage(_weapon.DamagePerShot, _weapon.ChillPerHit, OwnerClientId, hit.point);
                }
                else if (hit.collider.GetComponentInParent<LiveLine>() != null)
                {
                    impact = ImpactKind.Steam;
                    ServerLiveLineHit?.Invoke(hit.point);
                }
            }

            ShotFxRpc(origin, end, hitSomething ? hit.normal : Vector3.zero, (byte)impact);
        }

        [Rpc(SendTo.NotOwner)]
        private void ShotFxRpc(Vector3 origin, Vector3 end, Vector3 normal, byte impact)
        {
            if (_weapon == null)
            {
                return;
            }

            CombatFx.Tracer(origin, end, _weapon.TracerColor);
            if (normal != Vector3.zero)
            {
                CombatFx.Impact(end, normal, (ImpactKind)impact);
            }

            AudioCues.Play(ShotCue(), origin);
            if (characterAnimator != null)
            {
                characterAnimator.PlayAction(CharacterAction.Fire);
            }
        }

        private AudioCue ShotCue()
        {
            switch (_weapon.DamageKind)
            {
                case DamageKind.Cryo: return AudioCue.ShotCryo;
                case DamageKind.Pulse: return AudioCue.ShotPulse;
                default: return AudioCue.ShotSlag;
            }
        }
    }
}
