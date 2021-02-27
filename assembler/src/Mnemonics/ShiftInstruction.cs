using Mips.Emulator;

namespace Mips.Assembler.Mnemonics
{
    /// <summary>
    /// Format R with 2 registers and shamt ($d, $t, shamt)
    /// </summary>
    public class ShiftInstruction : AbsInstruction
    {
        public ShiftInstruction(InstructionDescriptor ins, AssemblerServiceContainer services) : base(ins, services)
        {
            Parameters = new IMnemonic[]
            {
                services.Register,
                services.Register,
                services.Scalar,
            };
        }

        protected override IMnemonic[] Parameters { get; }

        protected override bool TryEncode(out uint value)
        {
            if (services.ParameterQueue.TryGetSigned(out int rd)
                && services.ParameterQueue.TryGetSigned(out int rt)
                && services.ParameterQueue.TryGetSigned(out int shamt))
            {
                value = OperationEncoder.EncodeFormatR(0, rt, rd, shamt, ins.FunctionOrOpcode);
                return true;
            }
            value = 0;
            return false;
        }
    }
}
