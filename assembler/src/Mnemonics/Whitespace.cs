namespace Mips.Assembler.Mnemonics
{
    /// <summary>
    /// Reads whitespace, including tab, newline, carriage return, etc.
    /// </summary>
    public class Whitespace : IMnemonic
    {
        public Whitespace() { }

        public int TryRead(string code, int startIndex)
        {
            while (startIndex < code.Length && char.IsWhiteSpace(code, startIndex)) startIndex++;
            return startIndex;
        }
    }
}
