using Parallax.Gameplay.Input;
using Parallax.Gameplay.World;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Parallax.Gameplay.Characters
{
    /// <summary>
    /// Phantom: arpéu 360° até pontos altos (mobilidade vertical) e cabo de tensão para prender plataformas.
    /// A puxada é do dono (movimento); o cabo físico é criado no servidor.
    /// </summary>
    public sealed class PhantomGrapple : NetworkBehaviour
    {
        [SerializeField] private ParallaxInput input;
        [SerializeField] private ParallaxCharacterMotor motor;
        [SerializeField] private Transform aimSource;
        [SerializeField, Min(1f)] private float maxRange = 28f;
        [SerializeField, Range(1f, 90f)] private float aimConeDegrees = 25f;
        [SerializeField, Min(1f)] private float pullSpeed = 16f;
        [SerializeField, Min(0.1f)] private float arriveDistance = 1.2f;

        private GrapplePoint _target;

        public bool IsPulling => _target != null;

        public override void OnNetworkSpawn()
        {
            if (IsOwner && input != null)
            {
                input.Ability.performed += OnAbility;
                input.Interact.performed += OnInteract;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (input != null)
            {
                input.Ability.performed -= OnAbility;
                input.Interact.performed -= OnInteract;
            }
        }

        // Integração de movimento da puxada (dono). Termina por distância, sem regra de jogo.
        private void Update()
        {
            if (!IsOwner || _target == null)
            {
                return;
            }

            Vector3 toTarget = _target.AnchorPosition - transform.position;
            if (toTarget.magnitude <= arriveDistance)
            {
                EndPull();
                return;
            }

            motor.SetExternalVelocity(toTarget.normalized * pullSpeed);
        }

        private void OnDisable()
        {
            if (_target != null)
            {
                EndPull();
            }
        }

        private void OnAbility(InputAction.CallbackContext context)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (_target != null)
            {
                EndPull();
                return;
            }

            Transform source = aimSource != null ? aimSource : transform;
            _target = GrapplePoint.FindBest(source.position, source.forward, maxRange, aimConeDegrees);
            if (_target != null)
            {
                motor.ExternallyDriven = true;
            }
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            Transform source = aimSource != null ? aimSource : transform;
            GrapplePoint anchor = GrapplePoint.FindBest(source.position, source.forward, maxRange, aimConeDegrees);
            if (anchor == null)
            {
                return;
            }

            Collider[] near = Physics.OverlapSphere(transform.position, 3f);
            foreach (Collider candidate in near)
            {
                TensionCable cable = candidate.GetComponentInParent<TensionCable>();
                if (cable != null && cable.NetworkObject != null)
                {
                    AttachCableRpc(cable.NetworkObject, anchor.transform.position);
                    return;
                }
            }
        }

        [Rpc(SendTo.Server)]
        private void AttachCableRpc(NetworkObjectReference cableReference, Vector3 anchorHint)
        {
            if (!cableReference.TryGet(out NetworkObject cableObject))
            {
                return;
            }

            TensionCable cable = cableObject.GetComponentInChildren<TensionCable>();
            GrapplePoint anchor = GrapplePoint.FindBest(anchorHint + Vector3.down, Vector3.up, 2f, 90f);
            if (cable != null && anchor != null)
            {
                cable.ServerAttach(anchor);
            }
        }

        private void EndPull()
        {
            _target = null;
            motor.SetExternalVelocity(Vector3.zero);
            motor.ExternallyDriven = false;
        }
    }
}
