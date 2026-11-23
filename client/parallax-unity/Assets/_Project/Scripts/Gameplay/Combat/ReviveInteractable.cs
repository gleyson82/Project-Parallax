using Parallax.Core.Roles;
using Parallax.Gameplay.Interaction;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Combat
{
    /// <summary>Resgate: um aliado segura Interagir junto do operador caído.</summary>
    public sealed class ReviveInteractable : NetworkInteractable
    {
        [SerializeField] private PlayerVitals vitals;
        [SerializeField, Min(0.5f)] private float holdSeconds = 2.5f;

        public override string GetPrompt(ParallaxClass viewer)
        {
            if (vitals == null || !vitals.IsDowned || IsOwner)
            {
                return string.Empty;
            }

            return "Segurar · resgatar operador";
        }

        public override float GetHoldSeconds(ParallaxClass viewer) => holdSeconds;

        protected override void OnServerInteract(ulong clientId, ParallaxClass parallaxClass, NetworkObject player)
        {
            if (clientId != OwnerClientId)
            {
                vitals.ServerRevive();
            }
        }
    }
}
