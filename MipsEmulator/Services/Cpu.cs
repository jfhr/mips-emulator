using System;

namespace MipsEmulator.Services
{
    public class Cpu
    {
        private readonly OperationDecoder decoder;

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
            decoder = new OperationDecoder();
        }

        /// <summary>
        /// Fetch, increment, execute.
        /// </summary>
        public void FIE()
        {
            // Fetch
            uint instruction = Memory.LoadWord(Pc);
            IInstructionFormat format = decoder.DecodeInstruction(instruction);

            // Increment
            Pc += 4;

            // Execute
            if (format is FormatR insR)
            {
                Execute(insR);
            }
            else if (format is FormatJ insJ)
            {
                Execute(insJ);
            }
            else if (format is FormatI insI)
            {
                Execute(insI);
            }
        }

        /// <summary>
        /// Execute a format J instruction (j or jal).
        /// </summary>
        void Execute(FormatJ ins)
        {
            uint pc_upper_4_bit = Pc & 0xF0000000;
            if (ins.Link)
            {
                Link();
            }
            Pc = pc_upper_4_bit | (ins.Address << 2);
        }

        /// <summary>
        /// Execute a format R (register) instruction.
        /// </summary>
        void Execute(FormatR ins)
        {
            switch (ins.Function)
            {
            case 0b100000:  // add
                Registers[ins.Rd] = (uint)checked((int)Registers[ins.Rs] + (int)Registers[ins.Rt]);
                break;
            case 0b100001:  // addu
                Registers[ins.Rd] = Registers[ins.Rs] + Registers[ins.Rt];
                break;
            case 0b100100:  // and                
                Registers[ins.Rd] = Registers[ins.Rs] & Registers[ins.Rt];
                break;
            case 0b011010:  // div
            {
                int numerator = (int)Registers[ins.Rs];
                int denominator = (int)Registers[ins.Rt];
                int result =  numerator / denominator;
                int remainder = numerator % denominator;
                Registers.Lo = (uint)result;
                Registers.Hi = (uint)remainder;
                break;
            }
            case 0b011011:  // divu
            {
                Registers.Lo = Registers[ins.Rs] / Registers[ins.Rt];
                Registers.Hi = Registers[ins.Rs] % Registers[ins.Rt];
                break;
            }
            case 0b001000:  // jr
                Pc = Registers[ins.Rs];
                break;
            case 0b010000:  // mfhi
                Registers[ins.Rd] = Registers.Hi;
                break;
            case 0b010010:  // mflo
                Registers[ins.Rd] = Registers.Lo;
                break;
            case 0b011000:  // mult
            {
                long result = (int)Registers[ins.Rs] * (int)Registers[ins.Rt];
                Registers.Lo = (uint)result;
                Registers.Hi = (uint)(result >> 32);
                break;
            }
            case 0b011001:  // multu
            {
                ulong result = Registers[ins.Rs] * (ulong)Registers[ins.Rt];
                Registers.Lo = (uint)result;
                Registers.Hi = (uint)(result >> 32);
                break;
            }
            case 0b100101:  // or
                Registers[ins.Rd] = Registers[ins.Rs] | Registers[ins.Rd];
                break;
            case 0b000000:  // sll
                Registers[ins.Rd] = Registers[ins.Rt] << ins.Shamt;
                break;
            case 0b000100:  // sllv
                Registers[ins.Rd] = Registers[ins.Rt] << (int)Registers[ins.Rs];
                break;
            case 0b101010:  //slt
                Registers[ins.Rd] = ((int)Registers[ins.Rs] < (int)Registers[ins.Rt]) ? 1U : 0U;
                break;
            case 0b101011:  //sltu
                Registers[ins.Rd] = (Registers[ins.Rs] < Registers[ins.Rt]) ? 1U : 0U;
                break;
            case 0b000011:  // sra
                Registers[ins.Rd] = (uint)((int)Registers[ins.Rt] >> ins.Shamt);
                break;
            case 0b000010:  // srl
                Registers[ins.Rd] = Registers[ins.Rt] >> ins.Shamt;
                break;
            case 0b000110:  // srlv
                Registers[ins.Rd] = Registers[ins.Rt] >> (int)Registers[ins.Rs];
                break;
            case 0b100010:  // sub
                Registers[ins.Rd] = checked(Registers[ins.Rs] - Registers[ins.Rt]);
                break;
            case 0b100011:  // subu
                Registers[ins.Rd] = Registers[ins.Rs] - Registers[ins.Rt];
                break;
            case 0b001100:  // TODO syscall
                throw new NotImplementedException();
            case 0b100110:  // xor
                Registers[ins.Rd] = Registers[ins.Rs] ^ Registers[ins.Rt];
                break;
            }
        }

