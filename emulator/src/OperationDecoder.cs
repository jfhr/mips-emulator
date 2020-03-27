using System;
using System.Collections.Generic;

namespace Mips.Emulator
{
    /// <summary>
    /// Decodes a 32-Bit word into the appropriate <see cref="IInstructionFormat"/>.
    /// </summary>
    public class OperationDecoder
    {
        public IInstructionFormat DecodeInstruction(uint instruction)
        {
            if ((instruction & 0b1111_1100_0000_0000_0000_0000_0000_0000) == 0)
            {
                // Use format R (register operation)
                int rs = (int)(instruction & 0b0000_0011_1110_0000_0000_0000_0000_0000) >> 21;
                int rt = (int)(instruction & 0b0000_0000_0001_1111_0000_0000_0000_0000) >> 16;
                int rd = (int)(instruction & 0b0000_0000_0000_0000_1111_1000_0000_0000) >> 11;
                int shamt = (int)(instruction & 0b0000_0000_0000_0000_0000_0111_1100_0000) >> 6;
                uint function = instruction & 0b0000_0000_0000_0000_0000_0000_0011_1111;
                return new FormatR(rs, rt, rd, shamt, function);
            }

            else if ((instruction & 0b1111_1000_0000_0000_0000_0000_0000_0000) == 0b0000_1000_0000_0000_0000_0000_0000_0000)
            {
                // Use format J (jump operation)
                uint address = (instruction & 0b0000_0011_1111_1111_1111_1111_1111_1111);
                bool link = (instruction & 0b0000_0100_0000_0000_0000_0000_0000_0000) != 0;
                return new FormatJ(address, link);
            }

            else
            {
                // Use format I (immediate operation)
                uint opcode = (instruction & 0b1111_1100_0000_0000_0000_0000_0000_0000) >> 26;
                int rs = (int)(instruction & 0b0000_0011_1110_0000_0000_0000_0000_0000) >> 21;
                int rt = (int)(instruction & 0b0000_0000_0001_1111_0000_0000_0000_0000) >> 16;
                uint value = instruction & 0b0000_0000_0000_0000_1111_1111_1111_1111;
                return new FormatI(opcode, rs, rt, value);
            }
        }
    }

    /// <summary>
    /// Encodes a <see cref="IInstructionFormat"/> into a 32-Bit word.
    /// </summary>
    public class OperationEncoder
    {
        public uint EncodeInstruction(IInstructionFormat ins) => ins switch
        {
            FormatR insR => EncodeInstruction(insR),
            FormatI insI => EncodeInstruction(insI),
            FormatJ insJ => EncodeInstruction(insJ),
            _ => throw new NotImplementedException(),
        };

        public uint EncodeInstruction(FormatR ins)
        {
            uint word = 0;
            word |= ins.Function;
            word |= ((uint)ins.Shamt << 6);
            word |= ((uint)ins.Rd << 11);
            word |= ((uint)ins.Rt << 16);
            word |= ((uint)ins.Rs << 21);
            return word;
        }

        public uint EncodeInstruction(FormatJ ins)
        {
            uint word = 0b0000_1000_0000_0000_0000_0000_0000_0000;
            word |= ins.Address;
            if (ins.Link)
            {
                word |= 0b0000_0100_0000_0000_0000_0000_0000_0000;
            }
            return word;
        }

        public uint EncodeInstruction(FormatI ins)
        {
            uint word = 0;
            word |= ins.Value;
            word |= ((uint)ins.Rt << 16);
            word |= ((uint)ins.Rs << 21);
            word |= (ins.Opcode << 26);
            return word;
        }
    }

    public interface IInstructionFormat { }

    public class FormatR : IInstructionFormat
    {
        public int Rs { get; }
        public int Rt { get; }
        public int Rd { get; }
        public int Shamt { get; }
        public uint Function { get; }

        public FormatR(int rs, int rt, int rd, int shamt, uint function)
        {
            Rs = rs;
            Rt = rt;
            Rd = rd;
            Shamt = shamt;
            Function = function;
        }

        public override string ToString() => $"FormatR(Rs={Rs}, Rt={Rt}, Rd={Rd}, Shamt={Shamt}, Function={Function})";

        public override bool Equals(object obj) => obj is FormatR r && Rs == r.Rs && Rt == r.Rt && Rd == r.Rd && Shamt == r.Shamt && Function == r.Function;
        public override int GetHashCode() => HashCode.Combine(Rs, Rt, Rd, Shamt, Function);

        public static bool operator ==(FormatR left, FormatR right) => EqualityComparer<FormatR>.Default.Equals(left, right);
        public static bool operator !=(FormatR left, FormatR right) => !(left == right);
    }

    public class FormatJ : IInstructionFormat
    {
        public uint Address { get; }
        public bool Link { get; }

        public FormatJ(uint address, bool link)
        {
            Address = address;
            Link = link;
        }

        public override string ToString() => $"FormatJ(Address={Address}, Link={Link})";

        public override bool Equals(object obj) => obj is FormatJ j && Address == j.Address && Link == j.Link;
        public override int GetHashCode() => HashCode.Combine(Address, Link);

        public static bool operator ==(FormatJ left, FormatJ right) => EqualityComparer<FormatJ>.Default.Equals(left, right);
        public static bool operator !=(FormatJ left, FormatJ right) => !(left == right);
    }

    public class FormatI : IInstructionFormat
    {
        public uint Opcode { get; }
        public int Rs { get; }
        public int Rt { get; }
        public uint Value { get; }

        public FormatI(uint opcode, int rs, int rt, uint value)
        {
            Opcode = opcode;
            Rs = rs;
            Rt = rt;
            Value = value;
        }

        public override string ToString() => $"FormatI(Opcode={Opcode}, Rs={Rs}, Rt={Rt}, Value={Value})";

        public override bool Equals(object obj) => obj is FormatI i && Opcode == i.Opcode && Rs == i.Rs && Rt == i.Rt && Value == i.Value;
        public override int GetHashCode() => HashCode.Combine(Opcode, Rs, Rt, Value);

        public static bool operator ==(FormatI left, FormatI right) => EqualityComparer<FormatI>.Default.Equals(left, right);
        public static bool operator !=(FormatI left, FormatI right) => !(left == right);
    }
}
