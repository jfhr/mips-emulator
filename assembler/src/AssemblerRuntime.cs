using Mips.Assembler.Mnemonics;
using Mips.Assembler.Services;
using Mips.Emulator;
using System;
using System.Collections.Generic;

namespace Mips.Assembler
{
    public class AssemblerRuntime
    {
        public Assembler Assembler { get; }

        private AssemblerServiceContainer services;

        private readonly List<IMnemonic> statements;

        public AssemblerRuntime(IMemory memory)
        {
            var parameterQueue = new ParameterQueue();
            var messageHelper = new MessageHelper();
            var binaryCodeWriter = new BinaryCodeWriter(memory);
            var labelRegistry = new LabelRegistry(messageHelper, binaryCodeWriter);

            var whitespace = new Whitespace();
            var comma = new Comma(whitespace);

            var register = new Register(parameterQueue);
            var scalar = new Scalar(parameterQueue);

            services = new AssemblerServiceContainer()
            {
                ParameterQueue = parameterQueue,
                MessageHelper = messageHelper,
                BinaryCodeWriter = binaryCodeWriter,
                LabelRegistry = labelRegistry,
                Whitespace = whitespace,
                Comma = comma,
                Register = register,
                Scalar = scalar,
            };

            statements = CreateInstructionObjects();
        }


        private List<IMnemonic> CreateInstructionObjects()
        {
            var instructions = AssemblerData.Instructions;
            var result = new List<IMnemonic>(instructions.Length);
            foreach (var ins in instructions)
            {
                IMnemonic obj = ins.SyntaxType switch
                {
                    InstructionSyntaxType.ArithLog => new ArithLogInstruction(ins, services),
                    InstructionSyntaxType.DivMult => new DivMultInstruction(ins, services),
                    InstructionSyntaxType.RJumpOrMove => new RJumpOrMoveInstruction(ins, services),
                    InstructionSyntaxType.Shift => new ShiftInstruction(ins, services),
                    InstructionSyntaxType.ShiftV => new ShiftVInstruction(ins, services),
                    InstructionSyntaxType.ArithLogI => new ArithLogIInstruction(ins, services),
                    _ => throw new NotImplementedException(),
                };
                result.Add(obj);
            }
            return result;
        }
    }

    public class AssemblerServiceContainer
    {
        public IParameterQueue ParameterQueue { get; set; }
        public ILabelRegistry LabelRegistry { get; set; }
        public IBinaryCodeWriter BinaryCodeWriter { get; set; }
        public IMessageHelper MessageHelper { get; set; }

        public IMnemonic Whitespace { get; set; }
        public IMnemonic Comma { get; set; }
        public IMnemonic Register { get; set; }
        public IMnemonic Scalar { get; set; }
    }

