using System.Collections.Generic;
using Parallax.Core.Roles;
using Parallax.Gameplay.Audio;
using Parallax.Gameplay.Combat;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Interaction
{
    /// <summary>
    /// Ponto de Brasa (checkpoint): registra o retorno do operador e é o único lugar onde se troca de classe
    /// (escalabilidade-1-a-3: "em sessão solo, o jogador troca de classe nos Pontos de Brasa").
    /// </summary>
    public sealed class BrasaPoint : NetworkInteractable
    {
        private static readonly List<BrasaPoint> Points = new List<BrasaPoint>();

        [SerializeField] private Transform respawnAnchor;
        [SerializeField, Min(1f)] private float classSwapRadius = 4f;

        public Vector3 RespawnPosition => respawnAnchor != null ? respawnAnchor.position : transform.position + transform.forward * 1.5f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetPoints() => Points.Clear();

        protected override void OnEnable()
        {
            base.OnEnable();
            Points.Add(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Points.Remove(this);
        }

        /// <summary>Troca de classe só perto de um Ponto de Brasa. Sem pontos na cena (lab), libera.</summary>
        public static bool AllowsClassSwapAt(Vector3 position)
        {
            if (Points.Count == 0)
            {
                return true;
            }

            foreach (BrasaPoint point in Points)
            {
                if ((point.transform.position - position).sqrMagnitude <= point.classSwapRadius * point.classSwapRadius)
                {
                    return true;
                }
            }

            return false;
        }

        public static BrasaPoint Nearest(Vector3 position)
        {
            BrasaPoint best = null;
            float bestDistance = float.MaxValue;
            foreach (BrasaPoint point in Points)
            {
                float distance = (point.transform.position - position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = point;
                }
            }

            return best;
        }

        public override string GetPrompt(ParallaxClass viewer) => "Ponto de Brasa · registrar retorno";

        public override void OnLocalInteract(ParallaxClass viewer) => AudioCues.Play(AudioCue.Pickup, FocusPosition);

        protected override void OnServerInteract(ulong clientId, ParallaxClass parallaxClass, NetworkObject player)
        {
            PlayerVitals vitals = player.GetComponent<PlayerVitals>();
            if (vitals != null)
            {
                vitals.ServerSetCheckpoint(RespawnPosition);
            }
        }
    }
}
