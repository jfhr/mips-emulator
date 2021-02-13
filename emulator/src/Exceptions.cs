using System;

namespace Mips.Emulator
{
    [Serializable]
    public class UnknownInstructionException : Exception
    {
        public UnknownInstructionException() : base("The Cpu received an instruction that does not equal any known instruction") { }
        protected UnknownInstructionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class ProgramTerminatedException : Exception
    {
        public ProgramTerminatedException() : base("The Cpu received a trap(10) instruction that required it to terminate immediately") { }
        protected ProgramTerminatedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
