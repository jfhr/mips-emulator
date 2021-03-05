using Mips.Emulator;
using System.Collections.Generic;

namespace Mips.Assembler
{

    public static class MipsAsm
    {
        public static readonly Dictionary<string, InstructionInfo> Instructions = new Dictionary<string, InstructionInfo>();

        static MipsAsm()
        {
            InitializeInstructions();
        }

        private static void AddInstruction(InstructionSyntaxType type, string name, uint functionOrOpcode, string help)
        {
            Instructions[name] = new InstructionInfo(type, functionOrOpcode, help);
        }

        public static void InitializeInstructions()
        {
            AddInstruction(InstructionSyntaxType.ArithLog, "add", Functions.Add, "add $d,$s,$t: Add $s and $t and store the result in $d. If the calculation overflows, an error is thrown.");
            AddInstruction(InstructionSyntaxType.ArithLog, "addu", Functions.Addu, "addu $d,$s,$t: Add $s and $t and store the result in $d. The calculation may overflow.");
            AddInstruction(InstructionSyntaxType.ArithLog, "and", Functions.And, "and $d,$s,$t: Bitwise and $s and $t and store the result in $d.");
            AddInstruction(InstructionSyntaxType.ArithLog, "nor", Functions.Nor, "nor $d,$s,$t: Bitwise nor $s and $t and store the result in $d.");
            AddInstruction(InstructionSyntaxType.ArithLog, "or", Functions.Or, "or $d,$s,$t: Bitwise or $s and $t and store the result in $d.");
            AddInstruction(InstructionSyntaxType.ArithLog, "sub", Functions.Sub, "sub $d,$s,$t: Subtract $t from $s and store the result in $d. If the calculation overflows, an error is thrown.");
            AddInstruction(InstructionSyntaxType.ArithLog, "subu", Functions.Subu, "subu $d,$s,$t: Subtract $t from $s and store the result in $d. The calculation may overflow.");
            AddInstruction(InstructionSyntaxType.ArithLog, "xor", Functions.Xor, "xor $d,$s,$t: Bitwise xor $s and $t and store the result in $d.");
            AddInstruction(InstructionSyntaxType.ArithLog, "slt", Functions.Slt, "slt $d,$s,$t: Set $d to 1 if $s is less than $t (signed); otherwise set $d to 0.");
            AddInstruction(InstructionSyntaxType.ArithLog, "sltu", Functions.Sltu, "sltu $d,$s,$t: Set $d to 1 if $s is less than $t (unsigned); otherwise set $d to 0.");

            AddInstruction(InstructionSyntaxType.ArithLogI, "addi", Opcodes.Addi, "addi $t,$s,i: Add $s and i and store the result in $t. If the calculation overflows, an error is thrown.");
            AddInstruction(InstructionSyntaxType.ArithLogI, "addiu", Opcodes.Addiu, "addiu $t,$s,i: Add $s and i and store the result in $t. The calculation may overflow.");
            AddInstruction(InstructionSyntaxType.ArithLogI, "andi", Opcodes.Andi, "andi $t,$s,i: Bitwise and $s and i and store the result in $t.");
            AddInstruction(InstructionSyntaxType.ArithLogI, "ori", Opcodes.Ori, "ori $t,$s,i: Bitwise or $s and i and store the result in $t.");
            AddInstruction(InstructionSyntaxType.ArithLogI, "xori", Opcodes.Xori, "xori $t,$s,i: Bitwise xor $s and i and store the result in $t.");
            AddInstruction(InstructionSyntaxType.ArithLogI, "slti", Opcodes.Slti, "slti $t,$s,i: Set $t to 1 if $s is less than i (signed); otherwise set $t to 0.");
            AddInstruction(InstructionSyntaxType.ArithLogI, "sltiu", Opcodes.Sltiu, "sltiu $t,$s,i: Set $t to 1 if $s is less than i (unsigned); otherwise set $t to 0.");

            AddInstruction(InstructionSyntaxType.DivMult, "div", Functions.Div, "div $s,$t: Calculate $s over $t (signed); store the result in $lo and the remainder in $hi.");
            AddInstruction(InstructionSyntaxType.DivMult, "divu", Functions.Divu, "divu $s,$t: Calculate $s over $t (unsigned); store the result in $lo and the remainder in $hi.");
            AddInstruction(InstructionSyntaxType.DivMult, "mult", Functions.Mult, "mult $s,$t: Multiplty $s and $t (signed); store the upper 32 bits of the result in $hi and the lower 32 bits in $lo.");
            AddInstruction(InstructionSyntaxType.DivMult, "multu", Functions.Multu, "multu $s,$t: Multiplty $s and $t (unsigned); store the upper 32 bits of the result in $hi and the lower 32 bits in $lo.");

            AddInstruction(InstructionSyntaxType.Shift, "sll", Functions.Sll, "sll $d,$t,a: Left-shift $t by a and store the result in $d.");
            AddInstruction(InstructionSyntaxType.Shift, "sra", Functions.Sra, "sra $d,$t,a: Right-shift $t by a and store the result in $d. If $t is negative, 1s are shifted in.");
            AddInstruction(InstructionSyntaxType.Shift, "srl", Functions.Srl, "srl $d,$t,a: Right-shift $t by a and store the result in $d. 0s are shifted in.");

            AddInstruction(InstructionSyntaxType.ShiftV, "sllv", Functions.Sllv, "sllv $d,$t,$s: Left-shift $t by $s and store the result in $d.");
            AddInstruction(InstructionSyntaxType.ShiftV, "srav", Functions.Srav, "srav $d,$t,$s: Right-shift $t by $s and store the result in $d. If $t is negative, 1s are shifted in.");
            AddInstruction(InstructionSyntaxType.ShiftV, "srlv", Functions.Srlv, "srlv $d,$t,$s: Right-shift $t by $s and store the result in $d. 0s are shifted in.");

            AddInstruction(InstructionSyntaxType.RJumpOrMove, "jr", Functions.Jr, "jr $s: Jump to the absolute position stored in $s.");
            AddInstruction(InstructionSyntaxType.RJumpOrMove, "jalr", Functions.Jalr, "jalr $s: Save the current position in $ra, then jump to the absolute position stored in $s.");
            AddInstruction(InstructionSyntaxType.RJumpOrMove, "mfhi", Functions.Mfhi, "mfhi $s: Move the value from $hi into $s.");
            AddInstruction(InstructionSyntaxType.RJumpOrMove, "mflo", Functions.Mflo, "mflo $s: Move the value from $lo into $s.");

            AddInstruction(InstructionSyntaxType.Move, "move", 0, "move $t,$s: Move the value from $s into $t.");

            AddInstruction(InstructionSyntaxType.Branch, "beq", Opcodes.Beq, "beq $s,$t,label: Branch to label if $s and $t are equal.");
            AddInstruction(InstructionSyntaxType.Branch, "bne", Opcodes.Bne, "bne $s,$t,label: Branch to label if $s and $t are not equal.");

            AddInstruction(InstructionSyntaxType.BranchZ, "bgtz", Opcodes.Bgtz, "bgtz $s,label: Branch to label if $s is greater than 0.");
            AddInstruction(InstructionSyntaxType.BranchZ, "blez", Opcodes.Blez, "blez $s,label: Branch to label if $s is less than or equal to 0.");
            AddInstruction(InstructionSyntaxType.BranchZ, "beqz", Opcodes.Beq, "beqz $s,label: Branch to label if $s is equal to 0.");

            AddInstruction(InstructionSyntaxType.BranchAlways, "b", 0, "b label: Branch to label.");

            AddInstruction(InstructionSyntaxType.LI, "li", 0, "li $t,i: Load i (32 bits) into $t. Creates two separate instructions.");
            AddInstruction(InstructionSyntaxType.LA, "la", 0, "la $t,label: Load the value of the label into $t. Creates two separate instructions.");
            AddInstruction(InstructionSyntaxType.LoadI, "lui", Opcodes.Lui, "lui $t,i: Load i into the upper 16 bits of $t.");

            AddInstruction(InstructionSyntaxType.LoadStore, "lb", Opcodes.Lb, "lb $t,i($s): Load a byte from address (i + $s) and store it in $t (signed).");
            AddInstruction(InstructionSyntaxType.LoadStore, "lbu", Opcodes.Lbu, "lbu $t,i($s): Load a byte from address (i + $s) and store it in $t (unsigned).");
            AddInstruction(InstructionSyntaxType.LoadStore, "lh", Opcodes.Lh, "lh $t,i($s): Load a half-word from address (i + $s) and store it in $t (signed).");
            AddInstruction(InstructionSyntaxType.LoadStore, "lhu", Opcodes.Lhu, "lhu $t,i($s): Load a half-word from address (i + $s) and store it in $t (unsigned).");
            AddInstruction(InstructionSyntaxType.LoadStore, "lw", Opcodes.Lw, "lw $t,i($s): Load a word from address (i + $s) and store it in $t.");
            AddInstruction(InstructionSyntaxType.LoadStore, "sb", Opcodes.Sb, "sb $t,i($s): Store the lowest byte from $t at address (i + $s).");
            AddInstruction(InstructionSyntaxType.LoadStore, "sh", Opcodes.Sh, "sh $t,i($s): Store the lowest half-word from $t at address (i + $s).");
            AddInstruction(InstructionSyntaxType.LoadStore, "sw", Opcodes.Sw, "sw $t,i($s): Store the word from $t at address (i + $s).");

            AddInstruction(InstructionSyntaxType.Jump, "j", Opcodes.J, "j label: Jump to label.");
            AddInstruction(InstructionSyntaxType.Jump, "jal", Opcodes.Jal, "jal label: Save the current position in $ra, then jump to label.");
        }

        public enum InstructionSyntaxType
        {
            ArithLog,
            DivMult,
            Shift,
            ShiftV,
            RJumpOrMove,
            Move,
            ArithLogI,
            LoadI,
            Branch,
            BranchZ,
            BranchAlways,
            LoadStore,
            Jump,
            Trap,
            LA,
            LI,
        }

        public class InstructionInfo
        {
            public InstructionInfo(InstructionSyntaxType type, uint functionOrOpcode, string help)
            {
                Type = type;
                FunctionOrOpcode = functionOrOpcode;
                Help = help;
            }

            public InstructionSyntaxType Type { get; }
            public uint FunctionOrOpcode { get; }
            public string Help { get; }
        }

        public static IAssemblerResult Assemble(string code, IMemory target)
        {
            var instance = new AssemblerInstance(code, target);
            instance.Assemble();
            return instance;
        }
    }
}
