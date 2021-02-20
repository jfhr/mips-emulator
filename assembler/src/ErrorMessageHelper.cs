using System.Collections.Generic;

namespace Mips.Assembler
{
    /// <summary>
    /// Keeps track of error messages.
    /// </summary>
    public class ErrorMessageHelper : IErrorMessageHelper
    {
        private readonly List<ErrorMessage> messages = new List<ErrorMessage>();

        public IEnumerable<ErrorMessage> Messages => messages;

        public ErrorMessageHelper() { }

        public void Add(ErrorMessageType type, int index)
        {
            messages.Add(new ErrorMessage() { Type = type, Index = index });
        }
    }
}
