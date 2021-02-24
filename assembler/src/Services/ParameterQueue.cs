using System.Collections.Generic;

namespace Mips.Assembler.Services
{
    /// <summary>
    /// A FIFO-queue of instruction parameters.
    /// </summary>
    public class ParameterQueue : IParameterQueue
    {
        private Queue<int> values = new Queue<int>();

        public ParameterQueue() { }

        /// <summary>
        /// Clears the parameter queue, use this after finishing reading an instruction.
        /// </summary>
        public void Clear()
        {
            values.Clear();
        }

        public void AddSigned(int value)
        {
            values.Enqueue(value);
        }

        public void AddUnsigned(uint value)
        {
            int signed = unchecked((int)value);
            values.Enqueue(signed);
        }

        public bool TryGetSigned(out int value)
        {
            if (values.TryDequeue(out var p))
            {
                value = p;
                return true;
            }
            value = 0;
            return false;
        }

        public bool TryGetUnsigned(out uint value)
        {
            if (values.TryDequeue(out var p))
            {
                value = unchecked((uint)p);
                return true;
            }
            value = 0;
            return false;
        }
    }
}
