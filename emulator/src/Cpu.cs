using System;

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
        public RegisterSet Registers { get; }

        /// <summary>
        /// The computer memory. Up to 4 GiB are available,
        /// but not all virtual space may actually be 
        /// physically allocated.
        /// </summary>
        public Memory Memory { get; }

        /// <summary>
        /// The program counter.
        /// </summary>
        public uint Pc { get; private set; }

        public Cpu()
        {
            Registers = new RegisterSet();
            Memory = new Memory();
        }

        /// <summary>
        /// Runs Cpu cycles until the program terminates.
        /// </summary>
        public void CycleUntilTerminate()
        {
            try
            {
                while (true) CycleOnce();
            }
            catch (ProgramTerminatedException) { }
        }

        /// <summary>
        /// Runs one Cpu cycle, returns a value indicating if execution should continue.
        /// </summary>
        public void CycleOnce()
        {
            // Fetch
            uint instruction = Memory.LoadWord(Pc);

            if (instruction == TerminateInstruction)
            {
                throw new ProgramTerminatedException();
            }

            // Increment
            Pc += 4;

            // Execute
            if ((instruction & 0b1111_1100_0000_0000_0000_0000_0000_0000) == 0)
            {
                // Use format R (register operation)
                int rs = (int)(instruction & 0b0000_0011_1110_0000_0000_0000_0000_0000) >> 21;
                int rt = (int)(instruction & 0b0000_0000_0001_1111_0000_0000_0000_0000) >> 16;
                int rd = (int)(instruction & 0b0000_0000_0000_0000_1111_1000_0000_0000) >> 11;
                int shamt = (int)(instruction & 0b0000_0000_0000_0000_0000_0111_1100_0000) >> 6;
                uint function = instruction & 0b0000_0000_0000_0000_0000_0000_0011_1111;
                ExecuteFormatR(rs, rt, rd, shamt, function);
            }

            else if ((instruction & 0b1111_1000_0000_0000_0000_0000_0000_0000) == 0b0000_1000_0000_0000_0000_0000_0000_0000)
            {
                // Use format J (jump operation)
                uint address = (instruction & 0b0000_0011_1111_1111_1111_1111_1111_1111);
                bool link = (instruction & 0b0000_0100_0000_0000_0000_0000_0000_0000) != 0;
                ExecuteFormatJ(address, link);
            }

            else
            {
                // Use format I (immediate operation)
                uint opcode = (instruction & 0b1111_1100_0000_0000_0000_0000_0000_0000) >> 26;
                int rs = (int)(instruction & 0b0000_0011_1110_0000_0000_0000_0000_0000) >> 21;
                int rt = (int)(instruction & 0b0000_0000_0001_1111_0000_0000_0000_0000) >> 16;
                uint value = instruction & 0b0000_0000_0000_0000_1111_1111_1111_1111;
                ExecuteFormatI(opcode, rs, rt, value);
            }
        }

        /// <summary>
        /// Execute a format R (register) instruction.
        /// </summary>
        void ExecuteFormatR(int rs, int rt, int rd, int shamt, uint function)
        {
            switch (function)
            {
                case 0b100000:  // add
                    Registers[rd] = (uint)checked((int)Registers[rs] + (int)Registers[rt]);
                    break;
                case 0b100001:  // addu
                    Registers[rd] = Registers[rs] + Registers[rt];
                    break;
                case 0b100100:  // and                
                    Registers[rd] = Registers[rs] & Registers[rt];
                    break;
                case 0b011010:  // div
                    {
                        int numerator = (int)Registers[rs];
                        int denominator = (int)Registers[rt];
                        int result = numerator / denominator;
                        int remainder = numerator % denominator;
                        Registers.Lo = (uint)result;
                        Registers.Hi = (uint)remainder;
                        break;
                    }
                case 0b011011:  // divu
                    {
                        Registers.Lo = Registers[rs] / Registers[rt];
                        Registers.Hi = Registers[rs] % Registers[rt];
                        break;
                    }
                case 0b001000:  // jr
                    Pc = Registers[rs];
                    break;
                case 0b010000:  // mfhi
                    Registers[rd] = Registers.Hi;
                    break;
                case 0b010010:  // mflo
                    Registers[rd] = Registers.Lo;
                    break;
                case 0b011000:  // mult
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
                case 0b011001:  // multu
                    {
                        ulong result = Registers[rs] * (ulong)Registers[rt];
                        Registers.Lo = (uint)result;
                        Registers.Hi = (uint)(result >> 32);
                        break;
                    }
                case 0b100101:  // or
                    Registers[rd] = Registers[rs] | Registers[rt];
                    break;
                case 0b000000:  // sll
                    Registers[rd] = Registers[rt] << shamt;
                    break;
                case 0b000100:  // sllv
                    Registers[rd] = Registers[rt] << (int)Registers[rs];
                    break;
                case 0b101010:  //slt
                    Registers[rd] = ((int)Registers[rs] < (int)Registers[rt]) ? 1U : 0U;
                    break;
                case 0b101011:  //sltu
                    Registers[rd] = (Registers[rs] < Registers[rt]) ? 1U : 0U;
                    break;
                case 0b000011:  // sra
                    Registers[rd] = (uint)((int)Registers[rt] >> shamt);
                    break;
                case 0b000010:  // srl
                    Registers[rd] = Registers[rt] >> shamt;
                    break;
                case 0b000110:  // srlv
                    Registers[rd] = Registers[rt] >> (int)Registers[rs];
                    break;
                case 0b100010:  // sub
                    {
                        int minuend = (int)Registers[rs];
                        int subtrahend = (int)Registers[rt];
                        Registers[rd] = (uint)checked(minuend - subtrahend);
                        break;
                    }
                case 0b100011:  // subu
                    Registers[rd] = Registers[rs] - Registers[rt];
                    break;
                case 0b001100:  // TODO syscall
                    throw new NotImplementedException();
                case 0b100110:  // xor
                    Registers[rd] = Registers[rs] ^ Registers[rt];
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Execute a format J instruction (j or jal).
        /// </summary>
        void ExecuteFormatJ(uint address, bool link)
        {
            uint pc_upper_4_bit = Pc & 0xF0000000;
            if (link)
            {
                Link();
            }
            Pc = pc_upper_4_bit | (address << 2);
        }

        /// <summary>
        /// Execute a format I (immediate) instruction.
        /// </summary>
        private void ExecuteFormatI(uint opcode, int rs, int rt, uint value)
        {
            switch (opcode)
            {
                case 0b001000:  // addi
                    Registers[rt] = (uint)checked((int)Registers[rs] + (int)value);
                    break;
                case 0b001001:  // addiu
                    Registers[rt] = Registers[rs] + value;
                    break;
                case 0b001100:  // andi
                    Registers[rt] = Registers[rs] & value;
                    break;
                case 0b000100:  // beq
                    if (Registers[rt] == Registers[rs])
                    {
                        BranchTo(value);
                    }
                    break;
                case 0b000001:  // multiple branch instructions
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
                case 0b000111:  // bgtz
                    if ((int)Registers[rs] > 0)
                    {
                        BranchTo(value);
                    }
                    break;
                case 0b000110:  // blez
                    if ((int)Registers[rs] <= 0)
                    {
                        BranchTo(value);
                    }
                    break;
                case 0b000101:  // bne
                    if (Registers[rs] != Registers[rt])
                    {
                        BranchTo(value);
                    }
                    break;
                case 0b100000:  // lb
                    {
                        uint address = value + Registers[rs];
                        Registers[rt] = Memory[address];
                        break;
                    }
                case 0b001111:  // lui
                    Registers[rt] = (value << 16);
                    break;
                case 0b100011:  // lw
                    {
                        uint address = value + Registers[rs];
                        Registers[rt] = Memory.LoadWord(address);
                        break;
                    }
                case 0b001101:  // ori
                    Registers[rt] = Registers[rs] | value;
                    break;
                case 0b101000:  // sb
                    {
                        uint address = value + Registers[rs];
                        Memory[address] = (byte)Registers[rt];
                        break;
                    }
                case 0b001010:  // slti
                    if ((int)Registers[rs] < (int)value)
                    {
                        Registers[rt] = 1;
                    }
                    else
                    {
                        Registers[rt] = 0;
                    }
                    break;
                case 0b001011:  // sltiu
                    if (Registers[rs] < value)
                    {
                        Registers[rt] = 1;
                    }
                    else
                    {
                        Registers[rt] = 0;
                    }
                    break;
                case 0b101011:  // sw
                    {
                        uint address = value + Registers[rs];
                        Memory.StoreWord(address, Registers[rt]);
                        break;
                    }
                case 0b001110:  // xori
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
            int signed_offset = (int)offset;
            Pc = (uint)(Pc + signed_offset);
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
