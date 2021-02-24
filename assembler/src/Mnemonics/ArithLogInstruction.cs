using Mips.Assembler.Services;
using Mips.Emulator;

namespace Mips.Assembler.Mnemonics
{
    /// <summary>
    /// Format R with 3 registers ($d, $s, $t)
    /// </summary>
    public class ArithLogInstruction : AbsInstruction
    {
        private readonly uint function;

        public ArithLogInstruction(
            string instructionName,
            uint function,
            IParameterQueue parameterQueue,
            ILabelRegistry labelRegistry,
            IBinaryCodeWriter binaryCodeWriter,
            IMnemonic whitespace,
            IMnemonic comma,
            IMnemonic register)
            : base(instructionName, parameterQueue, labelRegistry, binaryCodeWriter, whitespace, comma)
        {
            Parameters = new IMnemonic[]
            {
                register,
                register,
                register
            };
            this.function = function;
        }

        protected override IMnemonic[] Parameters { get; }

        protected override bool TryEncode(out uint value)
        {
            if (parameterQueue.TryGetSigned(out int rd)
                && parameterQueue.TryGetSigned(out int rs)
                && parameterQueue.TryGetSigned(out int rt))
            {
                value = OperationEncoder.EncodeFormatR(rs, rt, rd, 0, function);
                return true;
            }
            value = 0;
            return false;
        }
    }
}
