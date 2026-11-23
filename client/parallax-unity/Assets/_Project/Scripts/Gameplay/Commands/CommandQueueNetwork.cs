using System;
using System.Collections;
using System.Collections.Generic;
using Parallax.Core.Commands;
using Parallax.Core.Roles;
using Parallax.Gameplay.Session;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Commands
{
    public struct NetAuxiliaryCommand : INetworkSerializeByMemcpy, IEquatable<NetAuxiliaryCommand>
    {
        public AuxiliaryId Auxiliary;
        public ulong TargetId;
        public byte Order;

        public bool Equals(NetAuxiliaryCommand other) =>
            Auxiliary == other.Auxiliary && TargetId == other.TargetId && Order == other.Order;

        public override bool Equals(object obj) => obj is NetAuxiliaryCommand other && Equals(other);

        public override int GetHashCode() => HashCode.Combine((byte)Auxiliary, TargetId, Order);
    }

    /// <summary>
    /// Fila de Comando autoritativa no servidor, replicada para todos. Qualquer jogador pode ensaiar;
    /// a execução roda na ordem marcada enquanto os humanos fazem a ação da própria classe.
    /// </summary>
    public sealed class CommandQueueNetwork : NetworkBehaviour
    {
        [SerializeField, Min(0f)] private float delayBetweenCommandsSeconds = 0.35f;

        private readonly NetworkVariable<CommandQueuePhase> _phase = new NetworkVariable<CommandQueuePhase>();
        private readonly NetworkVariable<int> _failedIndex = new NetworkVariable<int>(-1);
        private readonly List<ParallaxClass> _humanBuffer = new List<ParallaxClass>(3);
        private NetworkList<NetAuxiliaryCommand> _replicated;
        private CommandQueueModel _model;
        private SessionRoster _roster;

        public event Action QueueChanged;

        public CommandQueuePhase Phase => _phase.Value;
        public int FailedIndex => _failedIndex.Value;
        public int Count => _replicated?.Count ?? 0;

        public NetAuxiliaryCommand this[int index] => _replicated[index];

        private void Awake()
        {
            _replicated = new NetworkList<NetAuxiliaryCommand>();
            _model = new CommandQueueModel();
        }

        public override void OnNetworkSpawn()
        {
            _replicated.OnListChanged += OnReplicatedChanged;
            _phase.OnValueChanged += OnPhaseChanged;
            if (IsServer)
            {
                _roster = SessionRoster.Active;
                if (_roster != null)
                {
                    _roster.RosterChanged += SyncHumans;
                }

                SyncHumans();
            }
        }

        public override void OnNetworkDespawn()
        {
            _replicated.OnListChanged -= OnReplicatedChanged;
            _phase.OnValueChanged -= OnPhaseChanged;
            if (_roster != null)
            {
                _roster.RosterChanged -= SyncHumans;
            }
        }

        public override void OnDestroy()
        {
            _replicated?.Dispose();
            base.OnDestroy();
        }

        public void Mark(AuxiliaryId auxiliary, ulong targetId) => MarkRpc(auxiliary, targetId);

        public void Unmark(int index) => UnmarkRpc(index);

        public void Confirm() => ConfirmRpc();

        public void Rehearse() => RehearseRpc();

        public bool IsMarked(AuxiliaryId auxiliary)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_replicated[i].Auxiliary == auxiliary)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Servidor: marca um Auxiliar num alvo (validação de papel já feita pelo chamador).</summary>
        public CommandRejection ServerMark(AuxiliaryId auxiliary, ulong targetId)
        {
            if (!IsServer)
            {
                return CommandRejection.NotInRehearsal;
            }

            if (_model.Phase != CommandQueuePhase.Rehearsal)
            {
                _model.Rehearse();
            }

            CommandRejection result = _model.Mark(auxiliary, targetId);
            Publish();
            return result;
        }

        public CommandRejection ServerConfirm()
        {
            if (!IsServer)
            {
                return CommandRejection.NotInRehearsal;
            }

            CommandRejection result = _model.Confirm();
            Publish();
            return result;
        }

        /// <summary>Servidor: volta à Janela de Ensaio mantendo as marcas.</summary>
        public void ServerRehearse()
        {
            if (!IsServer)
            {
                return;
            }

            StopAllCoroutines();
            _model.Rehearse();
            Publish();
        }

        /// <summary>Chamado pelo puzzle (servidor) no momento crítico.</summary>
        public bool ServerBeginExecution()
        {
            if (!IsServer || !_model.BeginExecution())
            {
                return false;
            }

            Publish();
            StartCoroutine(ExecuteQueue());
            return true;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void MarkRpc(AuxiliaryId auxiliary, ulong targetId)
        {
            if (_model.Mark(auxiliary, targetId) == CommandRejection.None)
            {
                Publish();
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void UnmarkRpc(int index)
        {
            if (_model.Unmark(index) == CommandRejection.None)
            {
                Publish();
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ConfirmRpc()
        {
            if (_model.Confirm() == CommandRejection.None)
            {
                Publish();
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RehearseRpc()
        {
            _model.Rehearse();
            Publish();
        }

        private IEnumerator ExecuteQueue()
        {
            var wait = new WaitForSeconds(delayBetweenCommandsSeconds);
            for (int i = 0; i < _model.Commands.Count; i++)
            {
                AuxiliaryCommand command = _model.Commands[i];
                IRehearsableAction action = FindAction(command.TargetId);
                if (action == null || action.Role != ParallaxRoles.ClassCoveredBy(command.Auxiliary))
                {
                    _model.ReportResult(i, false);
                    Publish();
                    yield break;
                }

                bool? result = null;
                action.ExecuteAsAuxiliary(command.Auxiliary, success => result = success);
                while (!result.HasValue)
                {
                    yield return null;
                }

                _model.ReportResult(i, result.Value);
                Publish();
                if (!result.Value)
                {
                    yield break;
                }

                yield return wait;
            }

            _model.CompleteExecution();
            Publish();
        }

        private IRehearsableAction FindAction(ulong networkObjectId)
        {
            if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject target))
            {
                return null;
            }

            return target.GetComponentInChildren<IRehearsableAction>();
        }

        private void SyncHumans()
        {
            if (_roster != null)
            {
                _roster.CopyHumanClasses(_humanBuffer);
            }
            else
            {
                _humanBuffer.Clear();
            }

            _model.SetHumanClasses(_humanBuffer);
            Publish();
        }

        private void Publish()
        {
            if (!IsServer)
            {
                return;
            }

            _replicated.Clear();
            foreach (AuxiliaryCommand command in _model.Commands)
            {
                _replicated.Add(new NetAuxiliaryCommand { Auxiliary = command.Auxiliary, TargetId = command.TargetId, Order = command.Order });
            }

            _phase.Value = _model.Phase;
            _failedIndex.Value = _model.FailedIndex;
        }

        private void OnReplicatedChanged(NetworkListEvent<NetAuxiliaryCommand> change) => QueueChanged?.Invoke();

        private void OnPhaseChanged(CommandQueuePhase previous, CommandQueuePhase current) => QueueChanged?.Invoke();
    }
}
