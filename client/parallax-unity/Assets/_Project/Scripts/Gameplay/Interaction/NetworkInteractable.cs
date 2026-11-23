using System.Collections.Generic;
using Parallax.Core.Roles;
using Parallax.Gameplay.Session;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Interaction
{
    /// <summary>
    /// Base de tudo que o jogador aciona com Interagir. O cliente só pede; o servidor valida classe,
    /// distância e disponibilidade antes de executar. Prompt vazio significa "indisponível para este olhar".
    /// </summary>
    public abstract class NetworkInteractable : NetworkBehaviour
    {
        private static readonly List<NetworkInteractable> Active = new List<NetworkInteractable>();

        [SerializeField] private Transform focusPoint;
        [SerializeField, Min(0.5f)] private float range = 2.6f;

        public static IReadOnlyList<NetworkInteractable> All => Active;

        public Vector3 FocusPosition => focusPoint != null ? focusPoint.position : transform.position;
        public float Range => range;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Active.Clear();

        protected virtual void OnEnable() => Active.Add(this);

        protected virtual void OnDisable() => Active.Remove(this);

        /// <summary>Texto da ação para este olhar; vazio quando a ação não está disponível.</summary>
        public abstract string GetPrompt(ParallaxClass viewer);

        /// <summary>Segundos segurando Interagir; 0 aciona no toque.</summary>
        public virtual float GetHoldSeconds(ParallaxClass viewer) => 0f;

        /// <summary>Apresentação local (inspeção, som) no momento do acionamento, antes da resposta do servidor.</summary>
        public virtual void OnLocalInteract(ParallaxClass viewer)
        {
        }

        public void RequestInteract() => InteractRpc();

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void InteractRpc(RpcParams rpcParams = default)
        {
            ulong sender = rpcParams.Receive.SenderClientId;
            SessionRoster roster = SessionRoster.Active;
            if (roster == null || !roster.TryGetClassOf(sender, out ParallaxClass parallaxClass))
            {
                return;
            }

            if (!NetworkManager.ConnectedClients.TryGetValue(sender, out NetworkClient client) || client.PlayerObject == null)
            {
                return;
            }

            float maxDistance = range * 1.5f + 1.5f;
            if ((client.PlayerObject.transform.position - FocusPosition).sqrMagnitude > maxDistance * maxDistance)
            {
                return;
            }

            if (string.IsNullOrEmpty(GetPrompt(parallaxClass)))
            {
                return;
            }

            OnServerInteract(sender, parallaxClass, client.PlayerObject);
        }

        protected abstract void OnServerInteract(ulong clientId, ParallaxClass parallaxClass, NetworkObject player);
    }
}
