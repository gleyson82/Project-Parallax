using System;
using System.Collections.Generic;
using Parallax.Core.Combat;
using Parallax.Gameplay.Audio;
using Parallax.Gameplay.Characters;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Combat
{
    /// <summary>
    /// Integridade do traje do operador, autoritativa no servidor. Em zero ele cai: um aliado resgata segurando
    /// Interagir; sem aliado de pé (ou após o tempo limite), ele volta ao último Ponto de Brasa.
    /// O frio da espuma criogênica do Sindicato desacelera o operador.
    /// </summary>
    public sealed class PlayerVitals : NetworkBehaviour, IDamageable
    {
        private static readonly List<PlayerVitals> Registry = new List<PlayerVitals>();

        [SerializeField] private ParallaxCharacterMotor motor;
        [SerializeField] private CharacterController controller;
        [SerializeField, Min(1f)] private float bleedOutSeconds = 22f;
        [SerializeField, Min(0.5f)] private float soloRespawnSeconds = 4f;
        [SerializeField, Min(0f)] private float regenDelaySeconds = 6f;
        [SerializeField, Min(0f)] private float regenPerSecond = 8f;
        [SerializeField, Min(0f)] private float chillRecoveryPerSecond = 0.3f;
        [SerializeField, Range(0f, 1f)] private float reviveFraction = 0.4f;

        private readonly NetworkVariable<float> _integrity = new NetworkVariable<float>(100f);
        private readonly NetworkVariable<float> _max = new NetworkVariable<float>(100f);
        private readonly NetworkVariable<float> _chill = new NetworkVariable<float>();
        private readonly NetworkVariable<bool> _downed = new NetworkVariable<bool>();
        private readonly NetworkVariable<double> _downedAt = new NetworkVariable<double>();

        private VitalsModel _model;
        private Vector3 _checkpoint;
        private double _lastDamageAt;
        private float _tickDelta;

        public static IReadOnlyList<PlayerVitals> All => Registry;

        public event Action<float> Damaged;
        public event Action<bool> DownedChanged;

        public bool IsAlive => !_downed.Value;
        public bool IsDowned => _downed.Value;
        public float Integrity01 => _max.Value <= 0f ? 0f : _integrity.Value / _max.Value;
        public float Chill01 => _chill.Value;
        public double DownedAt => _downedAt.Value;
        public float BleedOutSeconds => bleedOutSeconds;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Registry.Clear();

        public override void OnNetworkSpawn()
        {
            Registry.Add(this);
            _integrity.OnValueChanged += OnIntegrityChanged;
            _downed.OnValueChanged += OnDownedChanged;
            _chill.OnValueChanged += OnChillChanged;
            if (IsServer)
            {
                _model = new VitalsModel(_max.Value);
                _checkpoint = transform.position;
                _tickDelta = 1f / Mathf.Max(1, (int)NetworkManager.NetworkConfig.TickRate);
                NetworkManager.NetworkTickSystem.Tick += OnServerTick;
            }
        }

        public override void OnNetworkDespawn()
        {
            Registry.Remove(this);
            _integrity.OnValueChanged -= OnIntegrityChanged;
            _downed.OnValueChanged -= OnDownedChanged;
            _chill.OnValueChanged -= OnChillChanged;
            if (IsServer && NetworkManager != null && NetworkManager.NetworkTickSystem != null)
            {
                NetworkManager.NetworkTickSystem.Tick -= OnServerTick;
            }
        }

        public static int CountStanding()
        {
            int count = 0;
            foreach (PlayerVitals vitals in Registry)
            {
                if (vitals != null && vitals.IsAlive)
                {
                    count++;
                }
            }

            return count;
        }

        public void ServerSetMax(float maxIntegrity)
        {
            if (!IsServer || _model == null)
            {
                return;
            }

            float ratio = _model.Integrity01;
            bool downed = _model.IsDowned;
            _model.SetMax(maxIntegrity, true);
            if (!downed && ratio < 1f)
            {
                _model.ApplyDamage(maxIntegrity * (1f - ratio));
            }

            Publish();
        }

        public void ServerSetCheckpoint(Vector3 position)
        {
            if (IsServer)
            {
                _checkpoint = position;
            }
        }

        public void ServerApplyDamage(float damage, float chill, ulong attackerClientId, Vector3 hitPoint)
        {
            if (!IsServer || _model == null || _downed.Value)
            {
                return;
            }

            _lastDamageAt = NetworkManager.ServerTime.Time;
            _chill.Value = Mathf.Clamp01(_chill.Value + chill);
            if (_model.ApplyDamage(damage))
            {
                _downed.Value = true;
                _downedAt.Value = _lastDamageAt;
            }

            Publish();
        }

        public void ServerRevive()
        {
            if (!IsServer || !_downed.Value)
            {
                return;
            }

            _model.Revive(reviveFraction);
            _downed.Value = false;
            _chill.Value = 0f;
            Publish();
        }

        /// <summary>Servidor: leva o operador a um ponto (o transform é do dono, então quem move é ele).</summary>
        public void ServerTeleport(Vector3 position, Quaternion rotation) => TeleportRpc(position, rotation);

        [Rpc(SendTo.Owner)]
        private void TeleportRpc(Vector3 position, Quaternion rotation)
        {
            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            controller.enabled = wasEnabled;
        }

        private void OnServerTick()
        {
            double now = NetworkManager.ServerTime.Time;
            if (_chill.Value > 0f)
            {
                _chill.Value = Mathf.Max(0f, _chill.Value - chillRecoveryPerSecond * _tickDelta);
            }

            if (_downed.Value)
            {
                bool someoneStanding = CountStanding() > 0;
                double limit = someoneStanding ? bleedOutSeconds : soloRespawnSeconds;
                if (now - _downedAt.Value >= limit)
                {
                    _model.Revive(1f);
                    _downed.Value = false;
                    _chill.Value = 0f;
                    Publish();
                    TeleportRpc(_checkpoint, transform.rotation);
                }

                return;
            }

            if (now - _lastDamageAt >= regenDelaySeconds && _model.Integrity < _model.MaxIntegrity)
            {
                _model.Heal(regenPerSecond * _tickDelta);
                Publish();
            }
        }

        private void Publish()
        {
            _max.Value = _model.MaxIntegrity;
            _integrity.Value = _model.Integrity;
        }

        private void OnIntegrityChanged(float previous, float current)
        {
            if (current < previous)
            {
                Damaged?.Invoke(previous - current);
            }
        }

        private void OnDownedChanged(bool previous, bool downed)
        {
            if (downed)
            {
                AudioCues.Play(AudioCue.Downed, transform.position);
            }

            ApplyStatus();
            DownedChanged?.Invoke(downed);
        }

        private void OnChillChanged(float previous, float current) => ApplyStatus();

        private void ApplyStatus()
        {
            if (motor != null)
            {
                motor.StatusMultiplier = _downed.Value ? 0f : 1f - 0.55f * _chill.Value;
            }
        }
    }
}
