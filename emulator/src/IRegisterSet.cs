namespace Mips.Emulator
{
    public interface IRegisterSet
    {
        uint this[int index] { get; set; }

        uint A0 { get; set; }
        uint A1 { get; set; }
        uint A2 { get; set; }
        uint A3 { get; set; }
        uint At { get; set; }
        uint Fp { get; set; }
        uint Gp { get; set; }
        uint Hi { get; set; }
        uint K0 { get; set; }
        uint K1 { get; set; }
        uint Lo { get; set; }
        uint Ra { get; set; }
        uint S0 { get; set; }
        uint S1 { get; set; }
        uint S2 { get; set; }
        uint S3 { get; set; }
        uint S4 { get; set; }
        uint S5 { get; set; }
        uint S6 { get; set; }
        uint S7 { get; set; }
        uint Sp { get; set; }
        uint T0 { get; set; }
        uint T1 { get; set; }
        uint T2 { get; set; }
        uint T3 { get; set; }
        uint T4 { get; set; }
        uint T5 { get; set; }
        uint T6 { get; set; }
        uint T7 { get; set; }
        uint T8 { get; set; }
        uint T9 { get; set; }
        uint V0 { get; set; }
        uint V1 { get; set; }
        uint Zero { get; set; }
    }
}