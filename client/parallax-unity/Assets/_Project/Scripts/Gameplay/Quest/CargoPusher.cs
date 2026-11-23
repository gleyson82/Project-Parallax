using Parallax.Core.Roles;
using Parallax.Gameplay.Players;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Quest
{
    /// <summary>O dono Goliath empurra o contêiner andando contra ele; o servidor confirma a classe.</summary>
    public sealed class CargoPusher : NetworkBehaviour
    {
        [SerializeField] private PlayerClassBinder binder;
        [SerializeField, Min(0.02f)] private float requestInterval = 0.12f;

        private float _nextRequest;

        // Contato físico do CharacterController do dono (só gera pedido; a regra é do servidor).
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!IsOwner || !binder.HasClass || binder.CurrentClass != ParallaxClass.Goliath || Time.time < _nextRequest)
            {
                return;
            }

            PushableCargo cargo = hit.collider.GetComponentInParent<PushableCargo>();
            if (cargo == null || !cargo.IsActive || cargo.IsDelivered)
            {
                return;
            }

            float along = Vector3.Dot(hit.moveDirection, Vector3.right);
            if (Mathf.Abs(along) < 0.3f)
            {
                return;
            }

            _nextRequest = Time.time + requestInterval;
            cargo.RequestPush(along);
        }
    }
}