        /// <summary>
        /// Execute a format I (immediate) instruction.
        /// </summary>
        private void Execute(FormatI ins)
        {
            switch (ins.Opcode)
            {
            case 0b001000:  // addi
                Registers[ins.Rt] = (uint)checked((int)Registers[ins.Rs] + (int)ins.Value);
                break;
            case 0b001001:  // addiu
                Registers[ins.Rt] = Registers[ins.Rs] + ins.Value;
                break;
            case 0b001100:  // andi
                Registers[ins.Rt] = Registers[ins.Rs] & ins.Value;
                break;
            case 0b000100:  // beq
                if (Registers[ins.Rt] == Registers[ins.Rs])
                {
                    BranchTo(ins.Value);
                }
                break;
            case 0b000001:  // multiple branch instructions
                switch (ins.Rt)
                {
                case 0b00001:  // bgez
                    if ((int)Registers[ins.Rs] >= 0)
                    {
                        BranchTo(ins.Value);
                    }
                    break;
                case 0b10001:  // bgezal
                    if ((int)Registers[ins.Rs] >= 0)
                    {
                        Link();
                        BranchTo(ins.Value);
                    }
                    break;
                case 0b00000:  // bltz
                    if ((int)Registers[ins.Rs] < 0)
                    {
                        BranchTo(ins.Value);
                    }
                    break;
                case 0b10000:   // bltzal
                    if ((int)Registers[ins.Rs] < 0)
                    {
                        Link();
                        BranchTo(ins.Value);
                    }
                    break;
                }
                break;
            case 0b000111:  // bgtz
                if ((int)Registers[ins.Rs] > 0)
                {
                    BranchTo(ins.Value);
                }
                break;
            case 0b000110:  // blez
                if ((int)Registers[ins.Rs] <= 0)
                {
                    BranchTo(ins.Value);
                }
                break;
            case 0b000101:  // bne
                if (Registers[ins.Rs] != Registers[ins.Rt])
                {
                    BranchTo(ins.Value);
                }
                break;
            case 0b100000:  // lb
            {
                uint address = ins.Value + Registers[ins.Rs];
                Registers[ins.Rt] = Memory[address];
                break;
            }
            case 0b001111:  // lui
                Registers[ins.Rt] = (ins.Value << 16);
                break;
            case 0b100011:  // lw
            {
                uint address = ins.Value + Registers[ins.Rs];
                Registers[ins.Rt] = Memory.LoadWord(address);
                break;
            }
            case 0b001101:  // ori
                Registers[ins.Rt] = Registers[ins.Rs] | ins.Value;
                break;
            case 0b101000:  // sb
            {
                uint address = ins.Value + (uint)ins.Rs;
                Memory[address] = (byte)ins.Rt;
                break;
            }
            case 0b001010:  // slti
                if ((int)Registers[ins.Rs] < (int)ins.Value)
                {
                    Registers[ins.Rt] = 1;
                }
                else
                {
                    Registers[ins.Rt] = 0;
                }
                break;
            case 0b001011:  // sltiu
                if (Registers[ins.Rs] < ins.Value)
                {
                    Registers[ins.Rt] = 1;
                }
                else
                {
                    Registers[ins.Rt] = 0;
                }
                break;
            case 0b101011:  // sw
            {
                uint address = ins.Value + (uint)ins.Rs;
                Memory.StoreWord(address, (uint)ins.Rt);
                break;
            }
            case 0b001110:  // xori
                Registers[ins.Rt] = Registers[ins.Rs] ^ ins.Value;
                break;
            }
        }

        /// <summary>
        /// Branch to an instruction at the specified offset
        /// from the current instruction. The offset is 
        /// interpreted as signed.
        /// </summary>
        /// <remarks>
        /// For convenience, the offset can be given as a uint.
        /// Nonetheless, it is interpreted as a signed int.
        /// </remarks>
        private void BranchTo(uint offset)
        {
            offset <<= 2;
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
