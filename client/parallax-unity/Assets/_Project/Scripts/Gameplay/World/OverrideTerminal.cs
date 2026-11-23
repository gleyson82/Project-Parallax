using System;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.World
{
    /// <summary>
    /// Terminal, trava lógica ou máquina invadível pelo Cipher. Durante a invasão, o Cipher fica cego para o
    /// mundo físico; a Janela de Sincronia abre a fechadura lógica no momento certo.
    /// </summary>
    public sealed class OverrideTerminal : NetworkBehaviour
    {
        [SerializeField, Min(0.1f)] private float breachSeconds = 4f;
        [SerializeField, Tooltip("Máquina que vira aliada se o Cipher carrega o Código-Fonte Ômega.")]
        private bool infectable;

        private readonly NetworkVariable<bool> _breached = new NetworkVariable<bool>();

        public event Action<bool> BreachedChanged;

        public float BreachSeconds => breachSeconds;
        public bool Infectable => infectable;
        public bool IsBreached => _breached.Value;

        public override void OnNetworkSpawn() => _breached.OnValueChanged += OnBreachedChanged;

        public override void OnNetworkDespawn() => _breached.OnValueChanged -= OnBreachedChanged;

        public void ServerSetBreached(bool breached)
        {
            if (IsServer)
            {
                _breached.Value = breached;
            }
        }

        private void OnBreachedChanged(bool previous, bool current) => BreachedChanged?.Invoke(current);
    }
}
