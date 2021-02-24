using Mips.Assembler.Services;
using Mips.Emulator;

namespace Mips.Assembler.Mnemonics
{
    /// <summary>
    /// Format R with 2 registers and shamt ($d, $t, shamt)
    /// </summary>
    public class ShiftInstruction : AbsInstruction
    {
        private readonly uint function;

        public ShiftInstruction(
            string instructionName,
            uint function,
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
            this.function = function;
        }

        protected override IMnemonic[] Parameters { get; }

        protected override bool TryEncode(out uint value)
        {
            if (parameterQueue.TryGetSigned(out int rd)
                && parameterQueue.TryGetSigned(out int rt)
                && parameterQueue.TryGetSigned(out int shamt))
            {
                value = OperationEncoder.EncodeFormatR(0, rt, rd, shamt, function);
                return true;
            }
            value = 0;
            return false;
        }
    }
}
