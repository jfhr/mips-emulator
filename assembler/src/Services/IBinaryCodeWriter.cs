namespace Mips.Assembler.Services
{
    /// <summary>
    /// Writes binary code output into a memory instance.
    /// </summary>
    public interface IBinaryCodeWriter
    {
        /// <summary>
        /// The current (free) address.
        /// </summary>
        uint CurrentAddress { get; }

        /// <summary>
        /// Writes a word at the current address, if the current address isn't 
        /// a multiple of four, we use the next higher multiple of 4.
        /// </summary>
        void WriteWord(uint value);

        /// <summary>
        /// Write a byte array starting at the current address.
        /// </summary>
        void Write(byte[] value);
    }
}
