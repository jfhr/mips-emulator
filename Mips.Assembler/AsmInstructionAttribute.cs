using System;

namespace Mips.Assembler
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class AsmInstructionAttribute : Attribute
    {
        public string Pattern { get; }

        public AsmInstructionAttribute(string pattern) => Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
    }
}
