using Parallax.Gameplay.Input;
using Parallax.Gameplay.World;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Parallax.Gameplay.Characters
{
    /// <summary>
    /// Goliath: ancora os pés para sustentar portas de 10 t e girar volantes travados. Ancorado, não anda;
    /// as mãos vão por IK (Animation Rigging) até as alças do alvo. Modo Brasa esconde a assinatura térmica
    /// da Ômega, ao custo de força (Meta Atingida).
    /// </summary>
    public sealed class GoliathAnchor : NetworkBehaviour
    {
        [SerializeField] private ParallaxInput input;
        [SerializeField] private ParallaxCharacterMotor motor;
        [SerializeField] private TwoBoneIKConstraint leftHandIk;
        [SerializeField] private TwoBoneIKConstraint rightHandIk;
        [SerializeField, Min(0.1f)] private float reachRadius = 2.2f;
        [SerializeField, Min(0.1f)] private float ikBlendSpeed = 4f;
        [SerializeField] private LayerMask heavyDoorMask = ~0;

        private readonly NetworkVariable<bool> _anchored = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<bool> _emberMode = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly Collider[] _overlap = new Collider[8];
        private HeavyDoor _supportedDoor;

        public bool IsAnchored => _anchored.Value;
        public bool IsEmberMode => _emberMode.Value;

        /// <summary>Modo Brasa reduz a força física pela metade.</summary>
        public float StrengthMultiplier => _emberMode.Value ? 0.5f : 1f;

        public override void OnNetworkSpawn()
        {
            _anchored.OnValueChanged += OnAnchoredChanged;
            if (IsOwner && input != null)
            {
                input.Ability.performed += OnAbility;
            }
        }

        public override void OnNetworkDespawn()
        {
            _anchored.OnValueChanged -= OnAnchoredChanged;
            if (input != null)
            {
                input.Ability.performed -= OnAbility;
            }
        }

        public void SetEmberMode(bool enabled)
        {
            if (IsOwner)
            {
                _emberMode.Value = enabled;
            }
        }

        // Visual: só interpola o peso do IK, sem regra de jogo.
        private void LateUpdate()
        {
            float target = _anchored.Value ? 1f : 0f;
            float step = ikBlendSpeed * Time.deltaTime;
            if (leftHandIk != null)
            {
                leftHandIk.weight = Mathf.MoveTowards(leftHandIk.weight, target, step);
            }

            if (rightHandIk != null)
            {
                rightHandIk.weight = Mathf.MoveTowards(rightHandIk.weight, target, step);
            }
        }

        private void OnAbility(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            _anchored.Value = !_anchored.Value;
        }

        private void OnDisable()
        {
            if (IsSpawned && IsOwner && _anchored.Value)
            {
                _anchored.Value = false;
            }
        }

        private void OnAnchoredChanged(bool previous, bool anchored)
        {
            if (motor != null)
            {
                motor.SpeedMultiplier = anchored ? 0f : 1f;
            }

            if (IsServer)
            {
                UpdateDoorSupport(anchored);
            }
        }

        private void UpdateDoorSupport(bool anchored)
        {
            if (!anchored)
            {
                if (_supportedDoor != null)
                {
                    _supportedDoor.ServerSetSupported(false);
                    _supportedDoor = null;
                }

                return;
            }

            int hits = Physics.OverlapSphereNonAlloc(transform.position, reachRadius, _overlap, heavyDoorMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits; i++)
            {
                HeavyDoor door = _overlap[i].GetComponentInParent<HeavyDoor>();
                if (door != null && StrengthMultiplier >= 1f)
                {
                    _supportedDoor = door;
                    door.ServerSetSupported(true);
                    AssignIkTargets(door.transform);
                    return;
                }
            }
        }

        private void AssignIkTargets(Transform door)
        {
            Transform left = door.Find("IK_Handle_L");
            Transform right = door.Find("IK_Handle_R");
            if (leftHandIk != null && left != null)
            {
                leftHandIk.data.target.SetPositionAndRotation(left.position, left.rotation);
            }

            if (rightHandIk != null && right != null)
            {
                rightHandIk.data.target.SetPositionAndRotation(right.position, right.rotation);
            }
        }
    }
}
