using System.Collections;
using Parallax.Gameplay.Input;
using Parallax.Gameplay.World;
using Parallax.Rendering;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Parallax.Gameplay.Characters
{
    /// <summary>
    /// Cipher: Override em terminais e máquinas. Durante a invasão ele fica cego para o mundo físico
    /// (Olhar substituído pelo terminal isolado). Com o Código-Fonte Ômega, máquinas invadidas viram
    /// aliadas e a Contagem de Vazamento sobe; em 3+, a interface falsifica uma leitura por puzzle.
    /// </summary>
    public sealed class CipherOverride : NetworkBehaviour
    {
        [SerializeField] private ParallaxInput input;
        [SerializeField] private ParallaxCharacterMotor motor;
        [SerializeField, Min(0.5f)] private float reach = 2.5f;
        [SerializeField] private bool carriesOmegaSourceCode;

        private readonly NetworkVariable<bool> _overriding = new NetworkVariable<bool>();
        private readonly NetworkVariable<int> _leakCount = new NetworkVariable<int>();
        private OverrideTerminal _current;

        public bool IsOverriding => _overriding.Value;
        public int LeakCount => _leakCount.Value;

        public override void OnNetworkSpawn()
        {
            _overriding.OnValueChanged += OnOverridingChanged;
            _leakCount.OnValueChanged += OnLeakChanged;
            if (IsOwner && input != null)
            {
                input.Ability.performed += OnAbility;
            }
        }

        public override void OnNetworkDespawn()
        {
            _overriding.OnValueChanged -= OnOverridingChanged;
            _leakCount.OnValueChanged -= OnLeakChanged;
            if (input != null)
            {
                input.Ability.performed -= OnAbility;
            }

            if (IsOwner)
            {
                ParallaxVisionState.SetOverrideBlind(false);
            }
        }

        private void OnAbility(InputAction.CallbackContext context)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            Collider[] near = Physics.OverlapSphere(transform.position, reach);
            foreach (Collider candidate in near)
            {
                OverrideTerminal terminal = candidate.GetComponentInParent<OverrideTerminal>();
                if (terminal != null && !terminal.IsBreached)
                {
                    BeginOverrideRpc(terminal.NetworkObject);
                    return;
                }
            }
        }

        [Rpc(SendTo.Server)]
        private void BeginOverrideRpc(NetworkObjectReference terminalReference)
        {
            if (_overriding.Value || !terminalReference.TryGet(out NetworkObject terminalObject))
            {
                return;
            }

            _current = terminalObject.GetComponentInChildren<OverrideTerminal>();
            if (_current == null || Vector3.Distance(_current.transform.position, transform.position) > reach * 1.5f)
            {
                return;
            }

            StartCoroutine(OverrideRoutine(_current));
        }

        private IEnumerator OverrideRoutine(OverrideTerminal terminal)
        {
            _overriding.Value = true;
            yield return new WaitForSeconds(terminal.BreachSeconds);
            terminal.ServerSetBreached(true);

            if (carriesOmegaSourceCode && terminal.Infectable)
            {
                _leakCount.Value = Mathf.Min(ParallaxVisionState.MaxLeakCount, _leakCount.Value + 1);
                OverrideDrone drone = terminal.GetComponentInParent<OverrideDrone>();
                if (drone != null)
                {
                    drone.ServerSetAllied(transform);
                }
            }

            _overriding.Value = false;
            _current = null;
        }

        private void OnOverridingChanged(bool previous, bool overriding)
        {
            if (motor != null)
            {
                motor.SpeedMultiplier = overriding ? 0f : 1f;
            }

            if (IsOwner)
            {
                ParallaxVisionState.SetOverrideBlind(overriding);
            }
        }

        private void OnLeakChanged(int previous, int current)
        {
            if (IsOwner)
            {
                ParallaxVisionState.SetLeakCount(current);
            }
        }
    }
}
