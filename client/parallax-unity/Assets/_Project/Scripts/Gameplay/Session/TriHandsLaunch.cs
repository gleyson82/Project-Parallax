using System;
using Parallax.Core.Commands;
using Parallax.Core.Roles;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Session
{
    /// <summary>
    /// "Três Mãos" em rede. Cada humano segura confirmar; o servidor valida a simultaneidade no tick de rede.
    /// As Mãos de papéis vazios aparecem como Auxiliares, já prontas.
    /// </summary>
    public sealed class TriHandsLaunch : NetworkBehaviour
    {
        [SerializeField, Range(0.1f, 3f), Tooltip("Acessibilidade: tempo de segurar ajustável.")]
        private float holdSeconds = (float)TriHandsConsensus.DefaultHoldSeconds;

        private readonly NetworkVariable<float> _progress = new NetworkVariable<float>();
        private readonly NetworkVariable<byte> _pressedMask = new NetworkVariable<byte>();
        private TriHandsConsensus _consensus;
        private SessionRoster _roster;
        private int _pendingSelection = -1;

        /// <summary>Índice da válvula (missão) confirmada pelas Três Mãos.</summary>
        public event Action<int> LaunchConfirmed;

        public float Progress => _progress.Value;

        public bool IsHandPressed(ParallaxClass seat) => (_pressedMask.Value & (1 << (int)seat)) != 0;

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                return;
            }

            _consensus = new TriHandsConsensus(holdSeconds);
            _roster = SessionRoster.Active;
            NetworkManager.NetworkTickSystem.Tick += OnServerTick;
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && NetworkManager != null && NetworkManager.NetworkTickSystem != null)
            {
                NetworkManager.NetworkTickSystem.Tick -= OnServerTick;
            }
        }

        public void Press(int selectedValve) => PressRpc(selectedValve);

        public void Release() => ReleaseRpc();

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void PressRpc(int selectedValve, RpcParams rpcParams = default)
        {
            if (!TryGetSeat(rpcParams.Receive.SenderClientId, out ParallaxClass seat))
            {
                return;
            }

            _pendingSelection = selectedValve;
            _consensus.Press(seat, NetworkManager.ServerTime.Time);
            _pressedMask.Value = (byte)(_pressedMask.Value | (1 << (int)seat));
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ReleaseRpc(RpcParams rpcParams = default)
        {
            if (!TryGetSeat(rpcParams.Receive.SenderClientId, out ParallaxClass seat))
            {
                return;
            }

            _consensus.Release(seat);
            _pressedMask.Value = (byte)(_pressedMask.Value & ~(1 << (int)seat));
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void LaunchConfirmedRpc(int selectedValve) => LaunchConfirmed?.Invoke(selectedValve);

        private void OnServerTick()
        {
            if (_roster == null)
            {
                _roster = SessionRoster.Active;
                if (_roster == null)
                {
                    return;
                }
            }

            for (int i = 0; i < ParallaxRoles.ClassCount; i++)
            {
                _consensus.SetHuman((ParallaxClass)i, _roster.IsHuman((ParallaxClass)i));
            }

            double now = NetworkManager.ServerTime.Time;
            _progress.Value = _consensus.Progress(now);
            if (_consensus.IsComplete(now) && _pendingSelection >= 0)
            {
                LaunchConfirmedRpc(_pendingSelection);
                _consensus.ResetPresses();
                _pressedMask.Value = 0;
                _pendingSelection = -1;
            }
        }

        private bool TryGetSeat(ulong clientId, out ParallaxClass seat)
        {
            seat = default;
            if (_roster == null)
            {
                _roster = SessionRoster.Active;
            }

            return _roster != null && _roster.TryGetClassOf(clientId, out seat);
        }
    }
}
