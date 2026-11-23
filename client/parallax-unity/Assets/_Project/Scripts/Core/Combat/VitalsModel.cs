using System;

namespace Parallax.Core.Combat
{
    /// <summary>
    /// Integridade do traje. Em zero o operador cai e espera resgate; resgatado, volta com parte da integridade.
    /// </summary>
    public sealed class VitalsModel
    {
        public VitalsModel(float maxIntegrity)
        {
            SetMax(maxIntegrity, true);
        }

        public float MaxIntegrity { get; private set; }
        public float Integrity { get; private set; }
        public bool IsDowned { get; private set; }
        public float Integrity01 => MaxIntegrity <= 0f ? 0f : Integrity / MaxIntegrity;

        public void SetMax(float maxIntegrity, bool refill)
        {
            MaxIntegrity = Math.Max(1f, maxIntegrity);
            Integrity = refill ? MaxIntegrity : Math.Min(Integrity, MaxIntegrity);
        }

        /// <returns>Verdadeiro se este dano derrubou o operador.</returns>
        public bool ApplyDamage(float amount)
        {
            if (IsDowned || amount <= 0f)
            {
                return false;
            }

            Integrity = Math.Max(0f, Integrity - amount);
            if (Integrity <= 0f)
            {
                IsDowned = true;
                return true;
            }

            return false;
        }

        public void Heal(float amount)
        {
            if (IsDowned || amount <= 0f)
            {
                return;
            }

            Integrity = Math.Min(MaxIntegrity, Integrity + amount);
        }

        public void Revive(float fraction)
        {
            IsDowned = false;
            Integrity = Math.Max(1f, MaxIntegrity * Math.Min(1f, Math.Max(0f, fraction)));
        }
    }
}
