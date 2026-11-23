using System;

namespace Parallax.Core.Session
{
    /// <summary>
    /// Tabela de ajuste de escalabilidade-1-a-3.md. O desafio de dedução não muda; a carga de execução sim.
    /// </summary>
    [Serializable]
    public struct SessionScalingTable
    {
        public float BaseFinalActionWindowSeconds;
        public float TwoPlayerWindowBonus;
        public float SoloWindowBonus;
        public float TwoPlayerEnemyMultiplier;
        public float SoloEnemyMultiplier;

        public static SessionScalingTable Canon => new SessionScalingTable
        {
            BaseFinalActionWindowSeconds = 12f,
            TwoPlayerWindowBonus = 0.5f,
            SoloWindowBonus = 1.0f,
            TwoPlayerEnemyMultiplier = 0.8f,
            SoloEnemyMultiplier = 0.65f
        };
    }

    public static class SessionScaling
    {
        public const int MinPlayers = 1;
        public const int MaxPlayers = 3;

        public static int ClampPlayers(int humanPlayers) => Math.Min(MaxPlayers, Math.Max(MinPlayers, humanPlayers));

        public static float ActionWindowMultiplier(int humanPlayers, SessionScalingTable table)
        {
            switch (ClampPlayers(humanPlayers))
            {
                case 1: return 1f + table.SoloWindowBonus;
                case 2: return 1f + table.TwoPlayerWindowBonus;
                default: return 1f;
            }
        }

        public static float FinalActionWindowSeconds(int humanPlayers, SessionScalingTable table) =>
            table.BaseFinalActionWindowSeconds * ActionWindowMultiplier(humanPlayers, table);

        public static float EnemyMultiplier(int humanPlayers, SessionScalingTable table)
        {
            switch (ClampPlayers(humanPlayers))
            {
                case 1: return table.SoloEnemyMultiplier;
                case 2: return table.TwoPlayerEnemyMultiplier;
                default: return 1f;
            }
        }

        public static int AuxiliariesNeeded(int humanPlayers) => MaxPlayers - ClampPlayers(humanPlayers);
    }
}
