using Parallax.Core.Roles;
using UnityEngine;

namespace Parallax.Core.Definitions
{
    [CreateAssetMenu(menuName = "Parallax/Definitions/Lore Fragment", fileName = "Fragment_")]
    public sealed class LoreFragment : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private ParallaxClass viewer;
        [SerializeField] private bool isDecoy;
        [SerializeField] private string titleKey;
        [SerializeField] private string textKey;
        [SerializeField] private string[] links = new string[0];

        public string Id => id;

        /// <summary>Um fragmento pertence a um único olhar (convencoes.md).</summary>
        public ParallaxClass Viewer => viewer;

        /// <summary>status: isca no Daedalus. O jogo nunca avisa o jogador.</summary>
        public bool IsDecoy => isDecoy;
        public string TitleKey => titleKey;
        public string TextKey => textKey;
        public string[] Links => links;

#if UNITY_EDITOR
        public void EditorSetup(string newId, ParallaxClass newViewer, bool decoy, string[] newLinks)
        {
            id = newId;
            viewer = newViewer;
            isDecoy = decoy;
            titleKey = "fragment." + newId + ".title";
            textKey = "fragment." + newId + ".text";
            links = newLinks ?? new string[0];
        }
#endif
    }
}
