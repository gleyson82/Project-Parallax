using Parallax.Core.Roles;
using UnityEngine;

namespace Parallax.Core.Definitions
{
    [CreateAssetMenu(menuName = "Parallax/Definitions/Class", fileName = "Class_")]
    public sealed class ClassDefinition : ParallaxDefinition
    {
        [SerializeField] private ParallaxClass parallaxClass;
        [SerializeField, Tooltip("Rig compartilhado: Bípede Padrão (1,8 m) ou Bípede Pesado (2,6 m).")]
        private bool usesHeavyRig;
        [SerializeField, Min(0.1f)] private float moveSpeed = 4.5f;
        [SerializeField, Min(1f)] private float bodyMassKg = 90f;
        [SerializeField] private CorporationId lockCorporation;

        public ParallaxClass Class => parallaxClass;
        public bool UsesHeavyRig => usesHeavyRig;
        public float MoveSpeed => moveSpeed;
        public float BodyMassKg => bodyMassKg;
        public CorporationId LockCorporation => lockCorporation;
        public AuxiliaryId Auxiliary => ParallaxRoles.AuxiliaryFor(parallaxClass);

#if UNITY_EDITOR
        public void EditorSetup(ParallaxClass newClass, bool heavyRig, float speed, float massKg)
        {
            parallaxClass = newClass;
            usesHeavyRig = heavyRig;
            moveSpeed = speed;
            bodyMassKg = massKg;
            lockCorporation = ParallaxRoles.LockOperatedBy(newClass);
        }
#endif
    }
}
