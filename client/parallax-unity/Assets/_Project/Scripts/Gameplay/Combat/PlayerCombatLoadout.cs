using Parallax.Core.Definitions;
using Parallax.Core.Roles;
using Parallax.Gameplay.Players;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Combat
{
    /// <summary>
    /// Equipa a arma da corporação da classe (Therma-Tek / Kinesys / Omni-Cripto) quando o olhar muda.
    /// Também liga vitalidade e o ponto de resgate no mesmo prefab.
    /// </summary>
    public sealed class PlayerCombatLoadout : NetworkBehaviour
    {
        [SerializeField] private PlayerClassBinder binder;
        [SerializeField] private PlayerWeapon weapon;
        [SerializeField] private PlayerVitals vitals;
        [SerializeField] private ReviveInteractable revive;
        [SerializeField] private WeaponDefinition thermaTekSlag;
        [SerializeField] private WeaponDefinition kinesysCryo;
        [SerializeField] private WeaponDefinition omniPulse;

        public override void OnNetworkSpawn()
        {
            if (binder != null)
            {
                binder.ClassApplied += OnClassApplied;
                if (binder.HasClass)
                {
                    OnClassApplied(binder.CurrentClass);
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            if (binder != null)
            {
                binder.ClassApplied -= OnClassApplied;
            }
        }

        private void OnClassApplied(ParallaxClass parallaxClass)
        {
            WeaponDefinition definition = WeaponFor(parallaxClass);
            if (weapon != null)
            {
                weapon.Equip(definition);
            }
        }

        private WeaponDefinition WeaponFor(ParallaxClass parallaxClass)
        {
            switch (parallaxClass)
            {
                case ParallaxClass.Goliath: return thermaTekSlag;
                case ParallaxClass.Phantom: return kinesysCryo;
                default: return omniPulse;
            }
        }
    }
}
