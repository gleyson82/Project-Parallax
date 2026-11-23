using System;
using System.Collections;
using Parallax.Core.Roles;
using Parallax.Gameplay.Audio;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Interaction
{
    /// <summary>
    /// Volante hidráulico. O Goliath ancora os pés e gira rápido; as outras classes giram devagar.
    /// Só responde depois de armado pela quest.
    /// </summary>
    public sealed class ValveWheel : NetworkInteractable
    {
        [SerializeField] private string label = "Válvula Mestra 14";
        [SerializeField] private Transform wheel;
        [SerializeField] private Vector3 wheelAxis = Vector3.forward;
        [SerializeField, Min(0.1f)] private float goliathHoldSeconds = 1.6f;
        [SerializeField, Min(0.1f)] private float otherHoldSeconds = 4.5f;
        [SerializeField] private ParticleSystem releaseSteam;

        private readonly NetworkVariable<bool> _armed = new NetworkVariable<bool>();
        private readonly NetworkVariable<bool> _opened = new NetworkVariable<bool>();

        public event Action<ParallaxClass> ServerOpened;

        public bool IsOpened => _opened.Value;

        public override void OnNetworkSpawn() => _opened.OnValueChanged += OnOpenedChanged;

        public override void OnNetworkDespawn() => _opened.OnValueChanged -= OnOpenedChanged;

        public void ServerSetArmed(bool armed)
        {
            if (IsServer)
            {
                _armed.Value = armed;
            }
        }

        public override string GetPrompt(ParallaxClass viewer)
        {
            if (!_armed.Value || _opened.Value)
            {
                return string.Empty;
            }

            return viewer == ParallaxClass.Goliath ? "Segurar · girar a " + label : "Segurar · girar a " + label + " (lento sem o Goliath)";
        }

        public override float GetHoldSeconds(ParallaxClass viewer) => viewer == ParallaxClass.Goliath ? goliathHoldSeconds : otherHoldSeconds;

        public override void OnLocalInteract(ParallaxClass viewer) => AudioCues.Play(AudioCue.ValveCreak, FocusPosition);

        protected override void OnServerInteract(ulong clientId, ParallaxClass parallaxClass, NetworkObject player)
        {
            if (!_armed.Value || _opened.Value)
            {
                return;
            }

            _opened.Value = true;
            ServerOpened?.Invoke(parallaxClass);
        }

        private void OnOpenedChanged(bool previous, bool opened)
        {
            if (!opened)
            {
                return;
            }

            if (releaseSteam != null)
            {
                releaseSteam.Play();
            }

            AudioCues.Play(AudioCue.SteamBurst, FocusPosition);
            if (wheel != null)
            {
                StartCoroutine(Spin());
            }
        }

        private IEnumerator Spin()
        {
            Quaternion start = wheel.localRotation;
            const float seconds = 1.6f;
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                float eased = Mathf.SmoothStep(0f, 1f, t / seconds);
                wheel.localRotation = start * Quaternion.AngleAxis(-540f * eased, wheelAxis);
                yield return null;
            }

            wheel.localRotation = start * Quaternion.AngleAxis(-540f, wheelAxis);
        }
    }
}
