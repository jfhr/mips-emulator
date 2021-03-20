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
        public static IAssemblerResult AssemblerResult { get; private set; }

        static Runtime()
        {
            Code = string.Empty;
            Cpu = new Cpu();
        }

        public static void Assemble() => AssemblerResult = MipsAsm.Assemble(Code, Memory);

        /// <summary>
        /// Returns the number of newlines that occur before the given index inside the code.
        /// </summary>
        public static int CountLinesToIndex(int index)
        {
            int lines = 0;
            int i = -1;
            while (true)
            {
                i = Code.IndexOf('\n', i + 1);
                if (i == -1 || i > index)
                {
                    break;
                }
                lines++;
            }
            return lines;
        }
    }
}
