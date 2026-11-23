using UnityEngine;

namespace Parallax.Core.Definitions
{
    /// <summary>Base de todo dado de design. O id é estável e igual ao nome da nota no Daedalus quando existir.</summary>
    public abstract class ParallaxDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayNameKey;
        [SerializeField] private string daedalusNote;

        public string Id => id;
        public string DisplayNameKey => displayNameKey;

        /// <summary>Nome da nota de lore em jogos/parallax/ que justifica este dado (Gameplay First).</summary>
        public string DaedalusNote => daedalusNote;

#if UNITY_EDITOR
        public void EditorInitialize(string newId, string newDisplayNameKey, string newDaedalusNote)
        {
            id = newId;
            displayNameKey = newDisplayNameKey;
            daedalusNote = newDaedalusNote;
        }
#endif
    }
}
