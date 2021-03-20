namespace Mips.Emulator
{
    public static class Functions
    {
        public const uint Add = 0b100000;
        public const uint Addu = 0b100001;
        public const uint And = 0b100100;
        public const uint Div = 0b011010;
        public const uint Divu = 0b011011;
        public const uint Jalr = 0b001001;
        public const uint Jr = 0b001000;
        public const uint Mfhi = 0b010000;
        public const uint Mflo = 0b010010;
        public const uint Mult = 0b011000;
        public const uint Multu = 0b011001;
        public const uint Nor = 0b100111;
        public const uint Or = 0b100101;
        public const uint Sll = 0b000000;
        public const uint Sllv = 0b000100;
        public const uint Slt = 0b101010;
        public const uint Sltu = 0b101011;
        public const uint Sra = 0b000011;
        public const uint Srav = 0b000111;
        public const uint Srl = 0b000010;
        public const uint Srlv = 0b000110;
        public const uint Sub = 0b100010;
        public const uint Subu = 0b100011;
        public const uint Xor = 0b100110;
    }

    public static class Opcodes
    {
        public const uint Addi = 0b001000;
        public const uint Addiu = 0b001001;
        public const uint Andi = 0b001100;
        public const uint Beq = 0b000100;
        public const uint Bgtz = 0b000111;
        public const uint Blez = 0b000110;
        public const uint Bne = 0b000101;
        public const uint J = 0b000010;
        public const uint Jal = 0b000011;
        public const uint Lb = 0b100000;
        public const uint Lbu = 0b100100;
        public const uint Lh = 0b100001;
        public const uint Lhu = 0b100101;
        public const uint Lui = 0b001111;
        public const uint Lw = 0b100011;
        public const uint Ori = 0b001101;
        public const uint Sb = 0b101000;
        public const uint Sh = 0b101001;
        public const uint Slti = 0b001010;
        public const uint Sltiu = 0b001011;
        public const uint Sw = 0b101011;
        public const uint Xori = 0b001110;
    }

    public static class Constants
    {
        public static readonly string[] RegisterNames = 
        {
            "zero", "at",
            "v0", "v1",
            "a0", "a1", "a2", "a3",
            "t0", "t1", "t2", "t3", "t4", "t5", "t6", "t7",
            "s0", "s1", "s2", "s3", "s4", "s5", "s6", "s7",
            "t8", "t9",
            "k0", "k1",
            "gp", "sp", "fp", "ra",
        };
    }
}
