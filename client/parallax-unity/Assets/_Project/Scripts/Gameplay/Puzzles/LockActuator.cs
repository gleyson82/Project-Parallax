using System;
using System.Collections;
using Parallax.Core.Roles;
using Parallax.Gameplay.Commands;
using Parallax.Gameplay.Session;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Puzzles
{
    /// <summary>
    /// Uma fechadura da Chave Tripla. O humano da classe certa a aciona; na ausência dele,
    /// o Auxiliar correspondente executa o verbo marcado na Fila de Comando.
    /// </summary>
    public sealed class LockActuator : NetworkBehaviour, IRehearsableAction
    {
        [SerializeField] private ParallaxClass role;
        [SerializeField, Min(0f)] private float auxiliaryVerbSeconds = 1.5f;

        // Ordem importa: os clientes recebem as variáveis na ordem de declaração.
        private readonly NetworkVariable<bool> _engagedByAuxiliary = new NetworkVariable<bool>();
        private readonly NetworkVariable<bool> _engaged = new NetworkVariable<bool>();

        public event Action<LockActuator> ServerActivated;
        public event Action<bool> EngagedChanged;

        /// <summary>Servidor: um Auxiliar começou a executar o verbo desta trava.</summary>
        public event Action<LockActuator> ServerAuxiliaryStarted;

        public ParallaxClass Role => role;
        public ulong RehearsalTargetId => NetworkObjectId;
        public bool IsSatisfied => _engaged.Value;
        public bool EngagedByAuxiliary => _engagedByAuxiliary.Value;
        public float AuxiliaryVerbSeconds => auxiliaryVerbSeconds;

        public override void OnNetworkSpawn() => _engaged.OnValueChanged += OnEngagedChanged;

        public override void OnNetworkDespawn() => _engaged.OnValueChanged -= OnEngagedChanged;

        /// <summary>Chamado pelo cliente da classe dona da fechadura.</summary>
        public void RequestActivate() => ActivateRpc();

        /// <summary>Servidor: um humano da classe certa (validado pelo chamador) acionou a trava.</summary>
        public void ServerEngage() => Engage(false);

        public void ServerReset()
        {
            if (IsServer)
            {
                _engaged.Value = false;
                _engagedByAuxiliary.Value = false;
            }
        }

        public void ExecuteAsAuxiliary(AuxiliaryId auxiliary, Action<bool> onFinished)
        {
            if (!IsServer || ParallaxRoles.ClassCoveredBy(auxiliary) != role)
            {
                onFinished?.Invoke(false);
                return;
            }

            ServerAuxiliaryStarted?.Invoke(this);
            StartCoroutine(AuxiliaryRoutine(onFinished));
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ActivateRpc(RpcParams rpcParams = default)
        {
            SessionRoster roster = SessionRoster.Active;
            if (roster == null || !roster.TryGetClassOf(rpcParams.Receive.SenderClientId, out ParallaxClass senderClass) || senderClass != role)
            {
                return;
            }

            Engage(false);
        }

        private IEnumerator AuxiliaryRoutine(Action<bool> onFinished)
        {
            yield return new WaitForSeconds(auxiliaryVerbSeconds);
            Engage(true);
            onFinished?.Invoke(true);
        }

        private void Engage(bool byAuxiliary)
        {
            if (!IsServer)
            {
                return;
            }

            _engagedByAuxiliary.Value = byAuxiliary;
            _engaged.Value = true;
            ServerActivated?.Invoke(this);
        }

        private void OnEngagedChanged(bool previous, bool current) => EngagedChanged?.Invoke(current);
    }
}
