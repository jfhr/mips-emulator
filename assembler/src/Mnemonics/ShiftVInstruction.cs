using Mips.Emulator;

namespace Mips.Assembler.Mnemonics
{
    /// <summary>
    /// Format R with 3 registers ($d, $s, $t)
    /// </summary>
    public class ShiftVInstruction : AbsInstruction
    {
        public ShiftVInstruction(InstructionDescriptor ins, AssemblerServiceContainer services) : base(ins, services)
        {
            Parameters = new IMnemonic[]
            {
                services.Register,
                services.Register,
                services.Register,
            };
        }

        protected override IMnemonic[] Parameters { get; }

        protected override bool TryEncode(out uint value)
        {
            if (services.ParameterQueue.TryGetSigned(out int rd)
                && services.ParameterQueue.TryGetSigned(out int rt)
                && services.ParameterQueue.TryGetSigned(out int rs))
            {
                value = OperationEncoder.EncodeFormatR(rs, rt, rd, 0, ins.FunctionOrOpcode);
                return true;
            }
            value = 0;
            return false;
        }
    }
}
