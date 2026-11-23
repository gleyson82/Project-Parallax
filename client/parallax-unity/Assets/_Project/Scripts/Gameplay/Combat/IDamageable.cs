using UnityEngine;

namespace Parallax.Gameplay.Combat
{
    /// <summary>Qualquer coisa que recebe dano no servidor (operadores, Esquadras, máquinas).</summary>
    public interface IDamageable
    {
        bool IsAlive { get; }

        /// <param name="chill">Frio 0–1 aplicado ao alvo (armas criogênicas e espuma de contenção).</param>
        void ServerApplyDamage(float damage, float chill, ulong attackerClientId, Vector3 hitPoint);
    }
}
