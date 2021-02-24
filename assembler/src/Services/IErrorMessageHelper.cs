using System.Collections.Generic;

namespace Mips.Assembler.Services
{
    public interface IErrorMessageHelper
    {
        IEnumerable<ErrorMessage> Messages { get; }

        void Add(ErrorMessageType type, int index);
    }
}