    public static class AssemblerData
    {
        public static readonly InstructionDescriptor[] Instructions = new[]
        {
            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "add", 0b100000, "add $d,$s,$t: Add $s and $t and store the result in $d. If the calculation overflows, an error is thrown."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "addu", 0b100001, "addu $d,$s,$t: Add $s and $t and store the result in $d. The calculation may overflow."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLogI, "addi", 0b001000, "addi $t,$s,i: Add $s and i and store the result in $t. If the calculation overflows, an error is thrown."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLogI, "addiu", 0b001001, "addiu $t,$s,i: Add $s and i and store the result in $t. The calculation may overflow."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "sub", 0b100010, "sub $d,$s,$t: Subtract $t from $s and store the result in $d. If the calculation overflows, an error is thrown."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "subu", 0b100011, "subu $d,$s,$t: Subtract $t from $s and store the result in $d. The calculation may overflow."),

            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "and", 0b100100, "and $d,$s,$t: Bitwise and $s and $t and store the result in $d."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLogI, "andi", 0b001100, "andi $t,$s,i: Bitwise and $s and i and store the result in $t."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "or", 0b100101, "or $d,$s,$t: Bitwise or $s and $t and store the result in $d."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLogI, "ori", 0b001101, "ori $t,$s,i: Bitwise or $s and i and store the result in $t."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "xor", 0b100110, "xor $d,$s,$t: Bitwise xor $s and $t and store the result in $d."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLogI, "xori", 0b001110, "xori $t,$s,i: Bitwise xor $s and i and store the result in $t."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "nor", 0b100111, "nor $d,$s,$t: Bitwise nor $s and $t and store the result in $d."),

            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "slt", 0b101010, "slt $d,$s,$t: Set $d to 1 if $s is less than $t (signed), otherwise set $d to 0."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "sltu", 0b101011, "sltu $d,$s,$t: Set $d to 1 if $s is less than $t (unsigned), otherwise set $d to 0."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLogI, "slti", 0b001010, "slti $t,$s,i: Set $t to 1 if $s is less than i (signed), otherwise set $t to 0."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLogI, "sltiu", 0b001011, "sltiu $t,$s,i: Set $t to 1 if $s is less than i (unsigned), otherwise set $t to 0."),

            new InstructionDescriptor(InstructionSyntaxType.DivMult, "div", 0b011010, "div $s,$t: Calculate $s over $t (signed), store the result in $lo and the remainder in $hi."),
            new InstructionDescriptor(InstructionSyntaxType.DivMult, "divu", 0b011011, "divu $s,$t: Calculate $s over $t (unsigned), store the result in $lo and the remainder in $hi."),
            new InstructionDescriptor(InstructionSyntaxType.DivMult, "mult", 0b011000, "mult $s,$t: Multiplty $s and $t (signed), store the upper 32 bits of the result in $hi and the lower 32 bits in $lo."),
            new InstructionDescriptor(InstructionSyntaxType.DivMult, "multu", 0b011001, "multu $s,$t: Multiplty $s and $t (unsigned), store the upper 32 bits of the result in $hi and the lower 32 bits in $lo."),

            new InstructionDescriptor(InstructionSyntaxType.Shift, "sll", 0b000000, "sll $d,$t,a: Left-shift $t by a and store the result in $d."),
            new InstructionDescriptor(InstructionSyntaxType.ShiftV, "sllv", 0b000100, "sllv $d,$t,$s: Left-shift $t by $s and store the result in $d."),
            new InstructionDescriptor(InstructionSyntaxType.Shift, "sra", 0b000011, "sra $d,$t,a: Right-shift $t by a and store the result in $d. If $t is negative, 1s are shifted in."),
            new InstructionDescriptor(InstructionSyntaxType.ShiftV, "srav", 0b000111, "srav $d,$t,$s: Right-shift $t by $s and store the result in $d. If $t is negative, 1s are shifted in."),
            new InstructionDescriptor(InstructionSyntaxType.Shift, "srl", 0b000010, "srl $d,$t,a: Right-shift $t by a and store the result in $d. 0s are shifted in."),
            new InstructionDescriptor(InstructionSyntaxType.ShiftV, "srlv", 0b000110, "srlv $d,$t,$s: Right-shift $t by $s and store the result in $d. 0s are shifted in."),

            new InstructionDescriptor(InstructionSyntaxType.RJumpOrMove, "jr", 0b001000, "jr $s: Jump to the absolute position stored in $s."),
            new InstructionDescriptor(InstructionSyntaxType.RJumpOrMove, "jalr", 0b001001, "jalr $s: Save the current position in $ra, then jump to the absolute position stored in $s."),
            new InstructionDescriptor(InstructionSyntaxType.RJumpOrMove, "mfhi", 0b010000, "mfhi $s: Move the value from $hi into $s."),
            new InstructionDescriptor(InstructionSyntaxType.RJumpOrMove, "mflo", 0b010010, "mflo $s: Move the value from $lo into $s."),

            new InstructionDescriptor(InstructionSyntaxType.Branch, "beq", 0b000100, "beq $s,$t,label: Branch to label if $s and $t are equal."),
            new InstructionDescriptor(InstructionSyntaxType.Branch, "bne", 0b000101, "bne $s,$t,label: Branch to label if $s and $t are not equal."),
            new InstructionDescriptor(InstructionSyntaxType.BranchZ, "bgtz", 0b000111, "bgtz $s,label: Branch to label if $s is greater than 0."),
            new InstructionDescriptor(InstructionSyntaxType.BranchZ, "blez", 0b000110, "blez $s,label: Branch to label if $s is less than or equal to 0."),

            new InstructionDescriptor(InstructionSyntaxType.LoadStore, "lb", 0b100000, "lb $t,i($s): Load a byte from address (i + $s) and store it in $t (signed)."),
            new InstructionDescriptor(InstructionSyntaxType.LoadStore, "lbu", 0b100100, "lbu $t,i($s): Load a byte from address (i + $s) and store it in $t (unsigned)."),
            new InstructionDescriptor(InstructionSyntaxType.LoadStore, "lh", 0b100001, "lh $t,i($s): Load a half-word from address (i + $s) and store it in $t (signed)."),
            new InstructionDescriptor(InstructionSyntaxType.LoadStore, "lhu", 0b100101, "lhu $t,i($s): Load a half-word from address (i + $s) and store it in $t (unsigned)."),
            new InstructionDescriptor(InstructionSyntaxType.LoadStore, "lw", 0b100011, "lw $t,i($s): Load a word from address (i + $s) and store it in $t."),
            new InstructionDescriptor(InstructionSyntaxType.LoadStore, "sb", 0b101000, "sb $t,i($s): Store the lowest byte from $t at address (i + $s)."),
            new InstructionDescriptor(InstructionSyntaxType.LoadStore, "sh", 0b101001, "sh $t,i($s): Store the lowest half-word from $t at address (i + $s)."),
            new InstructionDescriptor(InstructionSyntaxType.LoadStore, "sw", 0b101011, "sw $t,i($s): Store the word from $t at address (i + $s)."),

            new InstructionDescriptor(InstructionSyntaxType.Jump, "j", 0b000010, "j label: Jump to label."),
            new InstructionDescriptor(InstructionSyntaxType.Jump, "jal", 0b000011, "jal label: Save the current position in $ra, then jump to label."),
        };
    }

    public class InstructionDescriptor
    {
        public InstructionSyntaxType SyntaxType { get; }
        public string Name { get; }
        public uint FunctionOrOpcode { get; }
        public string Description { get; }

        public InstructionDescriptor(InstructionSyntaxType syntaxType, string name, uint functionOrOpcode, string syntaxHelp)
        {
            SyntaxType = syntaxType;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            FunctionOrOpcode = functionOrOpcode;
            Description = syntaxHelp ?? throw new ArgumentNullException(nameof(syntaxHelp));
        }
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
        LoadStore,
        Jump,
        Trap,
        Special,
    }
}
