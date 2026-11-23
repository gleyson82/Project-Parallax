using System;

namespace Parallax.Core.Thermal
{
    [Flags]
    public enum ThermalEvents : byte
    {
        None = 0,
        BecameOverheated = 1 << 0,
        BecameFrozen = 1 << 1,
        ThermalShock = 1 << 2,
        Changed = 1 << 3
    }

    [Serializable]
    public struct ThermalSettings
    {
        public float AmbientKelvin;
        public float OverheatThresholdKelvin;
        public float FreezeThresholdKelvin;
        public float ThermalShockMinDropKelvin;
        public float CoolingRateKelvinPerSecond;
        public bool PassiveCoolingEnabled;

        public static ThermalSettings Default => new ThermalSettings
        {
            AmbientKelvin = ThermalConstants.TritonAmbientKelvin,
            OverheatThresholdKelvin = 750f,
            FreezeThresholdKelvin = 95f,
            ThermalShockMinDropKelvin = 380f,
            CoolingRateKelvinPerSecond = 6f,
            PassiveCoolingEnabled = true
        };
    }

    public static class ThermalConstants
    {
        /// <summary>38 K (-235 °C): superfície de Tritão. Aço comum vira vidro.</summary>
        public const float TritonAmbientKelvin = 38f;

        /// <summary>Célula Térmica perde ~2% da carga por dia (economia-do-calor).</summary>
        public const float ThermalCellDecayPerDay = 0.02f;

        public const float KelvinToCelsiusOffset = 273.15f;
    }

    /// <summary>
    /// Modelo termodinâmico puro (sem UnityEngine).
    /// Quem possui a autoridade (servidor) chama Step/ApplyHeat/ApplyCryo; os clientes só leem.
    /// </summary>
    public sealed class ThermalModel
    {
        private ThermalSettings _settings;

        public ThermalModel(ThermalSettings settings, float initialKelvin)
        {
            _settings = settings;
            CurrentKelvin = Math.Max(settings.AmbientKelvin, initialKelvin);
            IsOverheated = CurrentKelvin >= settings.OverheatThresholdKelvin;
            IsFrozen = CurrentKelvin <= settings.FreezeThresholdKelvin;
        }

        public float CurrentKelvin { get; private set; }
        public bool IsOverheated { get; private set; }
        public bool IsFrozen { get; private set; }
        public float LastShockDropKelvin { get; private set; }
        public ThermalSettings Settings => _settings;

        public float NormalizedHeat
        {
            get
            {
                float range = Math.Max(1f, _settings.OverheatThresholdKelvin - ThermalConstants.TritonAmbientKelvin);
                return Clamp01((CurrentKelvin - ThermalConstants.TritonAmbientKelvin) / range);
            }
        }

        public void SetAmbient(float ambientKelvin) => _settings.AmbientKelvin = ambientKelvin;

        public ThermalEvents Step(float deltaSeconds)
        {
            if (!_settings.PassiveCoolingEnabled || deltaSeconds <= 0f)
            {
                return ThermalEvents.None;
            }

            if (Math.Abs(CurrentKelvin - _settings.AmbientKelvin) <= 0.1f)
            {
                return ThermalEvents.None;
            }

            float next = MoveTowards(CurrentKelvin, _settings.AmbientKelvin, _settings.CoolingRateKelvinPerSecond * deltaSeconds);
            return SetTemperature(next);
        }

        public ThermalEvents ApplyHeat(float deltaKelvin) => SetTemperature(CurrentKelvin + Math.Abs(deltaKelvin));

        /// <summary>Resfriamento criogênico. Superaquecido + queda grande = Choque Térmico (Mestre de Fornalha).</summary>
        public ThermalEvents ApplyCryo(float deltaKelvin)
        {
            float drop = Math.Abs(deltaKelvin);
            bool wasOverheated = IsOverheated;
            ThermalEvents events = SetTemperature(Math.Max(ThermalConstants.TritonAmbientKelvin, CurrentKelvin - drop));
            if (wasOverheated && drop >= _settings.ThermalShockMinDropKelvin)
            {
                LastShockDropKelvin = drop;
                events |= ThermalEvents.ThermalShock;
            }

            return events;
        }

        public static float ComputeThermalCellCharge(float initialCharge, float elapsedDays)
        {
            double days = Math.Max(0.0, elapsedDays);
            return (float)Math.Max(0.0, initialCharge * Math.Pow(1.0 - ThermalConstants.ThermalCellDecayPerDay, days));
        }

        private ThermalEvents SetTemperature(float newKelvin)
        {
            float previous = CurrentKelvin;
            CurrentKelvin = Math.Max(ThermalConstants.TritonAmbientKelvin, newKelvin);
            bool nowOverheated = CurrentKelvin >= _settings.OverheatThresholdKelvin;
            bool nowFrozen = CurrentKelvin <= _settings.FreezeThresholdKelvin;

            ThermalEvents events = ThermalEvents.None;
            if (nowOverheated && !IsOverheated)
            {
                events |= ThermalEvents.BecameOverheated;
            }

            if (nowFrozen && !IsFrozen)
            {
                events |= ThermalEvents.BecameFrozen;
            }

            if (Math.Abs(previous - CurrentKelvin) > float.Epsilon)
            {
                events |= ThermalEvents.Changed;
            }

            IsOverheated = nowOverheated;
            IsFrozen = nowFrozen;
            return events;
        }

        private static float MoveTowards(float current, float target, float maxDelta)
        {
            if (Math.Abs(target - current) <= maxDelta)
            {
                return target;
            }

            return current + Math.Sign(target - current) * maxDelta;
        }

        private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;
    }
}
