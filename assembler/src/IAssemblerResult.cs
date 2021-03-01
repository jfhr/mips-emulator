using System.Collections.Generic;

namespace Mips.Assembler
{
    public interface IAssemblerResult
    {
        /// <summary>
        /// User help messages (info and errors).
        /// </summary>
        IEnumerable<Message> Messages { get; }

        /// <summary>
        /// Labels with associated values.
        /// </summary>
        IReadOnlyDictionary<string, uint> Labels { get; }
    }
}
