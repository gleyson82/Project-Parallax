namespace Parallax.Core.Roles
{
    /// <summary>As três classes fixas do contrato mecânico. Nunca duplicadas na mesma sessão.</summary>
    public enum ParallaxClass : byte
    {
        Goliath = 0,
        Phantom = 1,
        Cipher = 2
    }

    /// <summary>Auxiliares que cobrem o papel ausente (escalabilidade-1-a-3).</summary>
    public enum AuxiliaryId : byte
    {
        Macaco = 0,
        Pipa = 1,
        CartuchoOverride = 2
    }

    /// <summary>Fechaduras da Chave Tripla, uma por corporação.</summary>
    public enum CorporationId : byte
    {
        ThermaTek = 0,
        Kinesys = 1,
        OmniCripto = 2
    }

    public static class ParallaxRoles
    {
        public const int ClassCount = 3;

        public static AuxiliaryId AuxiliaryFor(ParallaxClass parallaxClass) => (AuxiliaryId)(byte)parallaxClass;

        public static ParallaxClass ClassCoveredBy(AuxiliaryId auxiliary) => (ParallaxClass)(byte)auxiliary;

        public static CorporationId LockOperatedBy(ParallaxClass parallaxClass) => (CorporationId)(byte)parallaxClass;

        public static ParallaxClass OperatorOf(CorporationId corporation) => (ParallaxClass)(byte)corporation;
    }
}
