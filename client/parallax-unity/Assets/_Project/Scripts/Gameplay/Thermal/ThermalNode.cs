using System;
using Parallax.Core.Definitions;
using Parallax.Core.Thermal;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Thermal
{
    /// <summary>
    /// Componente termodinâmico de jogadores, armas, inimigos, props e salas. O servidor simula
    /// (ThermalSystem, por tick de rede); os clientes recebem a temperatura replicada e os eventos por RPC.
    /// </summary>
    public sealed class ThermalNode : NetworkBehaviour
    {
        [SerializeField] private ThermalProfile profile;

        private readonly NetworkVariable<float> _kelvin = new NetworkVariable<float>(ThermalConstants.TritonAmbientKelvin);
        private ThermalModel _model;

        public event Action<float, float> TemperatureChanged;
        public event Action Overheated;
        public event Action Frozen;
        public event Action<float> ThermalShock;

        public float CurrentKelvin => _kelvin.Value;
        public float NormalizedHeat => _model?.NormalizedHeat ?? 0f;
        public bool IsOverheated => _model != null && _model.IsOverheated;
        public bool IsFrozen => _model != null && _model.IsFrozen;

        private void Awake()
        {
            ThermalSettings settings = profile != null ? profile.ToSettings() : ThermalSettings.Default;
            float initial = profile != null ? profile.InitialKelvin : 290f;
            _model = new ThermalModel(settings, initial);
        }

        public override void OnNetworkSpawn()
        {
            _kelvin.OnValueChanged += OnKelvinReplicated;
            if (IsServer)
            {
                _kelvin.Value = _model.CurrentKelvin;
                ThermalSystem.Register(this);
            }
        }

        public override void OnNetworkDespawn()
        {
            _kelvin.OnValueChanged -= OnKelvinReplicated;
            ThermalSystem.Unregister(this);
        }

        public void ApplyHeat(float deltaKelvin)
        {
            if (IsServer)
            {
                Publish(_model.ApplyHeat(deltaKelvin));
            }
        }

        public void ApplyCryo(float deltaKelvin)
        {
            if (IsServer)
            {
                Publish(_model.ApplyCryo(deltaKelvin));
            }
        }

        internal void ServerTick(float deltaSeconds) => Publish(_model.Step(deltaSeconds));

        private void Publish(ThermalEvents events)
        {
            if ((events & ThermalEvents.Changed) != 0)
            {
                _kelvin.Value = _model.CurrentKelvin;
            }

            if ((events & (ThermalEvents.BecameOverheated | ThermalEvents.BecameFrozen | ThermalEvents.ThermalShock)) != 0)
            {
                NotifyThermalEventsRpc((byte)events, _model.LastShockDropKelvin);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void NotifyThermalEventsRpc(byte events, float shockDrop)
        {
            var flags = (ThermalEvents)events;
            if ((flags & ThermalEvents.BecameOverheated) != 0)
            {
                Overheated?.Invoke();
            }

            if ((flags & ThermalEvents.BecameFrozen) != 0)
            {
                Frozen?.Invoke();
            }

            if ((flags & ThermalEvents.ThermalShock) != 0)
            {
                ThermalShock?.Invoke(shockDrop);
            }
        }

        private void OnKelvinReplicated(float previous, float current)
        {
            if (!IsServer)
            {
                _model = new ThermalModel(_model.Settings, current);
            }

            TemperatureChanged?.Invoke(current, NormalizedHeat);
        }
    }
}
