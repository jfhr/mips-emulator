using Mips.Assembler.Services;
using System;

namespace Mips.Assembler.Mnemonics
{
    public abstract class AbsInstruction : IMnemonic
    {
        protected abstract IMnemonic[] Parameters { get; }

        private readonly string instructionName;
        protected readonly IParameterQueue parameterQueue;
        protected readonly ILabelRegistry labelRegistry;
        protected readonly IBinaryCodeWriter binaryCodeWriter;

        protected readonly IMnemonic whitespace;
        protected readonly IMnemonic comma;

        public AbsInstruction(
            string instructionName,
            IParameterQueue parameterQueue,
            ILabelRegistry labelRegistry,
            IBinaryCodeWriter binaryCodeWriter,
            IMnemonic whitespace,
            IMnemonic comma)
        {
            this.instructionName = instructionName;
            this.parameterQueue = parameterQueue;
            this.labelRegistry = labelRegistry;
            this.binaryCodeWriter = binaryCodeWriter;
            this.whitespace = whitespace;
            this.comma = comma;
        }

        public int TryRead(string code, int startIndex)
        {
            // clear parameters
            parameterQueue.Clear();

            // read instruction name
            for (int i = 0; i < instructionName.Length; i++)
            {
                if (startIndex + i >= code.Length || instructionName[i] != code[startIndex + i])
                {
                    return startIndex;
                }
            }

            // read parameters
            int index = startIndex + instructionName.Length;
            index = whitespace.TryRead(code, index);

            for (int p = 0; p < Parameters.Length; p++)
            {
                if (index == (index = Parameters[p].TryRead(code, index)))
                {
                    // failed to read parameter
                    return startIndex;
                }

                // if this isn't the last parameter, try to read a comma
                if (p != Parameters.Length - 1 && index == (index = comma.TryRead(code, index)))
                {
                    // failed to read comma
                    return startIndex;
                }
            }

            // instruction read successful
            if (TryEncode(out uint value))
            {
                binaryCodeWriter.WriteWord(value);
                return index;
            }

            // instruction encode failed (this shouldn't happen)
            throw new InvalidOperationException("Instruction read successful but instruction encode failed.");
        }

        protected abstract bool TryEncode(out uint value);
    }
}
