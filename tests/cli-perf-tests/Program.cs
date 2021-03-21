using Mips.Assembler;
using Mips.Emulator;
using System;
using System.IO;

namespace Mips.CliPerfTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Cpu cpu = new();
            string code = File.ReadAllText("NaivePrimalityTest.asm");

            for (uint i = 0; i < 100; i++)
            {
                cpu.Reset();
                MipsAsm.Assemble(code, cpu.Memory);
                cpu.Registers.A0 = i;
                cpu.CycleUntilTerminate();
            }
        }
    }
}
