using System;
using System.Collections;
using Parallax.Core.Roles;
using Parallax.Gameplay.Audio;
using Parallax.Gameplay.Interaction;
using Parallax.Gameplay.Session;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Quest
{
    /// <summary>
    /// Contêiner de acumuladores sobre trilho. Pesado demais para traje leve: só o Goliath empurra.
    /// Sem Goliath na sessão, o Macaco engata e arrasta até o duto. Física só no servidor.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PushableCargo : NetworkInteractable
    {
        [SerializeField] private Transform deliveryPoint;
        [SerializeField, Min(0.2f)] private float deliveryRadius = 1.6f;
        [SerializeField, Min(0.1f)] private float pushSpeed = 1.8f;
        [SerializeField, Min(0.1f)] private float towSpeed = 1.3f;
        [SerializeField] private Vector3 railAxis = Vector3.right;
        [SerializeField] private GameObject macacoTowVisual;

        private readonly NetworkVariable<bool> _active = new NetworkVariable<bool>();
        private readonly NetworkVariable<bool> _delivered = new NetworkVariable<bool>();
        private readonly NetworkVariable<bool> _towing = new NetworkVariable<bool>();

        private Rigidbody _body;
        private float _pushDirection;
        private double _pushUntil;

        public event Action ServerDelivered;

        public bool IsActive => _active.Value;
        public bool IsDelivered => _delivered.Value;
        public bool IsTowing => _towing.Value;

        public float Progress01
        {
            get
            {
                if (deliveryPoint == null)
                {
                    return 0f;
                }

                float total = Vector3.Dot(deliveryPoint.position - _startPosition, railAxis.normalized);
                float done = Vector3.Dot(transform.position - _startPosition, railAxis.normalized);
                return total <= 0.01f ? 1f : Mathf.Clamp01(done / total);
            }
        }

        private Vector3 _startPosition;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _startPosition = transform.position;
        }

        public override void OnNetworkSpawn()
        {
            _body.isKinematic = !IsServer;
            _towing.OnValueChanged += OnTowingChanged;
            OnTowingChanged(false, _towing.Value);
        }

        public override void OnNetworkDespawn() => _towing.OnValueChanged -= OnTowingChanged;

        public void ServerSetActive(bool active)
        {
            if (IsServer)
            {
                _active.Value = active;
            }
        }

        public override string GetPrompt(ParallaxClass viewer)
        {
            if (!_active.Value || _delivered.Value || _towing.Value)
            {
                return string.Empty;
            }

            SessionRoster roster = SessionRoster.Active;
            bool goliathPresent = roster != null && roster.IsHuman(ParallaxClass.Goliath);
            return goliathPresent ? string.Empty : "Engatar o Macaco no contêiner";
        }

        /// <summary>Dono Goliath encostou no contêiner andando contra ele.</summary>
        public void RequestPush(float direction) => PushRpc(Mathf.Sign(direction));

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void PushRpc(float direction, RpcParams rpcParams = default)
        {
            SessionRoster roster = SessionRoster.Active;
            if (!_active.Value || _delivered.Value || _towing.Value || roster == null ||
                !roster.TryGetClassOf(rpcParams.Receive.SenderClientId, out ParallaxClass sender) || sender != ParallaxClass.Goliath)
            {
                return;
            }

            _pushDirection = direction;
            _pushUntil = NetworkManager.ServerTime.Time + 0.25;
        }

        protected override void OnServerInteract(ulong clientId, ParallaxClass parallaxClass, NetworkObject player)
        {
            if (_towing.Value || _delivered.Value)
            {
                return;
            }

            _towing.Value = true;
            StartCoroutine(Tow());
        }

        // Física do servidor: aplica o empurrão ao longo do trilho.
        private void FixedUpdate()
        {
            if (!IsServer || _delivered.Value || _towing.Value)
            {
                return;
            }

            Vector3 axis = railAxis.normalized;
            bool pushing = NetworkManager.ServerTime.Time < _pushUntil;
            Vector3 velocity = _body.linearVelocity;
            float along = pushing ? _pushDirection * pushSpeed : Mathf.MoveTowards(Vector3.Dot(velocity, axis), 0f, 6f * Time.fixedDeltaTime);
            _body.linearVelocity = axis * along + Vector3.up * velocity.y;
            CheckDelivered();
        }

        private IEnumerator Tow()
        {
            _body.isKinematic = true;
            while (!_delivered.Value && deliveryPoint != null)
            {
                Vector3 target = new Vector3(deliveryPoint.position.x, transform.position.y, deliveryPoint.position.z);
                transform.position = Vector3.MoveTowards(transform.position, target, towSpeed * Time.deltaTime);
                CheckDelivered();
                yield return null;
            }
        }

        private void CheckDelivered()
        {
            if (deliveryPoint == null || _delivered.Value)
            {
                return;
            }

            Vector3 flat = deliveryPoint.position - transform.position;
            flat.y = 0f;
            if (flat.sqrMagnitude <= deliveryRadius * deliveryRadius)
            {
                _delivered.Value = true;
                _body.linearVelocity = Vector3.zero;
                _body.isKinematic = true;
                DeliveredFxRpc();
                ServerDelivered?.Invoke();
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void DeliveredFxRpc()
        {
            AudioCues.Play(AudioCue.DoorRumble, transform.position);
            AudioCues.Play(AudioCue.SteamBurst, transform.position);
        }

        private void OnTowingChanged(bool previous, bool towing)
        {
            if (macacoTowVisual != null)
            {
                macacoTowVisual.SetActive(towing);
            }
        }
    }
}
