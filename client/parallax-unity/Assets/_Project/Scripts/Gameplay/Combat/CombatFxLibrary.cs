using UnityEngine;

namespace Parallax.Gameplay.Combat
{
    [CreateAssetMenu(menuName = "Parallax/Combat Fx Library", fileName = "CombatFxLibrary")]
    public sealed class CombatFxLibrary : ScriptableObject
    {
        [SerializeField] private Material tracerMaterial;
        [SerializeField] private ParticleSystem impactSparks;
        [SerializeField] private ParticleSystem steamBurst;
        [SerializeField] private ParticleSystem cryoPuff;

        public Material TracerMaterial => tracerMaterial;
        public ParticleSystem ImpactSparks => impactSparks;
        public ParticleSystem SteamBurst => steamBurst;
        public ParticleSystem CryoPuff => cryoPuff;

#if UNITY_EDITOR
        public void EditorSetup(Material tracer, ParticleSystem sparks, ParticleSystem steam, ParticleSystem cryo)
        {
            tracerMaterial = tracer;
            impactSparks = sparks;
            steamBurst = steam;
            cryoPuff = cryo;
        }
#endif
    }
}
