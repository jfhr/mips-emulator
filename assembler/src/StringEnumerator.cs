using System;

namespace Mips.Assembler
{
    public class StringEnumerator
    {
        private readonly string text;

        public char this[int index] => text[index];
        public char this[Index index] => text[index];
        public string this[Range range] => text[range];

        public char Current
        {
            get
            {
                if (Index == -1 || Index == text.Length)
                {
                    return '\0';
                }

                return text[Index];
            }
        }

        public int Index { get; set; } = -1;

        public StringEnumerator(string text)
        {
            this.text = text;
        }

        public bool MoveNext()
        {
            if (Index < text.Length)
            {
                Index++;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            Index = -1;
        }
    }
}
