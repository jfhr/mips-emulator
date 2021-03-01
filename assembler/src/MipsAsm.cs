using Mips.Emulator;
using System.Collections.Generic;
using System.Linq;

namespace Mips.Assembler
{
    public static class MipsAsm
    {
        public static Dictionary<string, InstructionInfo> Instructions = new Dictionary<string, InstructionInfo>();

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
            AddInstruction(InstructionSyntaxType.ArithLog, "add", 0b100000, "add $d,$s,$t: Add $s and $t and store the result in $d. If the calculation overflows, an error is thrown.");
            AddInstruction(InstructionSyntaxType.ArithLog, "addu", 0b100001, "addu $d,$s,$t: Add $s and $t and store the result in $d. The calculation may overflow.");
            AddInstruction(InstructionSyntaxType.ArithLogI, "addi", 0b001000, "addi $t,$s,i: Add $s and i and store the result in $t. If the calculation overflows, an error is thrown.");
            AddInstruction(InstructionSyntaxType.ArithLogI, "addiu", 0b001001, "addiu $t,$s,i: Add $s and i and store the result in $t. The calculation may overflow.");
            AddInstruction(InstructionSyntaxType.ArithLog, "sub", 0b100010, "sub $d,$s,$t: Subtract $t from $s and store the result in $d. If the calculation overflows, an error is thrown.");
            AddInstruction(InstructionSyntaxType.ArithLog, "subu", 0b100011, "subu $d,$s,$t: Subtract $t from $s and store the result in $d. The calculation may overflow.");

            AddInstruction(InstructionSyntaxType.ArithLog, "and", 0b100100, "and $d,$s,$t: Bitwise and $s and $t and store the result in $d.");
            AddInstruction(InstructionSyntaxType.ArithLogI, "andi", 0b001100, "andi $t,$s,i: Bitwise and $s and i and store the result in $t.");
            AddInstruction(InstructionSyntaxType.ArithLog, "or", 0b100101, "or $d,$s,$t: Bitwise or $s and $t and store the result in $d.");
            AddInstruction(InstructionSyntaxType.ArithLogI, "ori", 0b001101, "ori $t,$s,i: Bitwise or $s and i and store the result in $t.");
            AddInstruction(InstructionSyntaxType.ArithLog, "xor", 0b100110, "xor $d,$s,$t: Bitwise xor $s and $t and store the result in $d.");
            AddInstruction(InstructionSyntaxType.ArithLogI, "xori", 0b001110, "xori $t,$s,i: Bitwise xor $s and i and store the result in $t.");
            AddInstruction(InstructionSyntaxType.ArithLog, "nor", 0b100111, "nor $d,$s,$t: Bitwise nor $s and $t and store the result in $d.");

            AddInstruction(InstructionSyntaxType.ArithLog, "slt", 0b101010, "slt $d,$s,$t: Set $d to 1 if $s is less than $t (signed); otherwise set $d to 0.");
            AddInstruction(InstructionSyntaxType.ArithLog, "sltu", 0b101011, "sltu $d,$s,$t: Set $d to 1 if $s is less than $t (unsigned); otherwise set $d to 0.");
            AddInstruction(InstructionSyntaxType.ArithLogI, "slti", 0b001010, "slti $t,$s,i: Set $t to 1 if $s is less than i (signed); otherwise set $t to 0.");
            AddInstruction(InstructionSyntaxType.ArithLogI, "sltiu", 0b001011, "sltiu $t,$s,i: Set $t to 1 if $s is less than i (unsigned); otherwise set $t to 0.");

            AddInstruction(InstructionSyntaxType.DivMult, "div", 0b011010, "div $s,$t: Calculate $s over $t (signed); store the result in $lo and the remainder in $hi.");
            AddInstruction(InstructionSyntaxType.DivMult, "divu", 0b011011, "divu $s,$t: Calculate $s over $t (unsigned); store the result in $lo and the remainder in $hi.");
            AddInstruction(InstructionSyntaxType.DivMult, "mult", 0b011000, "mult $s,$t: Multiplty $s and $t (signed); store the upper 32 bits of the result in $hi and the lower 32 bits in $lo.");
            AddInstruction(InstructionSyntaxType.DivMult, "multu", 0b011001, "multu $s,$t: Multiplty $s and $t (unsigned); store the upper 32 bits of the result in $hi and the lower 32 bits in $lo.");

