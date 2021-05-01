using Mips.Assembler;
using Mips.Emulator;

namespace Mips.App
{
    public static class Runtime
    {
        public static string Code { get; set; }
        public static Cpu Cpu { get; }
        public static IMemory Memory => Cpu.Memory;
        public static RegisterSet RegisterSet => Cpu.Registers;

        static Runtime()
        {
            Code = string.Empty;
            Cpu = new Cpu();
        }

        public static void Assemble() => MipsAsm.Assemble(Code, Memory);
    }
}