using System;

namespace Parallax.Core.Combat
{
    /// <summary>
    /// Calor do cano (0–1). Armas Therma-Tek ensinam a rajada curta: ao chegar em 100% a arma trava
    /// e só destrava quando o cano esfria abaixo do limiar.
    /// </summary>
    public sealed class WeaponHeat
    {
        public WeaponHeat(float heatPerShot, float coolingPerSecond, float unlockBelow = 0.3f)
        {
            HeatPerShot = Math.Max(0f, heatPerShot);
            CoolingPerSecond = Math.Max(0f, coolingPerSecond);
            UnlockBelow = Math.Min(0.95f, Math.Max(0f, unlockBelow));
        }

        public float HeatPerShot { get; }
        public float CoolingPerSecond { get; }
        public float UnlockBelow { get; }
        public float Heat01 { get; private set; }
        public bool Overheated { get; private set; }

        /// <summary>Dispara se o cano permitir. O disparo que encosta em 100% sai, e então a arma trava.</summary>
        public bool TryFire()
        {
            if (Overheated)
            {
                return false;
            }

            Heat01 = Math.Min(1f, Heat01 + HeatPerShot);
            if (Heat01 >= 1f)
            {
                Overheated = true;
            }

            return true;
        }

        public void Cool(float deltaSeconds)
        {
            if (deltaSeconds <= 0f)
            {
                return;
            }

            Heat01 = Math.Max(0f, Heat01 - CoolingPerSecond * deltaSeconds);
            if (Overheated && Heat01 <= UnlockBelow)
            {
                Overheated = false;
            }
        }
    }
}
