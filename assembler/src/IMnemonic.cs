namespace Mips.Assembler
{
    public interface IMnemonic
    {
        /// <summary>
        /// Try to read this mnemonic starting at the specified index.
        /// If successful, return the index after the mnemonic ends.
        /// Otherwise, return the index unchanged.
        /// </summary>
        int TryRead(string code, int startIndex);
    }
}
