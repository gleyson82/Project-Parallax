using System.Collections.Generic;
using UnityEngine;

namespace Parallax.Gameplay.World
{
    /// <summary>
    /// Linha Viva: duto pressurizado com faixa amarela. Doutrina do Fogo Mínimo: armas do Sindicato travam
    /// (clique audível) ao mirar perto dela, e disparos do jogador que a atingem rompem o bônus de Fogo Mínimo.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class LiveLine : MonoBehaviour
    {
        private static readonly List<LiveLine> Active = new List<LiveLine>();

        [SerializeField, Min(0f), Tooltip("Raio da zona de Fogo Proibido em volta do duto.")]
        private float lockRadius = 4.5f;

        private Collider _collider;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Active.Clear();

        private void Awake() => _collider = GetComponent<Collider>();

        private void OnEnable() => Active.Add(this);

        private void OnDisable() => Active.Remove(this);

        /// <summary>Verdadeiro se a posição (mira ou cano) está numa zona de Fogo Proibido.</summary>
        public static bool IsInForbiddenFireZone(Vector3 position)
        {
            for (int i = 0; i < Active.Count; i++)
            {
                LiveLine line = Active[i];
                Vector3 closest = line._collider.ClosestPoint(position);
                if ((closest - position).sqrMagnitude <= line.lockRadius * line.lockRadius)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
