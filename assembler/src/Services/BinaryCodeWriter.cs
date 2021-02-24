using Mips.Emulator;

namespace Mips.Assembler.Services
{
    public class BinaryCodeWriter : IBinaryCodeWriter
    {
        private readonly IMemory memory;

        public BinaryCodeWriter(IMemory memory)
        {
            this.memory = memory;
        }

        public uint CurrentAddress { get; private set; }

        public void WriteWord(uint value)
        {
            // get next multiple of 4
            CurrentAddress += 3;
            CurrentAddress &= 0xFFFFFFFC;

            memory.StoreWord(CurrentAddress, value);

            CurrentAddress += 4;
        }

        public void Write(byte[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                memory[CurrentAddress] = value[i];
                CurrentAddress++;
            }
        }
    }
}
