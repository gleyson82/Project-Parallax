using System;
using System.Collections;
using Parallax.Core.Roles;
using Parallax.Gameplay.Commands;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.World
{
    /// <summary>
    /// Porta de chumbo de 10 t / porta de pressão. Só sobe enquanto um Goliath ancorado (ou o Macaco)
    /// sustenta o peso; se soltar, ela desce pela própria massa. Física simulada só no servidor.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(ConfigurableJoint))]
    public sealed class HeavyDoor : NetworkBehaviour, IRehearsableAction
    {
        [SerializeField, Min(100f)] private float massKg = 10000f;
        [SerializeField, Min(0.1f)] private float liftHeight = 3.4f;
        [SerializeField, Min(1f)] private float liftSpring = 2.5e6f;
        [SerializeField, Min(1f)] private float liftDamper = 2.0e5f;
        [SerializeField, Min(0.1f)] private float auxiliaryHoldSeconds = 6f;

        private readonly NetworkVariable<bool> _supported = new NetworkVariable<bool>();
        private readonly NetworkVariable<bool> _latched = new NetworkVariable<bool>();
        private Rigidbody _body;
        private ConfigurableJoint _joint;
        private Coroutine _auxiliaryHold;

        public event Action<bool> SupportChanged;

        public ParallaxClass Role => ParallaxClass.Goliath;
        public ulong RehearsalTargetId => NetworkObjectId;
        public bool IsSatisfied => _supported.Value;
        public float OpenAmount => _joint != null ? Mathf.Clamp01(transform.localPosition.y / liftHeight) : 0f;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _joint = GetComponent<ConfigurableJoint>();
            _body.mass = massKg;
            _body.interpolation = RigidbodyInterpolation.Interpolate;

            _joint.xMotion = ConfigurableJointMotion.Locked;
            _joint.zMotion = ConfigurableJointMotion.Locked;
            _joint.yMotion = ConfigurableJointMotion.Limited;
            _joint.angularXMotion = ConfigurableJointMotion.Locked;
            _joint.angularYMotion = ConfigurableJointMotion.Locked;
            _joint.angularZMotion = ConfigurableJointMotion.Locked;
            _joint.linearLimit = new SoftJointLimit { limit = liftHeight };
            ApplyDrive(false);
        }

        public override void OnNetworkSpawn()
        {
            _body.isKinematic = !IsServer;
            _supported.OnValueChanged += OnSupportChanged;
        }

        public override void OnNetworkDespawn() => _supported.OnValueChanged -= OnSupportChanged;

        public bool IsLatched => _latched.Value;

        /// <summary>Servidor: a Chave Tripla abriu; a porta trava no alto e não desce mais.</summary>
        public void ServerLatchOpen()
        {
            if (!IsServer || _latched.Value)
            {
                return;
            }

            _latched.Value = true;
            _supported.Value = true;
            ApplyDrive(true);
            _joint.yDrive = new JointDrive { positionSpring = liftSpring * 4f, positionDamper = liftDamper, maximumForce = float.MaxValue };
        }

        /// <summary>Servidor: o Goliath ancorado começa ou para de sustentar a porta.</summary>
        public void ServerSetSupported(bool supported)
        {
            if (!IsServer || _latched.Value)
            {
                return;
            }

            _supported.Value = supported;
            ApplyDrive(supported);
        }

        public void ExecuteAsAuxiliary(AuxiliaryId auxiliary, Action<bool> onFinished)
        {
            if (!IsServer || auxiliary != AuxiliaryId.Macaco)
            {
                onFinished?.Invoke(false);
                return;
            }

            if (_auxiliaryHold != null)
            {
                StopCoroutine(_auxiliaryHold);
            }

            _auxiliaryHold = StartCoroutine(MacacoHold(onFinished));
        }

        private IEnumerator MacacoHold(Action<bool> onFinished)
        {
            ServerSetSupported(true);
            onFinished?.Invoke(true);
            yield return new WaitForSeconds(auxiliaryHoldSeconds);
            ServerSetSupported(false);
            _auxiliaryHold = null;
        }

        private void ApplyDrive(bool supported)
        {
            _joint.targetPosition = new Vector3(0f, supported ? -liftHeight : 0f, 0f);
            _joint.yDrive = new JointDrive
            {
                positionSpring = supported ? liftSpring : 0f,
                positionDamper = liftDamper,
                maximumForce = supported ? float.MaxValue : 0f
            };
        }

        private void OnSupportChanged(bool previous, bool current) => SupportChanged?.Invoke(current);
    }
}
