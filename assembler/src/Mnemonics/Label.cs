using Mips.Assembler.Services;

namespace Mips.Assembler.Mnemonics
{
    /// <summary>
    /// Reads a label declaration.
    /// </summary>
    public class Label : IMnemonic
    {
        private readonly ILabelRegistry labelRegistry;

        public Label(ILabelRegistry labelRegistry)
        {
            this.labelRegistry = labelRegistry;
        }

        public int TryRead(string code, int startIndex)
        {
            int index = startIndex;
            if (index < code.Length && char.IsLetter(code, index))
            {
                index++;
                while (index < code.Length && char.IsLetterOrDigit(code, index)) index++;
                if (index < code.Length && code[index] == ':')
                {
                    string name = code.Substring(startIndex, index - startIndex);
                    labelRegistry.DefineLabel(name, index);
                    return index + 1;
                }
            }
            return startIndex;
        }
    }
}
