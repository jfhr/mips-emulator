using System.Collections.Generic;

namespace Mips.Assembler.Services
{
    /// <summary>
    /// Keeps track of messages (information and errors).
    /// </summary>
    public interface IMessageHelper
    {
        IEnumerable<Message> Messages { get; }

        void AddError(string text, int index);
        void AddInfo(string text, int index);
    }
}