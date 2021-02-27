using System;

namespace Mips.Assembler.Mnemonics
{
    public abstract class AbsInstruction : IMnemonic
    {
        protected readonly InstructionDescriptor ins;
        protected readonly AssemblerServiceContainer services;

        protected abstract IMnemonic[] Parameters { get; }


        public AbsInstruction(InstructionDescriptor ins, AssemblerServiceContainer services)
        {
            this.ins = ins;
            this.services = services;
        }

        public int TryRead(string code, int startIndex)
        {
            // clear parameters
            services.ParameterQueue.Clear();

            // read instruction name
            for (int i = 0; i < ins.Name.Length; i++)
            {
                if (startIndex + i >= code.Length || ins.Name[i] != code[startIndex + i])
                {
                    return startIndex;
                }
            }

            // read parameters
            int index = startIndex + ins.Name.Length;
            index = services.Whitespace.TryRead(code, index);

            for (int p = 0; p < Parameters.Length; p++)
            {
                if (index == (index = Parameters[p].TryRead(code, index)))
                {
                    // failed to read parameter
                    return startIndex;
                }

                // if this isn't the last parameter, try to read a comma
                if (p != Parameters.Length - 1 && index == (index = services.Comma.TryRead(code, index)))
                {
                    // failed to read comma
                    return startIndex;
                }
            }

            // instruction read successful
            if (TryEncode(out uint value))
            {
                services.BinaryCodeWriter.WriteWord(value);
                services.MessageHelper.AddInfo(ins.Description, startIndex);
                return index;
            }

            // instruction encode failed (this shouldn't happen)
            throw new InvalidOperationException("Instruction read successful but instruction encode failed.");
        }

        protected abstract bool TryEncode(out uint value);
    }
}