            AddInstruction(InstructionSyntaxType.Shift, "sll", 0b000000, "sll $d,$t,a: Left-shift $t by a and store the result in $d.");
            AddInstruction(InstructionSyntaxType.ShiftV, "sllv", 0b000100, "sllv $d,$t,$s: Left-shift $t by $s and store the result in $d.");
            AddInstruction(InstructionSyntaxType.Shift, "sra", 0b000011, "sra $d,$t,a: Right-shift $t by a and store the result in $d. If $t is negative, 1s are shifted in.");
            AddInstruction(InstructionSyntaxType.ShiftV, "srav", 0b000111, "srav $d,$t,$s: Right-shift $t by $s and store the result in $d. If $t is negative, 1s are shifted in.");
            AddInstruction(InstructionSyntaxType.Shift, "srl", 0b000010, "srl $d,$t,a: Right-shift $t by a and store the result in $d. 0s are shifted in.");
            AddInstruction(InstructionSyntaxType.ShiftV, "srlv", 0b000110, "srlv $d,$t,$s: Right-shift $t by $s and store the result in $d. 0s are shifted in.");

            AddInstruction(InstructionSyntaxType.RJumpOrMove, "jr", 0b001000, "jr $s: Jump to the absolute position stored in $s.");
            AddInstruction(InstructionSyntaxType.RJumpOrMove, "jalr", 0b001001, "jalr $s: Save the current position in $ra, then jump to the absolute position stored in $s.");
            AddInstruction(InstructionSyntaxType.RJumpOrMove, "mfhi", 0b010000, "mfhi $s: Move the value from $hi into $s.");
            AddInstruction(InstructionSyntaxType.RJumpOrMove, "mflo", 0b010010, "mflo $s: Move the value from $lo into $s.");

            AddInstruction(InstructionSyntaxType.Branch, "beq", 0b000100, "beq $s,$t,label: Branch to label if $s and $t are equal.");
            AddInstruction(InstructionSyntaxType.Branch, "bne", 0b000101, "bne $s,$t,label: Branch to label if $s and $t are not equal.");
            AddInstruction(InstructionSyntaxType.BranchZ, "bgtz", 0b000111, "bgtz $s,label: Branch to label if $s is greater than 0.");
            AddInstruction(InstructionSyntaxType.BranchZ, "blez", 0b000110, "blez $s,label: Branch to label if $s is less than or equal to 0.");
            AddInstruction(InstructionSyntaxType.BranchZ, "beqz", 0b000100, "beqz $s,label: Branch to label if $s is equal to 0.");
            AddInstruction(InstructionSyntaxType.BranchAlways, "b", 0b000100, "b label: Branch to label.");

            AddInstruction(InstructionSyntaxType.LoadI, "li", 0b001000, "li $t,i: Load i into $t.");

            AddInstruction(InstructionSyntaxType.LoadStore, "lb", 0b100000, "lb $t,i($s): Load a byte from address (i + $s) and store it in $t (signed).");
            AddInstruction(InstructionSyntaxType.LoadStore, "lbu", 0b100100, "lbu $t,i($s): Load a byte from address (i + $s) and store it in $t (unsigned).");
            AddInstruction(InstructionSyntaxType.LoadStore, "lh", 0b100001, "lh $t,i($s): Load a half-word from address (i + $s) and store it in $t (signed).");
            AddInstruction(InstructionSyntaxType.LoadStore, "lhu", 0b100101, "lhu $t,i($s): Load a half-word from address (i + $s) and store it in $t (unsigned).");
            AddInstruction(InstructionSyntaxType.LoadStore, "lw", 0b100011, "lw $t,i($s): Load a word from address (i + $s) and store it in $t.");
            AddInstruction(InstructionSyntaxType.LoadStore, "sb", 0b101000, "sb $t,i($s): Store the lowest byte from $t at address (i + $s).");
            AddInstruction(InstructionSyntaxType.LoadStore, "sh", 0b101001, "sh $t,i($s): Store the lowest half-word from $t at address (i + $s).");
            AddInstruction(InstructionSyntaxType.LoadStore, "sw", 0b101011, "sw $t,i($s): Store the word from $t at address (i + $s).");

            AddInstruction(InstructionSyntaxType.Jump, "j", 0b000010, "j label: Jump to label.");
            AddInstruction(InstructionSyntaxType.Jump, "jal", 0b000011, "jal label: Save the current position in $ra, then jump to label.");
        }
        public enum InstructionSyntaxType
        {
            ArithLog,
            DivMult,
            Shift,
            ShiftV,
            RJumpOrMove,
            ArithLogI,
            LoadI,
            Branch,
            BranchZ,
            BranchAlways,
            LoadStore,
            Jump,
            Trap,
            Special,
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
