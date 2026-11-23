using Parallax.Core.Definitions;
using Parallax.Gameplay.Input;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Characters
{
    /// <summary>
    /// Motor de personagem com CharacterController, controlado pelo dono (NetworkTransform com autoridade do dono).
    /// Update aqui só integra entrada e movimento; regras de jogo vivem em eventos, RPCs e no tick de rede.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class ParallaxCharacterMotor : NetworkBehaviour
    {
        [SerializeField] private ClassDefinition classDefinition;
        [SerializeField] private ParallaxInput input;
        [SerializeField] private Transform cameraYawSource;
        [SerializeField, Min(0f)] private float jumpHeight = 1.1f;
        [SerializeField, Min(0f)] private float turnSpeedDegrees = 540f;

        private CharacterController _controller;
        private Vector3 _verticalVelocity;
        private Vector3 _externalVelocity;

        /// <summary>0 trava o deslocamento (Goliath ancorado, Cipher em Override); 1 é a velocidade da classe.</summary>
        public float SpeedMultiplier { get; set; } = 1f;

        /// <summary>Efeitos de estado (frio da espuma criogênica, operador caído), independentes das habilidades.</summary>
        public float StatusMultiplier { get; set; } = 1f;

        /// <summary>Quando verdadeiro, gravidade e entrada são ignoradas (puxada de gancho, Override cego).</summary>
        public bool ExternallyDriven { get; set; }

        public ClassDefinition ClassDefinition => classDefinition;
        public bool IsGrounded => _controller != null && _controller.isGrounded;
        public Vector3 Velocity => _controller != null ? _controller.velocity : Vector3.zero;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        public void SetExternalVelocity(Vector3 velocity) => _externalVelocity = velocity;

        public void SetClassDefinition(ClassDefinition definition) => classDefinition = definition;

        public void SetCameraYawSource(Transform source) => cameraYawSource = source;

        private void Update()
        {
            if (!IsSpawned || !IsOwner || input == null)
            {
                return;
            }

            float dt = Time.deltaTime;
            if (ExternallyDriven)
            {
                _controller.Move(_externalVelocity * dt);
                _verticalVelocity = Vector3.zero;
                return;
            }

            Vector2 move = input.Move.ReadValue<Vector2>();
            Vector3 forward = cameraYawSource != null ? Vector3.ProjectOnPlane(cameraYawSource.forward, Vector3.up).normalized : Vector3.forward;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            float speed = (classDefinition != null ? classDefinition.MoveSpeed : 4.5f) * SpeedMultiplier * StatusMultiplier;
            Vector3 planar = (forward * move.y + right * move.x) * speed;

            if (planar.sqrMagnitude > 0.0001f)
            {
                Quaternion target = Quaternion.LookRotation(planar.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeedDegrees * dt);
            }

            if (_controller.isGrounded)
            {
                _verticalVelocity.y = -1f;
                if (input.Jump.WasPressedThisFrame() && SpeedMultiplier > 0f && StatusMultiplier > 0.2f)
                {
                    _verticalVelocity.y = Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y);
                }
            }
            else
            {
                _verticalVelocity += Physics.gravity * dt;
            }

            _controller.Move((planar + _verticalVelocity + _externalVelocity) * dt);
        }
    }
}
