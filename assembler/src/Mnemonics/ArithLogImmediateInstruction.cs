using Mips.Assembler.Services;
using Mips.Emulator;

namespace Mips.Assembler.Mnemonics
{
    /// <summary>
    /// Format I with 2 registers and immediate value ($t, $s, i)
    /// </summary>
    public class ArithLogImmediateInstruction : AbsInstruction
    {
        private readonly uint opcode;

        public ArithLogImmediateInstruction(
            string instructionName,
            uint opcode,
            IParameterQueue parameterQueue,
            ILabelRegistry labelRegistry,
            IBinaryCodeWriter binaryCodeWriter,
            IMnemonic whitespace,
            IMnemonic comma,
            IMnemonic register,
            IMnemonic scalar)
            : base(instructionName, parameterQueue, labelRegistry, binaryCodeWriter, whitespace, comma)
        {
            Parameters = new IMnemonic[]
            {
                register,
                register,
                scalar,
            };
            this.opcode = opcode;
        }

        protected override IMnemonic[] Parameters { get; }

        protected override bool TryEncode(out uint value)
        {
            if (parameterQueue.TryGetSigned(out int rt)
                && parameterQueue.TryGetSigned(out int rs)
                && parameterQueue.TryGetUnsigned(out uint immed))
            {
                value = OperationEncoder.EncodeFormatI(opcode, rs, rt, immed);
                return true;
            }
            value = 0;
            return false;
        }
    }
}
