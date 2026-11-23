using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Thermal
{
    /// <summary>
    /// Avança todos os ThermalNode do servidor no tick de rede (NetworkTickSystem), e não em Update.
    /// </summary>
    public static class ThermalSystem
    {
        private static readonly List<ThermalNode> Nodes = new List<ThermalNode>();
        private static NetworkTickSystem _subscribedTickSystem;
        private static float _tickDelta;

        public static int Count => Nodes.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Nodes.Clear();
            _subscribedTickSystem = null;
        }

        internal static void Register(ThermalNode node)
        {
            if (!Nodes.Contains(node))
            {
                Nodes.Add(node);
            }

            EnsureSubscribed(node.NetworkManager);
        }

        internal static void Unregister(ThermalNode node)
        {
            Nodes.Remove(node);
            if (Nodes.Count == 0 && _subscribedTickSystem != null)
            {
                _subscribedTickSystem.Tick -= OnTick;
                _subscribedTickSystem = null;
            }
        }

        private static void EnsureSubscribed(NetworkManager manager)
        {
            if (manager == null || manager.NetworkTickSystem == null || _subscribedTickSystem == manager.NetworkTickSystem)
            {
                return;
            }

            if (_subscribedTickSystem != null)
            {
                _subscribedTickSystem.Tick -= OnTick;
            }

            _subscribedTickSystem = manager.NetworkTickSystem;
            _tickDelta = 1f / Mathf.Max(1, (int)manager.NetworkConfig.TickRate);
            _subscribedTickSystem.Tick += OnTick;
        }

        private static void OnTick()
        {
            for (int i = Nodes.Count - 1; i >= 0; i--)
            {
                ThermalNode node = Nodes[i];
                if (node == null)
                {
                    Nodes.RemoveAt(i);
                    continue;
                }

                node.ServerTick(_tickDelta);
            }
        }
    }
}
