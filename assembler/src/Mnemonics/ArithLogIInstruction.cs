using Mips.Emulator;

namespace Mips.Assembler.Mnemonics
{
    /// <summary>
    /// Format I with two registers and an immediate value ($t, $s, i)
    /// </summary>
    public class ArithLogIInstruction : AbsInstruction
    {
        public ArithLogIInstruction(InstructionDescriptor ins, AssemblerServiceContainer services) : base(ins, services)
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
            if (services.ParameterQueue.TryGetSigned(out int rt)
                && services.ParameterQueue.TryGetSigned(out int rs)
                && services.ParameterQueue.TryGetUnsigned(out uint immed))
            {
                value = OperationEncoder.EncodeFormatI(ins.FunctionOrOpcode, rs, rt, immed);
                return true;
            }
            value = 0;
            return false;
        }
    }
}
