using System;
using System.Collections.Generic;
using Parallax.Core.Roles;
using Parallax.Core.Session;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Session
{
    public struct SeatClaim : INetworkSerializeByMemcpy, IEquatable<SeatClaim>
    {
        public ulong ClientId;
        public ParallaxClass Class;

        public bool Equals(SeatClaim other) => ClientId == other.ClientId && Class == other.Class;

        public override bool Equals(object obj) => obj is SeatClaim other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(ClientId, (byte)Class);
    }

    /// <summary>
    /// Quem ocupa qual classe. Nenhuma classe se repete; os papéis vazios viram Auxiliares.
    /// Em sessão solo, a troca de classe só é aceita em Ponto de Brasa (checkpoint).
    /// </summary>
    public sealed class SessionRoster : NetworkBehaviour
    {
        private NetworkList<SeatClaim> _claims;

        public static SessionRoster Active { get; private set; }

        public event Action RosterChanged;

        public int HumanCount => _claims?.Count ?? 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Active = null;

        private void Awake()
        {
            _claims = new NetworkList<SeatClaim>();
        }

        public override void OnNetworkSpawn()
        {
            Active = this;
            _claims.OnListChanged += OnClaimsChanged;
            if (IsServer)
            {
                NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }

        public override void OnNetworkDespawn()
        {
            _claims.OnListChanged -= OnClaimsChanged;
            if (IsServer && NetworkManager != null)
            {
                NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            }

            if (Active == this)
            {
                Active = null;
            }
        }

        public override void OnDestroy()
        {
            _claims?.Dispose();
            base.OnDestroy();
        }

        public bool IsHuman(ParallaxClass parallaxClass)
        {
            foreach (SeatClaim claim in _claims)
            {
                if (claim.Class == parallaxClass)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetClassOf(ulong clientId, out ParallaxClass parallaxClass)
        {
            foreach (SeatClaim claim in _claims)
            {
                if (claim.ClientId == clientId)
                {
                    parallaxClass = claim.Class;
                    return true;
                }
            }

            parallaxClass = default;
            return false;
        }

        public void CopyHumanClasses(List<ParallaxClass> buffer)
        {
            buffer.Clear();
            foreach (SeatClaim claim in _claims)
            {
                buffer.Add(claim.Class);
            }
        }

        public int AuxiliariesNeeded => SessionScaling.AuxiliariesNeeded(Math.Max(1, HumanCount));

        public void RequestClass(ParallaxClass parallaxClass) => RequestClassRpc(parallaxClass);

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestClassRpc(ParallaxClass parallaxClass, RpcParams rpcParams = default)
        {
            ulong sender = rpcParams.Receive.SenderClientId;
            for (int i = 0; i < _claims.Count; i++)
            {
                if (_claims[i].Class == parallaxClass && _claims[i].ClientId != sender)
                {
                    return;
                }
            }

            var claim = new SeatClaim { ClientId = sender, Class = parallaxClass };
            for (int i = 0; i < _claims.Count; i++)
            {
                if (_claims[i].ClientId == sender)
                {
                    // Troca no lugar: os clientes nunca observam o jogador sem classe no meio da troca.
                    _claims[i] = claim;
                    return;
                }
            }

            _claims.Add(claim);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            for (int i = _claims.Count - 1; i >= 0; i--)
            {
                if (_claims[i].ClientId == clientId)
                {
                    _claims.RemoveAt(i);
                }
            }
        }

        private void OnClaimsChanged(NetworkListEvent<SeatClaim> change) => RosterChanged?.Invoke();
    }
}
