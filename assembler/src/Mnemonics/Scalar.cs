using Mips.Assembler.Services;
using System.Globalization;

namespace Mips.Assembler.Mnemonics
{
    public class Scalar : IMnemonic
    {
        private readonly IParameterQueue parameterQueue;

        public Scalar(IParameterQueue parameterQueue)
        {
            this.parameterQueue = parameterQueue;
        }

        // TODO implement + test scalar
        public int TryRead(string code, int startIndex)
        {
            if (startIndex >= code.Length)
            {
                return startIndex;
            }

            int index = startIndex;
            char c;

            if (index + 2 < code.Length)
            {
                string start = code.Substring(index, 2);
                if (start == "0x" || start == "0X")
                {
                    // hexadecimal
                    index += 2;
                    do
                    {
                        c = code[index];
                        index++;
                    } while (index < code.Length && IsHexDigit(c));

                    string hexNumber = code.Substring(startIndex + 2, index - (startIndex + 2));
                    if (int.TryParse(hexNumber, NumberStyles.HexNumber, null, out int hexValue))
                    {
                        parameterQueue.AddSigned(hexValue);
                        return index;
                    }
                    else
                    {
                        return startIndex;
                    }
                }
            }

            // decimal
            if (code[index] == '-')
            {
                index++;
            }

            do
            {
                c = code[index];
                index++;
            } while (index < code.Length && IsDecDigit(c));

            string number = code.Substring(startIndex, index - startIndex);
            if (int.TryParse(number, NumberStyles.Integer, null, out int value))
            {
                parameterQueue.AddSigned(value);
                return index;
            }

            return startIndex;
        }

        private bool IsHexDigit(char c) => '0' <= c && c <= '9' || 'a' <= c && c <= 'f' || 'A' <= c && c <= 'F';
        private bool IsDecDigit(char c) => '0' <= c && c <= '9';
    }
}
