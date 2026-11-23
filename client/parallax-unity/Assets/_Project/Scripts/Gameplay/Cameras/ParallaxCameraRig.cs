using System.Collections.Generic;
using Parallax.Rendering;
using Unity.Cinemachine;
using UnityEngine;

namespace Parallax.Gameplay.Cameras
{
    /// <summary>
    /// Câmera local do jogador: 3ª pessoa por padrão e 1ª pessoa contextual com blend do CinemachineBrain.
    /// Qualquer sistema pede a 1ª pessoa com RequestFirstPerson(this) e devolve com Release(this).
    /// Existe um rig por máquina (na cena); o jogador local se liga a ele ao nascer.
    /// </summary>
    public sealed class ParallaxCameraRig : MonoBehaviour
    {
        private const int InactivePriority = 0;
        private const int ThirdPersonPriority = 10;
        private const int FirstPersonPriority = 20;

        [SerializeField] private CinemachineCamera thirdPersonCamera;
        [SerializeField] private CinemachineThirdPersonFollow thirdPersonFollow;
        [SerializeField] private CinemachineCamera firstPersonCamera;
        [SerializeField] private CinemachinePanTilt firstPersonPanTilt;
        [SerializeField] private VisionController vision;
        [SerializeField] private CameraProfile profile;

        private readonly HashSet<Object> _firstPersonRequests = new HashSet<Object>();

        public static ParallaxCameraRig Active { get; private set; }

        public bool IsFirstPerson => _firstPersonRequests.Count > 0;
        public VisionController Vision => vision;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Active = null;

        private void Awake()
        {
            if (profile != null)
            {
                ApplyProfile(profile);
            }

            Refresh();
        }

        private void OnEnable() => Active = this;

        private void OnDisable()
        {
            if (Active == this)
            {
                Active = null;
            }
        }

        /// <param name="body">Alvo que a 3ª pessoa acompanha (ombro); a rotação dele orienta a câmera.</param>
        /// <param name="eyes">Ponto de vista da 1ª pessoa (capacete, visor, viseira).</param>
        public void Bind(Transform body, Transform eyes)
        {
            thirdPersonCamera.Target.TrackingTarget = body;
            firstPersonCamera.Target.TrackingTarget = eyes != null ? eyes : body;
        }

        public void ApplyProfile(CameraProfile newProfile)
        {
            if (newProfile == null)
            {
                return;
            }

            profile = newProfile;
            if (thirdPersonFollow != null)
            {
                thirdPersonFollow.ShoulderOffset = newProfile.ShoulderOffset;
                thirdPersonFollow.VerticalArmLength = newProfile.VerticalArmLength;
                thirdPersonFollow.CameraDistance = newProfile.CameraDistance;
            }

            thirdPersonCamera.Lens.FieldOfView = newProfile.ThirdPersonFov;
            firstPersonCamera.Lens.FieldOfView = newProfile.FirstPersonFov;
            if (firstPersonPanTilt != null)
            {
                firstPersonPanTilt.TiltAxis.Range = new Vector2(newProfile.MinTilt, newProfile.MaxTilt);
            }
        }

        public void RequestFirstPerson(Object requester)
        {
            if (_firstPersonRequests.Add(requester))
            {
                Refresh();
            }
        }

        public void Release(Object requester)
        {
            if (_firstPersonRequests.Remove(requester))
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            _firstPersonRequests.RemoveWhere(r => r == null);
            thirdPersonCamera.Priority = ThirdPersonPriority;
            firstPersonCamera.Priority = IsFirstPerson ? FirstPersonPriority : InactivePriority;
        }
    }
}
