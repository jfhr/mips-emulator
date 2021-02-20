using System;

namespace Mips.Assembler
{
    /// <summary>
    /// Keeps track of labels and writes instructions into the binary code output.
    /// </summary>
    public interface ITokenBroker
    {
        /// <summary>
        /// Add a label definition. If the same label was defined before, cause an error message.
        /// </summary>
        void DefineLabel(string name, int indexInCode);

        /// <summary>
        /// Called after parsing the entire code to account for labels that were used
        /// before they were defined.
        /// </summary>
        void DoRemainingLabels();

        /// <summary>
        /// Runs an action that receives the value of a given label.
        /// Runs right away if the label is already defined.
        /// If the label is never defined, this will cause an error during assembly.
        /// </summary>
        void DoWithLabel(string labelName, Action<uint> action, int indexInCode);

        /// <summary>
        /// Push an instruction into the binary code output.
        /// </summary>
        void PushInstruction(uint instruction);
    }
}