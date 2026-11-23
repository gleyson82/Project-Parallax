using Parallax.Core.Roles;
using UnityEngine;

namespace Parallax.Core.Definitions
{
    [CreateAssetMenu(menuName = "Parallax/Definitions/Auxiliary", fileName = "Auxiliary_")]
    public sealed class AuxiliaryDefinition : ParallaxDefinition
    {
        [SerializeField] private AuxiliaryId auxiliary;
        [SerializeField, Min(0)] private int usesPerEncounter = 1;
        [SerializeField, Min(0.1f)] private float verbDurationSeconds = 2f;

        public AuxiliaryId Auxiliary => auxiliary;
        public ParallaxClass Covers => ParallaxRoles.ClassCoveredBy(auxiliary);
        public int UsesPerEncounter => usesPerEncounter;
        public float VerbDurationSeconds => verbDurationSeconds;

#if UNITY_EDITOR
        public void EditorSetup(AuxiliaryId newAuxiliary, int uses, float duration)
        {
            auxiliary = newAuxiliary;
            usesPerEncounter = uses;
            verbDurationSeconds = duration;
        }
#endif
    }
}
