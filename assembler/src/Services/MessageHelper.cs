using System.Collections.Generic;

namespace Mips.Assembler.Services
{
    public class MessageHelper : IMessageHelper
    {
        private readonly List<Message> messages = new List<Message>();

        public IEnumerable<Message> Messages => messages;

        public MessageHelper() { }

        public void AddError(string text, int index)
        {
            messages.Add(new Message(true, text, index));
        }

        public void AddInfo(string text, int index)
        {
            messages.Add(new Message(false, text, index));
        }
    }
}
