using System;
using System.Text;

namespace Mips.Assembler
{
    static class Instructions
    {
        // We don't enforce .text and .data
        // but allow them for compatibility
        [AsmInstruction(".text")]
        [AsmInstruction(".data")]
        public static void Nop() { }

        [AsmInstruction("\\.ascii +\"(?<text>(?>\\\"|[^\"])*)\"")]
        public static byte[] Ascii(string text)
        {
            return Encoding.ASCII.GetBytes(text);
        }

        [AsmInstruction("\\.asciiz +\"(?<text>(?>\\\"|[^\"])*)\"")]
        public static byte[] AsciiZ(string text)
        {
            int length = Encoding.ASCII.GetByteCount(text) + 1;
            var bytes = new byte[length];
            Encoding.ASCII.GetBytes(text, 0, text.Length, bytes, 0);
            return bytes;
        }

        [AsmInstruction("\\.byte (?<value>-?(?>0x[0-9a-fA-F]+)|-?(?>[0-9]+)|-?(?>0b[01]+))")]
        public static byte Byte(string value)
        {
            var signedValue = sbyte.Parse(value);
            var unsignedValue = (byte)signedValue;
            return unsignedValue;
        }
    }
}
