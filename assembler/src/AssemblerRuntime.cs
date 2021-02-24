using Mips.Assembler.Mnemonics;
using Mips.Assembler.Services;
using Mips.Emulator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mips.Assembler
{
    public class AssemblerRuntime
    {
        public Assembler Assembler { get; }

        private readonly IParameterQueue parameterQueue;
        private readonly ILabelRegistry labelRegistry;
        private readonly IBinaryCodeWriter binaryCodeWriter;
        private readonly IErrorMessageHelper errorMessageHelper;

        private readonly Whitespace whitespace;
        private readonly Comma comma;
        private readonly Register register;
        private readonly Scalar scalar;

        private readonly List<IMnemonic> statements;

        public AssemblerRuntime(IMemory memory)
        {
            parameterQueue = new ParameterQueue();
            errorMessageHelper = new ErrorMessageHelper();
            binaryCodeWriter = new BinaryCodeWriter(memory);
            labelRegistry = new LabelRegistry(errorMessageHelper, binaryCodeWriter);
            
            whitespace = new Whitespace();
            comma = new Comma(whitespace);

            register = new Register(parameterQueue);
            scalar = new Scalar(parameterQueue);

            statements = CreateInstructionObjects();
        }


        private List<IMnemonic> CreateInstructionObjects()
        {
            var result = new List<IMnemonic>(Instructions.Length);
            foreach (var ins in Instructions)
            {
                IMnemonic obj = ins.SyntaxType switch
                {
                    InstructionSyntaxType.ArithLog => new ArithLogInstruction(ins.Name, ins.FunctionOrOpcode, parameterQueue, labelRegistry, binaryCodeWriter, whitespace, comma, register),
                    InstructionSyntaxType.DivMult => new DivMultInstruction(ins.Name, ins.FunctionOrOpcode, parameterQueue, labelRegistry, binaryCodeWriter, whitespace, comma, register),
                    InstructionSyntaxType.RJumpOrMove => new RJumpOrMoveInstruction(ins.Name, ins.FunctionOrOpcode, parameterQueue, labelRegistry, binaryCodeWriter, whitespace, comma, register),
                    InstructionSyntaxType.Shift => new ShiftInstruction(ins.Name, ins.FunctionOrOpcode, parameterQueue, labelRegistry, binaryCodeWriter, whitespace, comma, register, scalar),
                    InstructionSyntaxType.ShiftV => new ShiftVInstruction(ins.Name, ins.FunctionOrOpcode, parameterQueue, labelRegistry, binaryCodeWriter, whitespace, comma, register),
                    _ => throw new NotImplementedException(),
                };
                result.Add(obj);
            }
            return result;
        }

        private static readonly InstructionDescriptor[] Instructions = new[]
        {
            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "add", 0b100000, "add $d,$s,$t: Add $s and $t and store the result in $d. If the calculation overflows, an error is thrown."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "addu", 0b100001, "addu $d,$s,$t: Add $s and $t and store the result in $d. The calculation may overflow."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLogI, "addi", 0b001000, "addi $t,$s,i: Add $s and i and store the result in $t. If the calculation overflows, an error is thrown."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLogI, "addui", 0b001000, "addi $t,$s,i: Add $s and i and store the result in $t. The calculation may overflow."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "sub", 0b100000, "sub $d,$s,$t: Subtract $t from $s and store the result in $d. If the calculation overflows, an error is thrown."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "subu", 0b100001, "subu $d,$s,$t: Subtract $t from $s and store the result in $d. The calculation may overflow."),

            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "and", 0b100100, "and $d,$s,$t: Bitwise and $s and $t and store the result in $d."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLogI, "andi", 0b001100, "andi $t,$s,i: Bitwise and $s and i and store the result in $t."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "or", 0b100101, "or $d,$s,$t: Bitwise or $s and $t and store the result in $d."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLogI, "ori", 0b001101, "ori $t,$s,i: Bitwise or $s and i and store the result in $t."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "xor", 0b100110, "xor $d,$s,$t: Bitwise xor $s and $t and store the result in $d."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLogI, "ori", 0b001110, "xori $t,$s,i: Bitwise xor $s and i and store the result in $t."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "nor", 0b100111, "nor $d,$s,$t: Bitwise nor $s and $t and store the result in $d."),

            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "slt", 0b101010, "slt $d,$s,$t: Set $d to 1 if $s is less than $t (signed), otherwise set $d to 0."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLog, "sltu", 0b101001, "slt $d,$s,$t: Set $d to 1 if $s is less than $t (unsigned), otherwise set $d to 0."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLogI, "slti", 0b001010, "slti $t,$s,i: Set $t to 1 if $s is less than i (signed), otherwise set $t to 0."),
            new InstructionDescriptor(InstructionSyntaxType.ArithLogI, "sltiu", 0b001001, "sltiu $t,$s,i: Set $t to 1 if $s is less than i (unsigned), otherwise set $t to 0."),

            new InstructionDescriptor(InstructionSyntaxType.DivMult, "div", 0b011010, "div $s,$t: Calculate $s over $t (signed), store the result in $lo and the remainder in $hi."),
            new InstructionDescriptor(InstructionSyntaxType.DivMult, "divu", 0b011011, "divu $s,$t: Calculate $s over $t (unsigned), store the result in $lo and the remainder in $hi."),
            new InstructionDescriptor(InstructionSyntaxType.DivMult, "mult", 0b011000, "mult $s,$t: Multiplty $s and $t (signed), store the upper 32 bits of the result in $hi and the lower 32 bits in $lo."),
            new InstructionDescriptor(InstructionSyntaxType.DivMult, "multu", 0b011001, "mult $s,$t: Multiplty $s and $t (unsigned), store the upper 32 bits of the result in $hi and the lower 32 bits in $lo."),

            // TODO add all the other beautiful little instructions
        };


        private class InstructionDescriptor
        {
            public InstructionSyntaxType SyntaxType { get; }
            public string Name { get; }
            public uint FunctionOrOpcode { get; }
            public string SyntaxHelp { get; }

            public InstructionDescriptor(InstructionSyntaxType syntaxType, string name, uint functionOrOpcode, string syntaxHelp)
            {
                SyntaxType = syntaxType;
                Name = name ?? throw new ArgumentNullException(nameof(name));
                FunctionOrOpcode = functionOrOpcode;
                SyntaxHelp = syntaxHelp ?? throw new ArgumentNullException(nameof(syntaxHelp));
            }
        }

        private enum InstructionSyntaxType
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
}
