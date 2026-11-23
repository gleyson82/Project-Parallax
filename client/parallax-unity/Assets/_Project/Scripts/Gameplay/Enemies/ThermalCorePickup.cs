using System;
using Parallax.Gameplay.Audio;
using Parallax.Gameplay.Combat;
using Parallax.Gameplay.Thermal;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Enemies
{
    /// <summary>
    /// Núcleo Térmico caído de um inimigo. Esfria no chão (ThermalNode): quem demora, extrai menos carga.
    /// O servidor recolhe quando um operador de pé passa perto.
    /// </summary>
    [RequireComponent(typeof(ThermalNode))]
    public sealed class ThermalCorePickup : NetworkBehaviour
    {
        [SerializeField, Min(0.3f)] private float pickupRadius = 1.5f;
        [SerializeField] private Transform visual;
        [SerializeField] private Light glow;
        [SerializeField, Min(0f)] private float maxGlowIntensity = 6f;

        private ThermalNode _thermal;
        private bool _collected;

        /// <summary>Servidor: núcleo recolhido com a carga (0–1) que ainda tinha.</summary>
        public static event Action<float> ServerCollected;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetEvents() => ServerCollected = null;

        private void Awake() => _thermal = GetComponent<ThermalNode>();

        public override void OnNetworkSpawn()
        {
            _thermal.TemperatureChanged += OnTemperatureChanged;
            if (IsServer)
            {
                NetworkManager.NetworkTickSystem.Tick += OnServerTick;
            }
        }

        public override void OnNetworkDespawn()
        {
            _thermal.TemperatureChanged -= OnTemperatureChanged;
            if (IsServer && NetworkManager != null && NetworkManager.NetworkTickSystem != null)
            {
                NetworkManager.NetworkTickSystem.Tick -= OnServerTick;
            }
        }

        // Visual: o núcleo gira e flutua.
        private void LateUpdate()
        {
            if (visual != null)
            {
                visual.localPosition = new Vector3(0f, 0.15f + Mathf.Sin(Time.time * 2.4f) * 0.08f, 0f);
                visual.Rotate(0f, 70f * Time.deltaTime, 0f, Space.World);
            }
        }

        private void OnServerTick()
        {
            if (_collected)
            {
                return;
            }

            foreach (PlayerVitals vitals in PlayerVitals.All)
            {
                if (vitals == null || !vitals.IsAlive)
                {
                    continue;
                }

                if ((vitals.transform.position - transform.position).sqrMagnitude <= pickupRadius * pickupRadius)
                {
                    _collected = true;
                    ServerCollected?.Invoke(_thermal.NormalizedHeat);
                    CollectedFxRpc();
                    NetworkObject.Despawn(true);
                    return;
                }
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void CollectedFxRpc() => AudioCues.Play(AudioCue.Pickup, transform.position);

        private void OnTemperatureChanged(float kelvin, float normalizedHeat)
        {
            if (glow != null)
            {
                glow.intensity = maxGlowIntensity * normalizedHeat;
            }
        }
    }
}
