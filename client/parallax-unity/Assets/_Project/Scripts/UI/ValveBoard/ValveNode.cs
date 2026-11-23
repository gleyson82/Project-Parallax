using System.Collections;
using Parallax.Core.Definitions;
using UnityEngine;

namespace Parallax.UI.ValveBoard
{
    /// <summary>Uma válvula física no Quadro: cada missão é um volante que o jogador abre.</summary>
    public sealed class ValveNode : MonoBehaviour
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private MissionDefinition mission;
        [SerializeField] private Transform wheel;
        [SerializeField] private Renderer highlightRenderer;
        [SerializeField] private Color selectedEmission = new Color(1.0f, 0.55f, 0.2f) * 2f;
        [SerializeField, Min(0.1f)] private float openSeconds = 1.6f;

        private MaterialPropertyBlock _block;

        public MissionDefinition Mission => mission;

        public void SetSelected(bool selected)
        {
            if (highlightRenderer == null)
            {
                return;
            }

            _block ??= new MaterialPropertyBlock();
            highlightRenderer.GetPropertyBlock(_block);
            _block.SetColor(EmissionColorId, selected ? selectedEmission : Color.black);
            highlightRenderer.SetPropertyBlock(_block);
        }

        /// <summary>O volante gira uma volta e meia e trava.</summary>
        public Coroutine PlayOpen() => StartCoroutine(OpenRoutine());

        /// <summary>Progresso das Três Mãos aplicado ao volante (0–1).</summary>
        public void SetTurnProgress(float progress)
        {
            if (wheel != null)
            {
                wheel.localRotation = Quaternion.Euler(0f, 0f, -90f * Mathf.Clamp01(progress));
            }
        }

        private IEnumerator OpenRoutine()
        {
            if (wheel == null)
            {
                yield break;
            }

            Quaternion start = wheel.localRotation;
            float elapsed = 0f;
            while (elapsed < openSeconds)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / openSeconds);
                wheel.localRotation = start * Quaternion.Euler(0f, 0f, -540f * t);
                yield return null;
            }
        }
    }
}
