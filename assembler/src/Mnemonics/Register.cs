using Mips.Assembler.Services;
using System;
using System.Globalization;

namespace Mips.Assembler.Mnemonics
{
    /// <summary>
    /// Reads a register reference and places the register index in a parameter queue.
    /// </summary>
    public class Register : IMnemonic
    {
        /// <summary>
        /// Named register, the index of a name in the array is equal to the
        /// associated register index.
        /// </summary>
        private static readonly string[] registerNames = new string[]
        {
            null, null,
            "v0", "v1",
            "a0", "a1", "a2", "a3",
            "t0", "t1", "t2", "t3", "t4", "t5", "t6", "t7",
            "s0", "s1", "s2", "s3", "s4", "s5", "s6", "s7",
            "t8", "t9",
            null, null, null,
            "sp", "fp", "ra",
        };

        private readonly IParameterQueue parameterQueue;

        public Register(IParameterQueue parameterQueue)
        {
            this.parameterQueue = parameterQueue;
        }

        public int TryRead(string code, int startIndex)
        {
            int index = startIndex;
            if (index < code.Length && code[startIndex] == '$')
            {
                index++;
                if (index + 1 < code.Length)
                {
                    string reference = code.Substring(index, 2);
                    // NumberStyles.None bc we don't allow leading or trailing stuff
                    if (int.TryParse(reference, NumberStyles.None, null, out int regIndex))
                    {
                        if (0 > regIndex || regIndex >= 32)
                        {
                            return startIndex;
                        }
                        parameterQueue.AddSigned(regIndex);
                        return index + 2;
                    }
                    int regNameIndex = Array.IndexOf(registerNames, reference);
                    if (regNameIndex != -1)
                    {
                        parameterQueue.AddSigned(regNameIndex);
                        return index + 2;
                    }
                }
                if (index < code.Length)
                {
                    string reference = code.Substring(index, 1);
                    if (int.TryParse(reference, NumberStyles.None, null, out int regIndex))
                    {
                        parameterQueue.AddSigned(regIndex);
                        return index + 1;
                    }
                }
            }

            return startIndex;
        }
    }
}
