using Mips.Assembler;
using Mips.Emulator;
using System.Threading.Tasks;

namespace Mips.App
{
    public class Runtime
    {
        public string Code { get; set; }
        public Cpu Cpu { get; }
        public IMemory Memory => Cpu.Memory;
        public RegisterSet RegisterSet => Cpu.Registers;

        public Runtime()
        {
            Code = string.Empty;
            Cpu = new();
        }

        public void Assemble() => MipsAsm.Assemble(Code, Memory);
        public async Task AssembleAsync() => await MipsAsm.AssembleAsync(Code, Memory);
    }
}