using Parallax.Core.Roles;
using UnityEngine;

namespace Parallax.Core.Definitions
{
    [CreateAssetMenu(menuName = "Parallax/Definitions/Relic (Grau-Zero)", fileName = "Relic_")]
    public sealed class RelicDefinition : ParallaxDefinition
    {
        [SerializeField, Tooltip("Cada Relíquia é exclusiva de uma classe e única na campanha.")]
        private ParallaxClass exclusiveTo;
        [SerializeField, Tooltip("Lágrima do Leviatã não projeta sombra.")]
        private bool castsShadow = true;
        [SerializeField, Min(0), Tooltip("Código-Fonte Ômega: com N+ máquinas infectadas, a interface falsifica uma leitura por puzzle.")]
        private int leakCountThreshold;

        public ParallaxClass ExclusiveTo => exclusiveTo;
        public bool CastsShadow => castsShadow;
        public int LeakCountThreshold => leakCountThreshold;
    }
}
