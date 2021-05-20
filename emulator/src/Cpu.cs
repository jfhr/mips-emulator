using System;
using System.Threading.Tasks;

namespace Mips.Emulator
{
    public class Cpu
    {
        /// <summary>
        /// Equivalent to the trap(10) instruction.
        /// If this value is executed as an instruction, the Cpu will immediately halt execution.
        /// </summary>
        public const uint TerminateInstruction = 0b011010_00000_00000_0000000000000010u;


        /// <summary>
        /// The complete Register set, including hi and lo.
        /// </summary>
        public RegisterSet Registers { get; } = new();

        /// <summary>
        /// The computer memory. Up to 4 GiB are available,
        /// but not all virtual space may actually be 
        /// physically allocated.
        /// </summary>
        public Memory Memory { get; } = new();

        /// <summary>
        /// The program counter.
        /// </summary>
        public uint Pc { get; private set; }

        /// <summary>
        /// Reset the program counter, register set, and memory.
        /// </summary>
        public void Reset()
        {
            Registers.Reset();
            Memory.Reset();
            Pc = 0;
        }

        /// <summary>
        /// Reset the program counter, register set, and memory. Async variant.
        /// </summary>
        public async Task ResetAsync()
        {
            await Task.Run(Reset);
        }

        /// <summary>
        /// Runs Cpu cycles until the program terminates. Async variant.
        /// </summary>
        public async Task CycleUntilTerminateAsync()
        {
            await Task.Run(CycleUntilTerminate);
        }

        /// <summary>
        /// Runs Cpu cycles until the program terminates.
        /// </summary>
        public void CycleUntilTerminate()
        {
            while (CycleOnce()) ;
        }

        /// <summary>
        /// Runs one Cpu cycle, returns a value indicating if execution should continue.
        /// </summary>
        public bool CycleOnce()
        {
            // Fetch
            if (Memory.IsUntouched(Pc))
            {
                return false;
            }

            uint instruction = Memory.LoadWord(Pc);

            if (instruction == TerminateInstruction)
            {
                return false;
            }

            // Increment
            Pc += 4;

            // Execute
            if ((instruction & 0b1111_1100_0000_0000_0000_0000_0000_0000) == 0)
            {
                // Use format R (register operation)
                OperationDecoder.DecodeFormatR(instruction, out int rs, out int rt, out int rd, out int shamt, out uint function);
                ExecuteFormatR(rs, rt, rd, shamt, function);
            }

            else if ((instruction & 0b1111_1000_0000_0000_0000_0000_0000_0000) == 0b0000_1000_0000_0000_0000_0000_0000_0000)
            {
                // Use format J (jump operation)
                OperationDecoder.DecodeFormatJ(instruction, out uint address, out bool link);
                ExecuteFormatJ(address, link);
            }

            else
            {
                // Use format I (immediate operation)
                OperationDecoder.DecodeFormatI(instruction, out uint opcode, out int rs, out int rt, out uint value);
                ExecuteFormatI(opcode, rs, rt, value);
            }

            return true;
        }

