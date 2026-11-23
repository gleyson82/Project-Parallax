using System;
using System.Collections.Generic;
using Parallax.Core.Roles;

namespace Parallax.Core.Commands
{
    /// <summary>Cada Auxiliar executa um único verbo curto e nunca interpreta.</summary>
    public enum AuxiliaryVerb : byte
    {
        HoldAndLift = 0,
        RiseAndShow = 1,
        OpenOnce = 2
    }

    public enum CommandQueuePhase : byte
    {
        Rehearsal = 0,
        Confirmed = 1,
        Executing = 2,
        Completed = 3,
        Failed = 4
    }

    public enum CommandRejection : byte
    {
        None = 0,
        NotInRehearsal,
        QueueFull,
        AuxiliaryUnavailable,
        AuxiliaryAlreadyQueued,
        EmptyQueue,
        InvalidIndex
    }

    [Serializable]
    public struct AuxiliaryCommand : IEquatable<AuxiliaryCommand>
    {
        public AuxiliaryId Auxiliary;
        public ulong TargetId;
        public byte Order;

        public AuxiliaryVerb Verb => CommandQueueModel.VerbOf(Auxiliary);

        public bool Equals(AuxiliaryCommand other) =>
            Auxiliary == other.Auxiliary && TargetId == other.TargetId && Order == other.Order;

        public override bool Equals(object obj) => obj is AuxiliaryCommand other && Equals(other);

        public override int GetHashCode() => HashCode.Combine((byte)Auxiliary, TargetId, Order);
    }

    /// <summary>
    /// Ensaio e Fila de Comando (escalabilidade-1-a-3.md): marcar no cenário, enfileirar até três comandos,
    /// confirmar e executar. Nada acontece antes da confirmação. Se um Auxiliar falha, o jogador vê onde e reensaia.
    /// </summary>
    public sealed class CommandQueueModel
    {
        public const int MaxCommands = 3;

        private readonly List<AuxiliaryCommand> _commands = new List<AuxiliaryCommand>(MaxCommands);
        private readonly bool[] _auxiliaryAvailable = new bool[ParallaxRoles.ClassCount];

        public CommandQueuePhase Phase { get; private set; } = CommandQueuePhase.Rehearsal;
        public int FailedIndex { get; private set; } = -1;
        public IReadOnlyList<AuxiliaryCommand> Commands => _commands;

        public static AuxiliaryVerb VerbOf(AuxiliaryId auxiliary) => (AuxiliaryVerb)(byte)auxiliary;

        /// <summary>Um Auxiliar só existe para o papel que nenhum humano ocupa.</summary>
        public void SetHumanClasses(IEnumerable<ParallaxClass> humanClasses)
        {
            for (int i = 0; i < _auxiliaryAvailable.Length; i++)
            {
                _auxiliaryAvailable[i] = true;
            }

            foreach (ParallaxClass human in humanClasses)
            {
                _auxiliaryAvailable[(int)ParallaxRoles.AuxiliaryFor(human)] = false;
            }

            _commands.RemoveAll(c => !_auxiliaryAvailable[(int)c.Auxiliary]);
            Renumber();
        }

        public bool IsAvailable(AuxiliaryId auxiliary) => _auxiliaryAvailable[(int)auxiliary];

        public CommandRejection Mark(AuxiliaryId auxiliary, ulong targetId)
        {
            if (Phase != CommandQueuePhase.Rehearsal)
            {
                return CommandRejection.NotInRehearsal;
            }

            if (!IsAvailable(auxiliary))
            {
                return CommandRejection.AuxiliaryUnavailable;
            }

            if (_commands.Exists(c => c.Auxiliary == auxiliary))
            {
                return CommandRejection.AuxiliaryAlreadyQueued;
            }

            if (_commands.Count >= MaxCommands)
            {
                return CommandRejection.QueueFull;
            }

            _commands.Add(new AuxiliaryCommand { Auxiliary = auxiliary, TargetId = targetId, Order = (byte)_commands.Count });
            return CommandRejection.None;
        }

        public CommandRejection Unmark(int index)
        {
            if (Phase != CommandQueuePhase.Rehearsal)
            {
                return CommandRejection.NotInRehearsal;
            }

            if (index < 0 || index >= _commands.Count)
            {
                return CommandRejection.InvalidIndex;
            }

            _commands.RemoveAt(index);
            Renumber();
            return CommandRejection.None;
        }

        public CommandRejection Confirm()
        {
            if (Phase != CommandQueuePhase.Rehearsal)
            {
                return CommandRejection.NotInRehearsal;
            }

            if (_commands.Count == 0)
            {
                return CommandRejection.EmptyQueue;
            }

            Phase = CommandQueuePhase.Confirmed;
            return CommandRejection.None;
        }

        public bool BeginExecution()
        {
            if (Phase != CommandQueuePhase.Confirmed)
            {
                return false;
            }

            Phase = CommandQueuePhase.Executing;
            FailedIndex = -1;
            return true;
        }

        public void ReportResult(int index, bool success)
        {
            if (Phase != CommandQueuePhase.Executing || index < 0 || index >= _commands.Count)
            {
                return;
            }

            if (!success)
            {
                FailedIndex = index;
                Phase = CommandQueuePhase.Failed;
            }
        }

        public void CompleteExecution()
        {
            if (Phase == CommandQueuePhase.Executing)
            {
                Phase = CommandQueuePhase.Completed;
            }
        }

        /// <summary>Volta à Janela de Ensaio mantendo as marcas, para corrigir o passo que falhou.</summary>
        public void Rehearse()
        {
            Phase = CommandQueuePhase.Rehearsal;
        }

        public void Clear()
        {
            _commands.Clear();
            FailedIndex = -1;
            Phase = CommandQueuePhase.Rehearsal;
        }

        private void Renumber()
        {
            for (int i = 0; i < _commands.Count; i++)
            {
                AuxiliaryCommand command = _commands[i];
                command.Order = (byte)i;
                _commands[i] = command;
            }
        }
    }
}
