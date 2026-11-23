using Parallax.Core.Roles;
using UnityEngine;

namespace Parallax.Core.Definitions
{
    [CreateAssetMenu(menuName = "Parallax/Definitions/Weapon", fileName = "Weapon_")]
    public sealed class WeaponDefinition : ParallaxDefinition
    {
        [SerializeField] private CorporationId manufacturer;
        [SerializeField] private DamageKind damageKind;
        [SerializeField, Min(0f)] private float damagePerShot = 20f;
        [SerializeField, Min(0.03f)] private float fireIntervalSeconds = 0.18f;
        [SerializeField, Min(1f)] private float rangeMeters = 60f;
        [SerializeField, Range(0f, 8f)] private float spreadDegrees = 0.8f;
        [SerializeField, Range(0f, 1f), Tooltip("Fração do cano aquecida por disparo (1 = trava no primeiro tiro).")]
        private float heatPerShot = 0.09f;
        [SerializeField, Min(0f), Tooltip("Fração do cano resfriada por segundo.")]
        private float coolingPerSecond = 0.32f;
        [SerializeField, Range(0f, 1f), Tooltip("Frio aplicado ao alvo por acerto (Kinesys criogênica).")]
        private float chillPerHit;
        [SerializeField, Tooltip("Doutrina do Fogo Mínimo: trava perto de Linhas Vivas (só armas do Sindicato).")]
        private bool locksNearLiveLines;
        [SerializeField] private Color tracerColor = new Color(1f, 0.45f, 0.15f);

        public CorporationId Manufacturer => manufacturer;
        public DamageKind DamageKind => damageKind;
        public float DamagePerShot => damagePerShot;
        public float FireIntervalSeconds => fireIntervalSeconds;
        public float RangeMeters => rangeMeters;
        public float SpreadDegrees => spreadDegrees;
        public float HeatPerShot => heatPerShot;
        public float CoolingPerSecond => coolingPerSecond;
        public float ChillPerHit => chillPerHit;
        public bool LocksNearLiveLines => locksNearLiveLines;
        public Color TracerColor => tracerColor;

#if UNITY_EDITOR
        public void EditorSetup(CorporationId newManufacturer, DamageKind kind, float damage, float interval, float range,
            float spread, float heat, float cooling, float chill, Color tracer)
        {
            manufacturer = newManufacturer;
            damageKind = kind;
            damagePerShot = damage;
            fireIntervalSeconds = interval;
            rangeMeters = range;
            spreadDegrees = spread;
            heatPerShot = heat;
            coolingPerSecond = cooling;
            chillPerHit = chill;
            tracerColor = tracer;
        }
#endif
    }
}
