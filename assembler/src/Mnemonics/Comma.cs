namespace Mips.Assembler.Mnemonics
{
    /// <summary>
    /// Reads a single comma surrounded by optional whitespace.
    /// </summary>
    public class Comma : IMnemonic
    {
        private readonly Whitespace whitespace;

        public Comma(Whitespace whitespace)
        {
            this.whitespace = whitespace;
        }

        public int TryRead(string code, int startIndex)
        {
            int index = startIndex;
            index = whitespace.TryRead(code, index);
            if (index < code.Length && code[index] == ',')
            {
                index++;
                index = whitespace.TryRead(code, index);
                return index;
            }

            return startIndex;
        }
    }
}
