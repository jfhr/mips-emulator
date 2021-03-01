using System.Collections.Generic;

namespace Mips.Assembler
{
    public interface IAssemblerResult
    {
        IEnumerable<Message> Messages { get; }
        IReadOnlyDictionary<string, uint> Labels { get; }
    }
}
