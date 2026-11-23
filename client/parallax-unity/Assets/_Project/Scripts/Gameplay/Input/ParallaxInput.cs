using UnityEngine;
using UnityEngine.InputSystem;

namespace Parallax.Gameplay.Input
{
    /// <summary>
    /// Ponto único de acesso às ações de ParallaxControls.inputactions. Componentes de jogo leem daqui
    /// em vez de consultar teclado ou controle diretamente.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public sealed class ParallaxInput : MonoBehaviour
    {
        [SerializeField] private InputActionAsset actionsAsset;

        // Cópia por instância: habilitar ou desabilitar um jogador não pode afetar o outro.
        private InputActionAsset actions;

        public InputAction Move { get; private set; }
        public InputAction Look { get; private set; }
        public InputAction Jump { get; private set; }
        public InputAction Interact { get; private set; }
        public InputAction Ability { get; private set; }
        public InputAction Fire { get; private set; }
        public InputAction Aim { get; private set; }
        public InputAction Inspect { get; private set; }
        public InputAction BoardNavigate { get; private set; }
        public InputAction BoardConfirm { get; private set; }
        public InputAction BoardCancel { get; private set; }
        public InputAction BoardSwitchLayer { get; private set; }

        public InputActionAsset Actions => actions;

        private void Awake()
        {
            if (actionsAsset == null)
            {
                Debug.LogError("[Parallax/Input] ParallaxControls.inputactions não foi atribuído.", this);
                enabled = false;
                return;
            }

            actions = Instantiate(actionsAsset);

            Move = actions.FindAction("Player/Move", true);
            Look = actions.FindAction("Player/Look", true);
            Jump = actions.FindAction("Player/Jump", true);
            Interact = actions.FindAction("Player/Interact", true);
            Ability = actions.FindAction("Player/Ability", true);
            Fire = actions.FindAction("Player/Fire", true);
            Aim = actions.FindAction("Player/Aim", true);
            Inspect = actions.FindAction("Player/Inspect", true);
            BoardNavigate = actions.FindAction("Board/Navigate", true);
            BoardConfirm = actions.FindAction("Board/Confirm", true);
            BoardCancel = actions.FindAction("Board/Cancel", true);
            BoardSwitchLayer = actions.FindAction("Board/SwitchLayer", true);
        }

        private void OnEnable()
        {
            if (actions != null)
            {
                actions.Enable();
            }
        }

        private void OnDisable()
        {
            if (actions != null)
            {
                actions.Disable();
            }
        }

        private void OnDestroy()
        {
            if (actions != null)
            {
                Destroy(actions);
            }
        }

        public void UsePlayerMap()
        {
            actions.FindActionMap("Board", true).Disable();
            actions.FindActionMap("Player", true).Enable();
        }

        public void UseBoardMap()
        {
            actions.FindActionMap("Player", true).Disable();
            actions.FindActionMap("Board", true).Enable();
        }
    }
}
