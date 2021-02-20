using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mips.Assembler
{
    /// <summary>
    /// A FIFO-queue of instruction parameters.
    /// </summary>
    public class ParameterQueue : IParameterQueue
    {
        /// <summary>
        /// A union-type that holds either a signed or unsigned 32-bit integer.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private struct SignedOrUnsigned32
        {
            [FieldOffset(0)] public bool IsSigned;
            [FieldOffset(1)] public int SignedValue;
            [FieldOffset(1)] public uint UnsignedValue;

            public SignedOrUnsigned32(int signedValue)
            {
                IsSigned = true;
                SignedValue = signedValue;
                UnsignedValue = 0;
            }

            public SignedOrUnsigned32(uint unsignedValue)
            {
                IsSigned = false;
                SignedValue = 0;
                UnsignedValue = unsignedValue;
            }
        }

        private Queue<SignedOrUnsigned32> values = new Queue<SignedOrUnsigned32>();

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
            values.Enqueue(new SignedOrUnsigned32(value));
        }

        public void AddUnsigned(uint value)
        {
            values.Enqueue(new SignedOrUnsigned32(value));
        }

        public bool TryGetSigned(out int value)
        {
            if (values.TryDequeue(out var p) && p.IsSigned)
            {
                value = p.SignedValue;
                return true;
            }
            value = 0;
            return false;
        }

        public bool TryGetUnsigned(out uint value)
        {
            if (values.TryDequeue(out var p) && !p.IsSigned)
            {
                value = p.UnsignedValue;
                return true;
            }
            value = 0;
            return false;
        }
    }
}
