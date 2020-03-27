using System;

namespace Mips.Emulator
{
    [Serializable]
    public class CpuBreakException : Exception
    {
        public CpuBreakException() : base("The Cpu received an instruction equal to the value of the BreakValue constant") { }
        protected CpuBreakException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
