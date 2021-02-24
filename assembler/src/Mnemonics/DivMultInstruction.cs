using Mips.Assembler.Services;
using Mips.Emulator;

namespace Mips.Assembler.Mnemonics
{
    /// <summary>
    /// Format R with 2 registers ($s, $t)
    /// </summary>
    public class DivMultInstruction : AbsInstruction
    {
        private readonly uint function;

        public DivMultInstruction(
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
            };
            this.function = function;
        }

        protected override IMnemonic[] Parameters { get; }

        protected override bool TryEncode(out uint value)
        {
            if (parameterQueue.TryGetSigned(out int rs)
                && parameterQueue.TryGetSigned(out int rt))
            {
                value = OperationEncoder.EncodeFormatR(rs, rt, 0, 0, function);
                return true;
            }
            value = 0;
            return false;
        }
    }
}
