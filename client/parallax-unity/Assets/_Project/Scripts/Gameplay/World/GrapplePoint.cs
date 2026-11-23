using System.Collections.Generic;
using UnityEngine;

namespace Parallax.Gameplay.World
{
    /// <summary>
    /// Ponto de arpéu do Phantom (vigas, beirais, mastros). Use a Rendering Layer "GrapplePoint" na malha
    /// para o realce ciano do Olhar do Phantom.
    /// </summary>
    public sealed class GrapplePoint : MonoBehaviour
    {
        private static readonly List<GrapplePoint> Active = new List<GrapplePoint>();

        [SerializeField] private Transform anchor;
        [SerializeField, Tooltip("Aceita cabo de tensão para segurar plataformas (Maré Baixa).")]
        private bool acceptsTensionCable = true;

        public Vector3 AnchorPosition => anchor != null ? anchor.position : transform.position;
        public bool AcceptsTensionCable => acceptsTensionCable;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Active.Clear();

        private void OnEnable() => Active.Add(this);

        private void OnDisable() => Active.Remove(this);

        /// <summary>Melhor ponto dentro do cone de mira e do alcance do cabo.</summary>
        public static GrapplePoint FindBest(Vector3 origin, Vector3 aimDirection, float maxRange, float maxAngleDegrees)
        {
            GrapplePoint best = null;
            float bestScore = float.MaxValue;
            float cosLimit = Mathf.Cos(maxAngleDegrees * Mathf.Deg2Rad);
            for (int i = 0; i < Active.Count; i++)
            {
                Vector3 toPoint = Active[i].AnchorPosition - origin;
                float distance = toPoint.magnitude;
                if (distance > maxRange || distance < 0.5f)
                {
                    continue;
                }

                float cos = Vector3.Dot(aimDirection.normalized, toPoint / distance);
                if (cos < cosLimit)
                {
                    continue;
                }

                float score = distance * (2f - cos);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = Active[i];
                }
            }

            return best;
        }
    }
}
