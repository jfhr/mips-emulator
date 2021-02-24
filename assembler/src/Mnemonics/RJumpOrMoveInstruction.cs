using Mips.Assembler.Services;
using Mips.Emulator;

namespace Mips.Assembler.Mnemonics
{
    /// <summary>
    /// Format R with 1 register ($s)
    /// </summary>
    public class RJumpOrMoveInstruction : AbsInstruction
    {
        private readonly uint function;

        public RJumpOrMoveInstruction(
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
                register
            };
            this.function = function;
        }

        protected override IMnemonic[] Parameters { get; }

        protected override bool TryEncode(out uint value)
        {
            if (parameterQueue.TryGetSigned(out int rs))
            {
                value = OperationEncoder.EncodeFormatR(rs, 0, 0, 0, function);
                return true;
            }
            value = 0;
            return false;
        }
    }
}