        /// <summary>
        /// Execute a format R (register) instruction.
        /// </summary>
        private void ExecuteFormatR(int rs, int rt, int rd, int shamt, uint function)
        {
            switch (function)
            {
                case Functions.Add:
                    Registers[rd] = (uint)checked((int)Registers[rs] + (int)Registers[rt]);
                    break;
                case Functions.Addu:
                    Registers[rd] = Registers[rs] + Registers[rt];
                    break;
                case Functions.And:
                    Registers[rd] = Registers[rs] & Registers[rt];
                    break;
                case Functions.Div:
                    {
                        int numerator = (int)Registers[rs];
                        int denominator = (int)Registers[rt];
                        int result = numerator / denominator;
                        int remainder = numerator % denominator;
                        Registers.Lo = (uint)result;
                        Registers.Hi = (uint)remainder;
                        break;
                    }
                case Functions.Divu:
                    {
                        Registers.Lo = Registers[rs] / Registers[rt];
                        Registers.Hi = Registers[rs] % Registers[rt];
                        break;
                    }
                case Functions.Jr:
                    Pc = Registers[rs];
                    break;
                case Functions.Mfhi:
                    Registers[rd] = Registers.Hi;
                    break;
                case Functions.Mflo:
                    Registers[rd] = Registers.Lo;
                    break;
                case Functions.Mult:
                    {
                        // First convert to Int32, then to Int64
                        // If we convert directly UInt32 -> Int64, we get the unsigned value.
                        int signedS = (int)Registers[rs];
                        int signedT = (int)Registers[rt];
                        long result = (long)signedS * signedT;
                        Registers.Lo = (uint)result;
                        Registers.Hi = (uint)(result >> 32);
                        break;
                    }
                case Functions.Multu:
                    {
                        ulong result = Registers[rs] * (ulong)Registers[rt];
                        Registers.Lo = (uint)result;
                        Registers.Hi = (uint)(result >> 32);
                        break;
                    }
                case Functions.Or:
                    Registers[rd] = Registers[rs] | Registers[rt];
                    break;
                case Functions.Sll:
                    Registers[rd] = Registers[rt] << shamt;
                    break;
                case Functions.Sllv:
                    Registers[rd] = Registers[rt] << (int)Registers[rs];
                    break;
                case Functions.Slt:
                    Registers[rd] = ((int)Registers[rs] < (int)Registers[rt]) ? 1U : 0U;
                    break;
                case Functions.Sltu:
                    Registers[rd] = (Registers[rs] < Registers[rt]) ? 1U : 0U;
                    break;
                case Functions.Sra:
                    Registers[rd] = (uint)((int)Registers[rt] >> shamt);
                    break;
                case Functions.Srl:
                    Registers[rd] = Registers[rt] >> shamt;
                    break;
                case Functions.Srlv:
                    Registers[rd] = Registers[rt] >> (int)Registers[rs];
                    break;
                case Functions.Sub:
                    {
                        int minuend = (int)Registers[rs];
                        int subtrahend = (int)Registers[rt];
                        Registers[rd] = (uint)checked(minuend - subtrahend);
                        break;
                    }
                case Functions.Subu:
                    Registers[rd] = Registers[rs] - Registers[rt];
                    break;
                case 0b001100:  // TODO syscall
                    throw new NotImplementedException();
                case Functions.Xor:
                    Registers[rd] = Registers[rs] ^ Registers[rt];
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Execute a format J instruction (j or jal).
        /// </summary>
        private void ExecuteFormatJ(uint address, bool link)
        {
            uint pcUpper4Bit = Pc & 0xF0000000;
            if (link)
            {
                Link();
            }
            Pc = pcUpper4Bit | (address << 2);
        }

        /// <summary>
        /// Execute a format I (immediate) instruction.
        /// </summary>
        private void ExecuteFormatI(uint opcode, int rs, int rt, uint value)
        {
            switch (opcode)
            {
                case Opcodes.Addi:
                    Registers[rt] = (uint)checked((int)Registers[rs] + (int)value);
                    break;
                case Opcodes.Addiu:
                    Registers[rt] = Registers[rs] + value;
                    break;
                case Opcodes.Andi:
                    Registers[rt] = Registers[rs] & value;
                    break;
                case Opcodes.Beq:
                    if (Registers[rt] == Registers[rs])
                    {
                        BranchTo(value);
                    }
                    break;
                case 0b000001:  // multiple branch instructions TODO add these to assembler
                    switch (rt)
                    {
                        case 0b00001:  // bgez
                            if ((int)Registers[rs] >= 0)
                            {
                                BranchTo(value);
                            }
                            break;
                        case 0b10001:  // bgezal
                            if ((int)Registers[rs] >= 0)
                            {
                                Link();
                                BranchTo(value);
                            }
                            break;
                        case 0b00000:  // bltz
                            if ((int)Registers[rs] < 0)
                            {
                                BranchTo(value);
                            }
                            break;
                        case 0b10000:   // bltzal
                            if ((int)Registers[rs] < 0)
                            {
                                Link();
                                BranchTo(value);
                            }
                            break;
                    }
                    break;
                case Opcodes.Bgtz:
                    if ((int)Registers[rs] > 0)
                    {
                        BranchTo(value);
                    }
                    break;
                case Opcodes.Blez:
                    if ((int)Registers[rs] <= 0)
                    {
                        BranchTo(value);
                    }
                    break;
                case Opcodes.Bne:
                    if (Registers[rs] != Registers[rt])
                    {
                        BranchTo(value);
                    }
                    break;
                case Opcodes.Lb:
                    {
                        uint address = value + Registers[rs];
                        Registers[rt] = Memory[address];
                        break;
                    }
                case Opcodes.Lui:
                    Registers[rt] = (value << 16);
                    break;
                case Opcodes.Lw:
                    {
                        uint address = value + Registers[rs];
                        Registers[rt] = Memory.LoadWord(address);
                        break;
                    }
                case Opcodes.Ori:
                    Registers[rt] = Registers[rs] | value;
                    break;
                case Opcodes.Sb:
                    {
                        uint address = value + Registers[rs];
                        Memory[address] = (byte)Registers[rt];
                        break;
                    }
                case Opcodes.Slti:
                    if ((int)Registers[rs] < (int)value)
                    {
                        Registers[rt] = 1;
                    }
                    else
                    {
                        Registers[rt] = 0;
                    }
                    break;
                case Opcodes.Sltiu:
                    if (Registers[rs] < value)
                    {
                        Registers[rt] = 1;
                    }
                    else
                    {
                        Registers[rt] = 0;
                    }
                    break;
                case Opcodes.Sw:
                    {
                        uint address = value + Registers[rs];
                        Memory.StoreWord(address, Registers[rt]);
                        break;
                    }
                case Opcodes.Xori:
                    Registers[rt] = Registers[rs] ^ value;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Branch to an instruction at the specified offset
        /// from the current instruction. The offset is 
        /// interpreted as a signed 16-bit integer.
        /// </summary>
        /// <remarks>
        /// For convenience, the offset can be given as a uint.
        /// Nonetheless, it is interpreted as a signed int.
        /// </remarks>
        private void BranchTo(uint offset)
        {
            offset <<= 2;
            // offset has 16 bits, 18 after shifting
            // we may have to carry the sign bit 
            if ((offset & 0b0000_0000_0000_0010_0000_0000_0000_0000) != 0)
            {
                offset |= 0b1111_1111_1111_1100_0000_0000_0000_0000;
            }
            int signedOffset = (int)offset;
            Pc = (uint)(Pc + signedOffset);
        }

        /// <summary>
        /// Writes the current value of <see cref="Pc"/> 
        /// to the $ra register.
        /// </summary>
        private void Link()
        {
            Registers.Ra = Pc;
        }
    }
}
