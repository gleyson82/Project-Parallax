using Parallax.Gameplay.Cameras;
using Parallax.Gameplay.Input;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Parallax.Gameplay.Players
{
    /// <summary>
    /// Olhar do jogador local: gira o alvo da câmera (yaw/pitch) com mouse ou analógico e pede a 1ª pessoa
    /// enquanto Mirar ou Inspecionar estiverem pressionados. Só roda no dono.
    /// </summary>
    public sealed class PlayerLook : NetworkBehaviour
    {
        [SerializeField] private ParallaxInput input;
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private PlayerClassBinder binder;
        [SerializeField, Min(0.01f), Tooltip("Graus por pixel de mouse.")] private float mouseSensitivity = 0.12f;
        [SerializeField, Min(1f), Tooltip("Graus por segundo no analógico.")] private float gamepadSpeed = 180f;
        [SerializeField] private Vector2 pitchLimits = new Vector2(-60f, 75f);

        private float _yaw;
        private float _pitch;
        private bool _aimHeld;
        private bool _inspectHeld;
        private bool _firstPerson;

        public float Yaw => _yaw;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                enabled = false;
                return;
            }

            _yaw = transform.eulerAngles.y;
            input.Aim.started += OnAimStarted;
            input.Aim.canceled += OnAimCanceled;
            input.Inspect.started += OnInspectStarted;
            input.Inspect.canceled += OnInspectCanceled;
            SetCursorLocked(true);
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner)
            {
                return;
            }

            input.Aim.started -= OnAimStarted;
            input.Aim.canceled -= OnAimCanceled;
            input.Inspect.started -= OnInspectStarted;
            input.Inspect.canceled -= OnInspectCanceled;
            ParallaxCameraRig rig = ParallaxCameraRig.Active;
            if (rig != null)
            {
                rig.Release(this);
            }

            SetCursorLocked(false);
        }

        public static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        // Integração de câmera do dono: lê o eixo Look e orienta o alvo, sem regra de jogo.
        private void LateUpdate()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                SetCursorLocked(Cursor.lockState != CursorLockMode.Locked);
            }

            Vector2 delta = input.Look.ReadValue<Vector2>();
            bool fromGamepad = input.Look.activeControl != null && input.Look.activeControl.device is Gamepad;
            if (fromGamepad)
            {
                delta *= gamepadSpeed * Time.deltaTime;
            }
            else if (Cursor.lockState == CursorLockMode.Locked)
            {
                delta *= mouseSensitivity;
            }
            else
            {
                delta = Vector2.zero;
            }

            _yaw += delta.x;
            _pitch = Mathf.Clamp(_pitch - delta.y, pitchLimits.x, pitchLimits.y);
            cameraTarget.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

            if (_firstPerson)
            {
                transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            }
        }

        private void OnAimStarted(InputAction.CallbackContext context) => SetHeld(ref _aimHeld, true);

        private void OnAimCanceled(InputAction.CallbackContext context) => SetHeld(ref _aimHeld, false);

        private void OnInspectStarted(InputAction.CallbackContext context) => SetHeld(ref _inspectHeld, true);

        private void OnInspectCanceled(InputAction.CallbackContext context) => SetHeld(ref _inspectHeld, false);

        private void SetHeld(ref bool flag, bool held)
        {
            flag = held;
            bool wantsFirstPerson = _aimHeld || _inspectHeld;
            if (wantsFirstPerson == _firstPerson)
            {
                return;
            }

            _firstPerson = wantsFirstPerson;
            ParallaxCameraRig rig = ParallaxCameraRig.Active;
            if (rig != null)
            {
                if (_firstPerson)
                {
                    rig.RequestFirstPerson(this);
                }
                else
                {
                    rig.Release(this);
                }
            }

            if (binder != null)
            {
                binder.SetLocalModelHidden(_firstPerson);
            }
        }
    }
}
