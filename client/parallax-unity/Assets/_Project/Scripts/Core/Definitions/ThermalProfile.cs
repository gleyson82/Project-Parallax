using Parallax.Core.Thermal;
using UnityEngine;

namespace Parallax.Core.Definitions
{
    [CreateAssetMenu(menuName = "Parallax/Definitions/Thermal Profile", fileName = "Thermal_")]
    public sealed class ThermalProfile : ScriptableObject
    {
        [SerializeField] private float initialKelvin = 290f;
        [SerializeField] private float ambientKelvin = ThermalConstants.TritonAmbientKelvin;
        [SerializeField] private float overheatThresholdKelvin = 750f;
        [SerializeField] private float freezeThresholdKelvin = 95f;
        [SerializeField] private float thermalShockMinDropKelvin = 380f;
        [SerializeField] private float coolingRateKelvinPerSecond = 6f;
        [SerializeField] private bool passiveCoolingEnabled = true;

        public float InitialKelvin => initialKelvin;

        public ThermalSettings ToSettings() => new ThermalSettings
        {
            AmbientKelvin = ambientKelvin,
            OverheatThresholdKelvin = overheatThresholdKelvin,
            FreezeThresholdKelvin = freezeThresholdKelvin,
            ThermalShockMinDropKelvin = thermalShockMinDropKelvin,
            CoolingRateKelvinPerSecond = coolingRateKelvinPerSecond,
            PassiveCoolingEnabled = passiveCoolingEnabled
        };

#if UNITY_EDITOR
        public void EditorSetup(float initial, float ambient, float coolingRate)
        {
            initialKelvin = initial;
            ambientKelvin = ambient;
            coolingRateKelvinPerSecond = coolingRate;
        }
#endif
    }
}
