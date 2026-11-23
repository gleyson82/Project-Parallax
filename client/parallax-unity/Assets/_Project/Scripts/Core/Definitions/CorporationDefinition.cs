using Parallax.Core.Roles;
using UnityEngine;

namespace Parallax.Core.Definitions
{
    [CreateAssetMenu(menuName = "Parallax/Definitions/Corporation", fileName = "Corporation_")]
    public sealed class CorporationDefinition : ParallaxDefinition
    {
        [SerializeField] private CorporationId corporation;
        [SerializeField] private Color identityColor = Color.white;
        [SerializeField] private Texture2D emblem;

        public CorporationId Corporation => corporation;
        public Color IdentityColor => identityColor;
        public Texture2D Emblem => emblem;

#if UNITY_EDITOR
        public void EditorSetup(CorporationId newCorporation, Color color)
        {
            corporation = newCorporation;
            identityColor = color;
        }
#endif
    }
}
