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

        public int Index { get; private set; } = -1;
        public int LineNumber { get; private set; } = -1;

        public int Length => text.Length;

        public StringEnumerator(string text)
        {
            this.text = text;
        }

        public bool MoveNext()
        {
            if (Index < text.Length)
            {
                if (Index == -1)
                {
                    LineNumber++;
                }
                else if (Current == '\n')
                {
                    LineNumber++;
                }
                Index++;
                return true;
            }
            return false;
        }

        public bool MovePrevious()
        {
            if (Index > -1)
            {
                Index--;
                if (Index == -1)
                {
                    LineNumber--;
                }
                else if (Current == '\n')
                {
                    LineNumber--;
                }
                return true;
            }
            return false;
        }

        public bool MoveTo(int target)
        {
            if (target < -1 || target >= text.Length)
            {
                return false;
            }
            
            if (target == -1)
            {
                Reset();
                return true;
            }

            while (Index < target)
            {
                MoveNext();
            }
            while (Index > target)
            {
                MovePrevious();
            }
            return true;
        }

        public void Reset()
        {
            Index = -1;
            LineNumber = -1;
        }
    }
}
