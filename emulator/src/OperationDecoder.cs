namespace Mips.Emulator
{
    /// <summary>
    /// Decodes a 32-Bit instruction into its components.
    /// </summary>
    public static class OperationDecoder
    {
        public static void DecodeFormatR(uint instruction, out int rs, out int rt, out int rd, out int shamt, out uint function)
        {
            rs = (int)(instruction & 0b0000_0011_1110_0000_0000_0000_0000_0000) >> 21;
            rt = (int)(instruction & 0b0000_0000_0001_1111_0000_0000_0000_0000) >> 16;
            rd = (int)(instruction & 0b0000_0000_0000_0000_1111_1000_0000_0000) >> 11;
            shamt = (int)(instruction & 0b0000_0000_0000_0000_0000_0111_1100_0000) >> 6;
            function = instruction & 0b0000_0000_0000_0000_0000_0000_0011_1111;
        }

        public static void DecodeFormatJ(uint instruction, out uint address, out bool link)
        {
            address = (instruction & 0b0000_0011_1111_1111_1111_1111_1111_1111);
            link = (instruction & 0b0000_0100_0000_0000_0000_0000_0000_0000) != 0;
        }

        public static void DecodeFormatI(uint instruction, out uint opcode, out int rs, out int rt, out uint value)
        {
            opcode = (instruction & 0b1111_1100_0000_0000_0000_0000_0000_0000) >> 26;
            rs = (int)(instruction & 0b0000_0011_1110_0000_0000_0000_0000_0000) >> 21;
            rt = (int)(instruction & 0b0000_0000_0001_1111_0000_0000_0000_0000) >> 16;
            value = instruction & 0b0000_0000_0000_0000_1111_1111_1111_1111;
        }
    }
}
