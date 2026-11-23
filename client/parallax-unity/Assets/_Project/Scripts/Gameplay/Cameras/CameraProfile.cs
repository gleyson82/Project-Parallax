using UnityEngine;

namespace Parallax.Gameplay.Cameras
{
    /// <summary>Enquadramento de 3ª pessoa por classe. Goliath mais aberto; Phantom com viés vertical.</summary>
    [CreateAssetMenu(menuName = "Parallax/Camera Profile", fileName = "CameraProfile_")]
    public sealed class CameraProfile : ScriptableObject
    {
        [Header("3ª pessoa")]
        [SerializeField] private Vector3 shoulderOffset = new Vector3(0.6f, 0.2f, 0f);
        [SerializeField, Min(0f)] private float verticalArmLength = 0.4f;
        [SerializeField, Min(0.5f)] private float cameraDistance = 4f;
        [SerializeField, Range(20f, 100f)] private float thirdPersonFov = 55f;

        [Header("1ª pessoa contextual (leitura de manômetro, mira, terminal)")]
        [SerializeField, Range(20f, 100f)] private float firstPersonFov = 65f;
        [SerializeField, Range(-80f, 0f)] private float minTilt = -70f;
        [SerializeField, Range(0f, 80f)] private float maxTilt = 70f;

        public Vector3 ShoulderOffset => shoulderOffset;
        public float VerticalArmLength => verticalArmLength;
        public float CameraDistance => cameraDistance;
        public float ThirdPersonFov => thirdPersonFov;
        public float FirstPersonFov => firstPersonFov;
        public float MinTilt => minTilt;
        public float MaxTilt => maxTilt;

#if UNITY_EDITOR
        public void EditorSetup(Vector3 shoulder, float arm, float distance, float tpFov, float fpFov)
        {
            shoulderOffset = shoulder;
            verticalArmLength = arm;
            cameraDistance = distance;
            thirdPersonFov = tpFov;
            firstPersonFov = fpFov;
        }
#endif
    }
}
