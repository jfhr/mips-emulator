using Mips.Emulator;

namespace Mips.Assembler.Mnemonics
{
    /// <summary>
    /// Format R with 2 registers ($s, $t)
    /// </summary>
    public class DivMultInstruction : AbsInstruction
    {
        public DivMultInstruction(InstructionDescriptor ins, AssemblerServiceContainer services) : base(ins, services)
        {
            Parameters = new IMnemonic[]
            {
                services.Register,
                services.Register,
            };
        }

        protected override IMnemonic[] Parameters { get; }

        protected override bool TryEncode(out uint value)
        {
            if (services.ParameterQueue.TryGetSigned(out int rs)
                && services.ParameterQueue.TryGetSigned(out int rt))
            {
                value = OperationEncoder.EncodeFormatR(rs, rt, 0, 0, ins.FunctionOrOpcode);
                return true;
            }
            value = 0;
            return false;
        }
    }
}
