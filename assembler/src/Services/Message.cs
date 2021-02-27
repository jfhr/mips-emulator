using System;

namespace Mips.Assembler.Services
{
    public class Message
    {
        public bool IsError { get; set; }
        public string Text { get; set; }
        public int Index { get; set; }

        public Message(bool isError, string text, int index)
        {
            IsError = isError;
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Index = index;
        }
    }
}
