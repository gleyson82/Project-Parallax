using System;
using Parallax.Gameplay.Combat;
using Parallax.Gameplay.Input;
using Parallax.Gameplay.Players;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Parallax.Gameplay.Interaction
{
    /// <summary>
    /// Escolhe o interagível em foco (perto e na direção da câmera) e aciona com toque ou segurando.
    /// Só roda no dono; a decisão final é sempre do servidor.
    /// </summary>
    public sealed class PlayerInteractor : NetworkBehaviour
    {
        [SerializeField] private ParallaxInput input;
        [SerializeField] private PlayerClassBinder binder;
        [SerializeField] private PlayerVitals vitals;
        [SerializeField, Range(0f, 1f), Tooltip("Peso da direção da câmera na escolha do foco.")]
        private float viewBias = 0.6f;

        private Transform _camera;
        private bool _holding;
        private float _held;

        public static PlayerInteractor Local { get; private set; }

        public event Action<NetworkInteractable> Committed;

        public NetworkInteractable Focus { get; private set; }
        public string FocusPrompt { get; private set; } = string.Empty;

        /// <summary>0–1 enquanto segura uma ação com tempo; negativo quando não há segurar em curso.</summary>
        public float HoldProgress => _holding && Focus != null ? Mathf.Clamp01(_held / Mathf.Max(0.01f, Focus.GetHoldSeconds(binder.CurrentClass))) : -1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Local = null;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                enabled = false;
                return;
            }

            Local = this;
            input.Interact.started += OnInteractStarted;
            input.Interact.canceled += OnInteractCanceled;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner)
            {
                return;
            }

            input.Interact.started -= OnInteractStarted;
            input.Interact.canceled -= OnInteractCanceled;
            if (Local == this)
            {
                Local = null;
            }
        }

        // Mira de interação do dono (apresentação e entrada); a regra fica no servidor.
        private void Update()
        {
            if (!binder.HasClass || (vitals != null && vitals.IsDowned))
            {
                SetFocus(null);
                _holding = false;
                return;
            }

            SetFocus(FindBest());
            if (!_holding)
            {
                return;
            }

            if (Focus == null)
            {
                _holding = false;
                return;
            }

            _held += Time.deltaTime;
            if (_held >= Focus.GetHoldSeconds(binder.CurrentClass))
            {
                _holding = false;
                Commit();
            }
        }

        private NetworkInteractable FindBest()
        {
            if (_camera == null && UnityEngine.Camera.main != null)
            {
                _camera = UnityEngine.Camera.main.transform;
            }

            Vector3 origin = transform.position + Vector3.up;
            Vector3 view = _camera != null ? _camera.forward : transform.forward;
            NetworkInteractable best = null;
            float bestScore = float.MaxValue;
            foreach (NetworkInteractable candidate in NetworkInteractable.All)
            {
                if (candidate == null || !candidate.IsSpawned)
                {
                    continue;
                }

                Vector3 toTarget = candidate.FocusPosition - origin;
                float distance = toTarget.magnitude;
                if (distance > candidate.Range + 1f)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(candidate.GetPrompt(binder.CurrentClass)))
                {
                    continue;
                }

                float facing = distance > 0.01f ? Vector3.Dot(view, toTarget / distance) : 1f;
                float score = distance * (1f + viewBias * (1f - facing));
                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            return best;
        }

        private void SetFocus(NetworkInteractable focus)
        {
            if (focus != Focus)
            {
                Focus = focus;
                _holding = false;
                _held = 0f;
            }

            FocusPrompt = Focus != null ? Focus.GetPrompt(binder.CurrentClass) : string.Empty;
        }

        private void OnInteractStarted(InputAction.CallbackContext context)
        {
            if (Focus == null)
            {
                return;
            }

            if (Focus.GetHoldSeconds(binder.CurrentClass) <= 0f)
            {
                Commit();
                return;
            }

            _holding = true;
            _held = 0f;
        }

        private void OnInteractCanceled(InputAction.CallbackContext context)
        {
            _holding = false;
            _held = 0f;
        }

        private void Commit()
        {
            if (Focus == null)
            {
                return;
            }

            Focus.OnLocalInteract(binder.CurrentClass);
            Committed?.Invoke(Focus);
            Focus.RequestInteract();
        }
    }
}
