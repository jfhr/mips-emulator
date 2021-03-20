using System.Collections.Generic;
using System.Linq;

namespace Mips.Assembler
{
    public interface IAssemblerResult
    {
        /// <summary>
        /// User help messages (info and errors).
        /// </summary>
        IEnumerable<Message> Messages { get; }

        /// <summary>
        /// User help messages (errors only).
        /// </summary>
        public IEnumerable<Message> Errors => Messages?.Where(x => x.IsError);

        /// <summary>
        /// Labels with associated values.
        /// </summary>
        IReadOnlyDictionary<string, uint> Labels { get; }
    }
}
