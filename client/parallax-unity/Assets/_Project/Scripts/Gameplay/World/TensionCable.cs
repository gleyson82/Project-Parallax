using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.World
{
    /// <summary>
    /// Cabo de tensão preso pelo Phantom entre uma plataforma suspensa e um ponto de arpéu.
    /// Junta configurável com limite linear: o cabo segura, mas não empurra. Criado só no servidor.
    /// </summary>
    public sealed class TensionCable : NetworkBehaviour
    {
        [SerializeField] private Rigidbody platform;
        [SerializeField, Min(0f)] private float slackMeters = 0.15f;
        [SerializeField, Min(0f)] private float breakForce = 4.0e6f;

        private ConfigurableJoint _joint;

        public bool IsAttached => _joint != null;

        public bool ServerAttach(GrapplePoint point)
        {
            if (!IsServer || platform == null || point == null || !point.AcceptsTensionCable)
            {
                return false;
            }

            ServerDetach();
            Vector3 anchorWorld = point.AnchorPosition;
            _joint = platform.gameObject.AddComponent<ConfigurableJoint>();
            _joint.autoConfigureConnectedAnchor = false;
            _joint.anchor = platform.transform.InverseTransformPoint(transform.position);
            _joint.connectedAnchor = anchorWorld;
            _joint.xMotion = ConfigurableJointMotion.Limited;
            _joint.yMotion = ConfigurableJointMotion.Limited;
            _joint.zMotion = ConfigurableJointMotion.Limited;
            _joint.linearLimit = new SoftJointLimit { limit = Vector3.Distance(transform.position, anchorWorld) + slackMeters };
            _joint.breakForce = breakForce;
            return true;
        }

        public void ServerDetach()
        {
            if (_joint != null)
            {
                Destroy(_joint);
                _joint = null;
            }
        }
    }
}
