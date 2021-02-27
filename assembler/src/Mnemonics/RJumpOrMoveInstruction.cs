using Mips.Emulator;

namespace Mips.Assembler.Mnemonics
{
    /// <summary>
    /// Format R with 1 register ($s)
    /// </summary>
    public class RJumpOrMoveInstruction : AbsInstruction
    {
        public RJumpOrMoveInstruction(InstructionDescriptor ins, AssemblerServiceContainer services) : base(ins, services)
        {
            Parameters = new IMnemonic[]
            {
                services.Register,
            };
        }

        protected override IMnemonic[] Parameters { get; }

        protected override bool TryEncode(out uint value)
        {
            if (services.ParameterQueue.TryGetSigned(out int rs))
            {
                value = OperationEncoder.EncodeFormatR(rs, 0, 0, 0, ins.FunctionOrOpcode);
                return true;
            }
            value = 0;
            return false;
        }
    }
}
