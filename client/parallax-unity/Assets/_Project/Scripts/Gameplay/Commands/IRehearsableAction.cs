using System;
using Parallax.Core.Roles;

namespace Parallax.Gameplay.Commands
{
    /// <summary>
    /// Toda ação de puzzle de três pontas implementa este contrato (Janela de Ensaio, Fila de Comando e
    /// execução simultânea). O Auxiliar só repete o que foi marcado; a dedução continua com o jogador.
    /// Implementações vivem em NetworkBehaviours e só executam no servidor.
    /// </summary>
    public interface IRehearsableAction
    {
        /// <summary>Classe dona da ação; o Auxiliar que a cobre é ParallaxRoles.AuxiliaryFor(Role).</summary>
        ParallaxClass Role { get; }

        /// <summary>Identificador estável na rede (NetworkObjectId) para a Fila de Comando apontar o alvo.</summary>
        ulong RehearsalTargetId { get; }

        bool IsSatisfied { get; }

        void ExecuteAsAuxiliary(AuxiliaryId auxiliary, Action<bool> onFinished);
    }
}
