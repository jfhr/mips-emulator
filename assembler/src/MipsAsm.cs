using Mips.Assembler.Properties;
using Mips.Emulator;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        private static void InitializeInstructions()
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

        [Flags]
        public enum InstructionSyntaxType : uint
        {
            ArithLog = 1 << 0,
            DivMult = 1 << 1,
            Shift = 1 << 2,
            ShiftV = 1 << 3,
            RJumpOrMove = 1 << 4,
            FormatR = ArithLog | DivMult | Shift | ShiftV | RJumpOrMove,

            ArithLogI = 1 << 5,
            LoadI = 1 << 6,
            Branch = 1 << 7,
            BranchZ = 1 << 8,
            LoadStore = 1 << 9,
            FormatI = ArithLogI | LoadI | Branch | BranchZ | LoadStore,

            Jump = 1 << 10,
            Trap = 1 << 11,
            FormatJ = Jump | Trap,

            Move = 1 << 12,
            BranchAlways = 1 << 13,
            LA = 1 << 14,
            LI = 1 << 15,
        }

        public record InstructionInfo(InstructionSyntaxType Type, uint FunctionOrOpcode, string Help);

        public static IAssemblerResult Assemble(string code, IMemory target)
        {
            var instance = new AssemblerInstance(code, target);
            instance.Assemble();
            return instance;
        }

        public static IDisassemblerResult Disassemble(byte[] binary)
        {
            var instance = new DisassemblerInstance(binary);
            instance.Disassemble();
            return instance;
        }
    }

    public class DisassemblerInstance : IDisassemblerResult
    {
        private readonly StringBuilder code = new();
        private readonly Stream stream;
        private readonly byte[] wordBuffer = new byte[4];
        private readonly List<Message> errors = new();
        private readonly Dictionary<string, uint> labels = new();
        private int labelCount;

        public DisassemblerInstance(byte[] binary)
        {
            stream = new MemoryStream(binary);
        }

        public string Code => code.ToString();

        public IEnumerable<Message> Errors => errors;

        public void Disassemble()
        {
            TryReadInstruction();
        }

        public bool TryReadInstruction()
        {
            if (TryReadWord(out uint word))
            {
                if ((word & 0b1111_1100_0000_0000_0000_0000_0000_0000) == 0)
                {
                    return TryProcessFormatR(word);
                }
                else
                {
                    return TryProcessFormatJOrI(word);
                }
            }
            return false;
        }

        public bool TryProcessFormatR(uint word)
        {
            var function = word & 0b0000_0000_0000_0000_0000_0000_0011_1111;
            var ins = MipsAsm.Instructions
                .Where(x => (x.Value.Type & MipsAsm.InstructionSyntaxType.FormatR) != 0)
                .FirstOrDefault(x => x.Value.FunctionOrOpcode == function);

            if (ins.Value == null)
            {
                return false;
            }

            return ins.Value.Type switch
            {
                MipsAsm.InstructionSyntaxType.ArithLog => TryProcessArithLog(word, ins.Key),
                MipsAsm.InstructionSyntaxType.DivMult => TryProcessDivMult(word, ins.Key),
                MipsAsm.InstructionSyntaxType.Shift => TryProcessShift(word, ins.Key),
                MipsAsm.InstructionSyntaxType.ShiftV => TryProcessShiftV(word, ins.Key),
                MipsAsm.InstructionSyntaxType.RJumpOrMove => TryProcessRJumpOrMove(word, ins.Key),
                _ => false,
            };
        }

        public bool TryProcessArithLog(uint word, string name)
        {
            OperationDecoder.DecodeFormatR(word, out int rs, out int rt, out int rd, out int shamt, out uint _);
            if (shamt != 0)
            {
                return false;
            }
            code.AppendLine($"{name} ${Constants.RegisterNames[rd]},${Constants.RegisterNames[rs]},${Constants.RegisterNames[rt]}");
            return true;
        }

        public bool TryProcessDivMult(uint word, string name)
        {
            OperationDecoder.DecodeFormatR(word, out int rs, out int rt, out int rd, out int shamt, out uint _);
            if (shamt != 0 || rd != 0)
            {
                return false;
            }
            code.AppendLine($"{name} ${Constants.RegisterNames[rs]},${Constants.RegisterNames[rt]}");
            return true;
        }

        public bool TryProcessShift(uint word, string name)
        {
            OperationDecoder.DecodeFormatR(word, out int rs, out int rt, out int rd, out int shamt, out uint _);
            if (rs != 0)
            {
                return false;
            }
            code.AppendLine($"{name} ${Constants.RegisterNames[rd]},${Constants.RegisterNames[rt]},{shamt}");
            return true;
        }

        public bool TryProcessShiftV(uint word, string name)
        {
            OperationDecoder.DecodeFormatR(word, out int rs, out int rt, out int rd, out int shamt, out uint _);
            if (shamt != 0)
            {
                return false;
            }
            code.AppendLine($"{name} ${Constants.RegisterNames[rd]},${Constants.RegisterNames[rt]},${Constants.RegisterNames[rs]}");
            return true;
        }

        public bool TryProcessRJumpOrMove(uint word, string name)
        {
            OperationDecoder.DecodeFormatR(word, out int rs, out int rt, out int rd, out int shamt, out uint _);
            if (rd != 0 || rt != 0 || shamt != 0)
            {
                return false;
            }
            code.AppendLine($"{name} ${Constants.RegisterNames[rs]}");
            return true;
        }

        public bool TryProcessFormatJOrI(uint word)
        {
            var opcode = (word & 0b1111_1100_0000_0000_0000_0000_0000_0000) >> 26;
            var ins = MipsAsm.Instructions
                .Where(x => (x.Value.Type & MipsAsm.InstructionSyntaxType.FormatR) == 0)
                .FirstOrDefault(x => x.Value.FunctionOrOpcode == opcode);

            if (ins.Value == null)
            {
                return false;
            }

            return ins.Value.Type switch
            {
                MipsAsm.InstructionSyntaxType.ArithLogI => TryProcessArithLogI(word, ins.Key),
                MipsAsm.InstructionSyntaxType.Branch => TryProcessBranch(word, ins.Key),
                MipsAsm.InstructionSyntaxType.BranchZ => TryProcessBranchZ(word, ins.Key),
                MipsAsm.InstructionSyntaxType.LoadStore => TryProcessLoadStore(word, ins.Key),
                MipsAsm.InstructionSyntaxType.Jump => TryProcessJump(word, ins.Key),
                _ => false,
            };
        }

        public bool TryProcessArithLogI(uint word, string name)
        {
            OperationDecoder.DecodeFormatI(word, out uint _, out int rs, out int rt, out uint value);
            code.AppendLine($"{name} ${Constants.RegisterNames[rt]},${Constants.RegisterNames[rs]},{value}");
            return true;
        }

        public bool TryProcessBranch(uint word, string name)
        {
            OperationDecoder.DecodeFormatI(word, out uint _, out int rs, out int rt, out uint value);
            var labelName = AddLabelAndGetName(value);
            code.AppendLine($"{name} ${Constants.RegisterNames[rs]},${Constants.RegisterNames[rt]},{labelName}");
            return true;
        }

        public bool TryProcessBranchZ(uint word, string name)
        {
            OperationDecoder.DecodeFormatI(word, out uint _, out int rs, out int rt, out uint value);
            if (rt != 0)
            {
                return false;
            }
            var labelName = AddLabelAndGetName(value);
            code.AppendLine($"{name} ${Constants.RegisterNames[rs]},{labelName}");
            return true;
        }

        public bool TryProcessLoadStore(uint word, string name)
        {
            OperationDecoder.DecodeFormatI(word, out uint _, out int rs, out int rt, out uint value);
            code.AppendLine($"{name} ${Constants.RegisterNames[rt]},{value}(${Constants.RegisterNames[rs]})");
            return true;
        }

        public bool TryProcessJump(uint word, string name)
        {
            OperationDecoder.DecodeFormatJ(word, out uint address, out bool _);
            var labelName = AddLabelAndGetName(address);
            code.AppendLine($"{name} {labelName}");
            return true;
        }

        public bool TryReadWord(out uint word)
        {
            var bytesRead = stream.Read(wordBuffer);
            if (bytesRead < 4)
            {
                word = 0;
                return false;
            }
            word = BinaryPrimitives.ReadUInt32BigEndian(wordBuffer);
            return true;
        }

        private string AddLabelAndGetName(uint address)
        {
            var labelName = $"l{labelCount++}";
            labels[labelName] = address;
            return labelName;
        }
    }

    public interface IDisassemblerResult
    {
        string Code { get; }

        IEnumerable<Message> Errors { get; }
    }
}
