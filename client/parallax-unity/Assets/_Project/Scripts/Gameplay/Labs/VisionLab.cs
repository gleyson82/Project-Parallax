using Parallax.Core.Roles;
using Parallax.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Parallax.Gameplay.Labs
{
    /// <summary>
    /// Laboratório dos 3 Olhares (cena Lab_Olhares): [1] Goliath, [2] Phantom, [3] Cipher, [0] neutro,
    /// [O] Override Cego, [L] Contagem de Vazamento (0–5), [B] pulso da Batida. Só para desenvolvimento.
    /// </summary>
    public sealed class VisionLab : MonoBehaviour
    {
        [SerializeField] private VisionController vision;
        [SerializeField, Min(0.1f)] private float pulseDecayPerSecond = 1.4f;

        private float _pulse;

        private void Start()
        {
            if (vision == null)
            {
                vision = FindFirstObjectByType<VisionController>();
            }
        }

        // Ferramenta de laboratório: lê teclado direto, sem regra de jogo.
        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || vision == null)
            {
                return;
            }

            if (keyboard.digit1Key.wasPressedThisFrame) vision.SetViewer(ParallaxClass.Goliath);
            if (keyboard.digit2Key.wasPressedThisFrame) vision.SetViewer(ParallaxClass.Phantom);
            if (keyboard.digit3Key.wasPressedThisFrame) vision.SetViewer(ParallaxClass.Cipher);
            if (keyboard.digit0Key.wasPressedThisFrame) vision.SetNeutral();
            if (keyboard.oKey.wasPressedThisFrame)
            {
                vision.SetViewer(ParallaxClass.Cipher);
                ParallaxVisionState.SetOverrideBlind(!ParallaxVisionState.OverrideBlind);
            }

            if (keyboard.lKey.wasPressedThisFrame)
            {
                ParallaxVisionState.SetLeakCount((ParallaxVisionState.LeakCount + 1) % (ParallaxVisionState.MaxLeakCount + 1));
            }

            if (keyboard.bKey.wasPressedThisFrame)
            {
                _pulse = 0.92f;
            }

            _pulse = Mathf.MoveTowards(_pulse, 0.18f, pulseDecayPerSecond * Time.deltaTime);
            ParallaxVisionState.SetSeismicPulse(_pulse);
        }
    }
}
