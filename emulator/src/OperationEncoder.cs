namespace Mips.Emulator
{
    /// <summary>
    /// Encodes an instruction from its components into a 32-Bit word.
    /// </summary>
    public static class OperationEncoder
    {
        public static uint EncodeFormatR(int rs, int rt, int rd, int shamt, uint function)
        {
            uint word = 0;
            word |= function;
            word |= ((uint)shamt << 6);
            word |= ((uint)rd << 11);
            word |= ((uint)rt << 16);
            word |= ((uint)rs << 21);
            return word;
        }

        public static uint EncodeFormatJ(uint address, bool link)
        {
            uint word = 0b0000_1000_0000_0000_0000_0000_0000_0000;
            word |= address;
            if (link)
            {
                word |= 0b0000_0100_0000_0000_0000_0000_0000_0000;
            }
            return word;
        }

        public static uint EncodeFormatI(uint opcode, int rs, int rt, uint value)
        {
            uint word = 0;
            word |= value;
            word |= ((uint)rt << 16);
            word |= ((uint)rs << 21);
            word |= (opcode << 26);
            return word;
        }
    }
}
