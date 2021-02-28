using Mips.Assembler.Properties;
using Mips.Emulator;
using System;
using System.Collections.Generic;

namespace Mips.Assembler
{
    public class Assembler
    {
        public static Dictionary<string, InstructionInfo> Instructions = new Dictionary<string, InstructionInfo>();

        static Assembler()
        {
            InitializeInstructions()
        }

        private static void AddInstruction(string name, uint functionOrOpcode, string help)
        {
            Instructions[name] = new InstructionInfo(functionOrOpcode, help);
        }

        public static void InitializeInstructions()
        {
            AddInstruction("add", 0b100000, "add $d,$s,$t: Add $s and $t and store the result in $d. If the calculation overflows, an error is thrown.");
            AddInstruction("addu", 0b100001, "addu $d,$s,$t: Add $s and $t and store the result in $d. The calculation may overflow.");
            AddInstruction("addi", 0b001000, "addi $t,$s,i: Add $s and i and store the result in $t. If the calculation overflows, an error is thrown.");
            AddInstruction("addiu", 0b001001, "addiu $t,$s,i: Add $s and i and store the result in $t. The calculation may overflow.");
            AddInstruction("sub", 0b100010, "sub $d,$s,$t: Subtract $t from $s and store the result in $d. If the calculation overflows, an error is thrown.");
            AddInstruction("subu", 0b100011, "subu $d,$s,$t: Subtract $t from $s and store the result in $d. The calculation may overflow.");
            AddInstruction("and", 0b100100, "and $d,$s,$t: Bitwise and $s and $t and store the result in $d.");
            AddInstruction("andi", 0b001100, "andi $t,$s,i: Bitwise and $s and i and store the result in $t.");
            AddInstruction("or", 0b100101, "or $d,$s,$t: Bitwise or $s and $t and store the result in $d.");
            AddInstruction("ori", 0b001101, "ori $t,$s,i: Bitwise or $s and i and store the result in $t.");
            AddInstruction("xor", 0b100110, "xor $d,$s,$t: Bitwise xor $s and $t and store the result in $d.");
            AddInstruction("xori", 0b001110, "xori $t,$s,i: Bitwise xor $s and i and store the result in $t.");
            AddInstruction("nor", 0b100111, "nor $d,$s,$t: Bitwise nor $s and $t and store the result in $d.");
            AddInstruction("slt", 0b101010, "slt $d,$s,$t: Set $d to 1 if $s is less than $t (signed); otherwise set $d to 0.");
            AddInstruction("sltu", 0b101011, "sltu $d,$s,$t: Set $d to 1 if $s is less than $t (unsigned); otherwise set $d to 0.");
            AddInstruction("slti", 0b001010, "slti $t,$s,i: Set $t to 1 if $s is less than i (signed); otherwise set $t to 0.");
            AddInstruction("sltiu", 0b001011, "sltiu $t,$s,i: Set $t to 1 if $s is less than i (unsigned); otherwise set $t to 0.");
            AddInstruction("div", 0b011010, "div $s,$t: Calculate $s over $t (signed); store the result in $lo and the remainder in $hi.");
            AddInstruction("divu", 0b011011, "divu $s,$t: Calculate $s over $t (unsigned); store the result in $lo and the remainder in $hi.");
            AddInstruction("mult", 0b011000, "mult $s,$t: Multiplty $s and $t (signed); store the upper 32 bits of the result in $hi and the lower 32 bits in $lo.");
            AddInstruction("multu", 0b011001, "multu $s,$t: Multiplty $s and $t (unsigned); store the upper 32 bits of the result in $hi and the lower 32 bits in $lo.");
            AddInstruction("sll", 0b000000, "sll $d,$t,a: Left-shift $t by a and store the result in $d.");
            AddInstruction("sllv", 0b000100, "sllv $d,$t,$s: Left-shift $t by $s and store the result in $d.");
            AddInstruction("sra", 0b000011, "sra $d,$t,a: Right-shift $t by a and store the result in $d. If $t is negative, 1s are shifted in.");
            AddInstruction("srav", 0b000111, "srav $d,$t,$s: Right-shift $t by $s and store the result in $d. If $t is negative, 1s are shifted in.");
            AddInstruction("srl", 0b000010, "srl $d,$t,a: Right-shift $t by a and store the result in $d. 0s are shifted in.");
            AddInstruction("srlv", 0b000110, "srlv $d,$t,$s: Right-shift $t by $s and store the result in $d. 0s are shifted in.");
            AddInstruction("jr", 0b001000, "jr $s: Jump to the absolute position stored in $s.");
            AddInstruction("jalr", 0b001001, "jalr $s: Save the current position in $ra, then jump to the absolute position stored in $s.");
            AddInstruction("mfhi", 0b010000, "mfhi $s: Move the value from $hi into $s.");
            AddInstruction("mflo", 0b010010, "mflo $s: Move the value from $lo into $s.");
            AddInstruction("beq", 0b000100, "beq $s,$t,label: Branch to label if $s and $t are equal.");
            AddInstruction("bne", 0b000101, "bne $s,$t,label: Branch to label if $s and $t are not equal.");
            AddInstruction("bgtz", 0b000111, "bgtz $s,label: Branch to label if $s is greater than 0.");
            AddInstruction("blez", 0b000110, "blez $s,label: Branch to label if $s is less than or equal to 0.");
            AddInstruction("lb", 0b100000, "lb $t,i($s): Load a byte from address (i + $s) and store it in $t (signed).");
            AddInstruction("lbu", 0b100100, "lbu $t,i($s): Load a byte from address (i + $s) and store it in $t (unsigned).");
            AddInstruction("lh", 0b100001, "lh $t,i($s): Load a half-word from address (i + $s) and store it in $t (signed).");
            AddInstruction("lhu", 0b100101, "lhu $t,i($s): Load a half-word from address (i + $s) and store it in $t (unsigned).");
            AddInstruction("lw", 0b100011, "lw $t,i($s): Load a word from address (i + $s) and store it in $t.");
            AddInstruction("sb", 0b101000, "sb $t,i($s): Store the lowest byte from $t at address (i + $s).");
            AddInstruction("sh", 0b101001, "sh $t,i($s): Store the lowest half-word from $t at address (i + $s).");
            AddInstruction("sw", 0b101011, "sw $t,i($s): Store the word from $t at address (i + $s).");
            AddInstruction("j", 0b000010, "j label: Jump to label.");
            AddInstruction("jal", 0b000011, "jal label: Save the current position in $ra, then jump to label.");
        }
        public class InstructionInfo
        {
            public InstructionInfo(uint functionOrOpcode, string help)
            {
                FunctionOrOpcode = functionOrOpcode;
                Help = help;
            }

            public uint FunctionOrOpcode { get; }
            public string Help { get; }
        }
    }


    public class AssemblerInstance
    {
        private readonly StringEnumerator code;
        private readonly IMemory memory;

        private List<Message> messages = new List<Message>();
        private Dictionary<string, uint> labels = new Dictionary<string, uint>();
        private bool writeEnable = false;
        private uint memoryAddress = 0;

        private static readonly string[] registerNames = new string[]
        {
            null, null,
            "v0", "v1",
            "a0", "a1", "a2", "a3",
            "t0", "t1", "t2", "t3", "t4", "t5", "t6", "t7",
            "s0", "s1", "s2", "s3", "s4", "s5", "s6", "s7",
            "t8", "t9",
            null, null, null,
            "sp", "fp", "ra",
        };

        public AssemblerInstance(string code, IMemory memory)
        {
            this.code = new StringEnumerator(code);
            this.code.MoveNext();
            this.memory = memory;
        }

        public void Assemble()
        {
        }

        public bool TryReadInstruction()
        {
            var startIndex = code.Index;
            if (TryReadName(out string name) && TryReadWhitespace())
            {
                switch (name.ToLower())
                {
                    case "add":
                    case "addu":
                    case "and":
                    case "nor":
                    case "or":
                    case "sub":
                    case "subu":
                    case "xor":
                    case "slt":
                    case "sltu":
                        {
                            var info = Assembler.Instructions[name];
                            if (TryReadArithLog(info.FunctionOrOpcode))
                            {
                                AddInfo(startIndex, code.Index, info.Help);
                                return true;
                            }
                            break;
                        }
                }
            }
            code.Index = startIndex;
            return false;
        }

        public bool TryReadArithLog(uint function)
        {
            var startIndex = code.Index;
            if (TryReadRegister(out int rd)
                && TryReadComma()
                && TryReadRegister(out int rs)
                && TryReadComma()
                && TryReadRegister(out int rt))
            {
                uint ins = OperationEncoder.EncodeFormatR(rs, rt, rd, 0, function);
                WriteWord(ins);
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        public bool TryReadDivMult(uint function)
        {
            var startIndex = code.Index;
            if (TryReadRegister(out int rs)
                && TryReadComma()
                && TryReadRegister(out int rt))
            {
                uint ins = OperationEncoder.EncodeFormatR(rs, rt, 0, 0, function);
                WriteWord(ins);
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        public bool TryReadShift(uint function)
        {
            var startIndex = code.Index;
            if (TryReadRegister(out int rd)
                && TryReadComma()
                && TryReadRegister(out int rt)
                && TryReadComma()
                && TryReadSigned(out int shamt))
            {
                uint ins = OperationEncoder.EncodeFormatR(0, rt, rd, shamt, function);
                WriteWord(ins);
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        public bool TryReadShiftV(uint function)
        {
            var startIndex = code.Index;
            if (TryReadRegister(out int rd)
                && TryReadComma()
                && TryReadRegister(out int rt)
                && TryReadComma()
                && TryReadRegister(out int rs))
            {
                uint ins = OperationEncoder.EncodeFormatR(rs, rt, rd, 0, function);
                WriteWord(ins);
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        public bool TryReadJumpROrMoveTo(uint function)
        {
            var startIndex = code.Index;
            if (TryReadRegister(out int rs))
            {
                uint ins = OperationEncoder.EncodeFormatR(rs, 0, 0, 0, function);
                WriteWord(ins);
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        public bool TryReadArithLogI(uint opcode)
        {
            var startIndex = code.Index;
            if (TryReadRegister(out int rt)
                && TryReadComma()
                && TryReadRegister(out int rs)
                && TryReadComma()
                && TryReadUnsigned(out uint immed))
            {
                uint ins = OperationEncoder.EncodeFormatI(opcode, rs, rt, immed);
                WriteWord(ins);
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        public bool TryReadBranch(uint opcode)
        {
            var startIndex = code.Index;
            if (TryReadRegister(out int rs)
                && TryReadComma()
                && TryReadRegister(out int rt)
                && TryReadComma()
                && TryReadAndLookupLabel(out uint address))
            {
                uint ins = OperationEncoder.EncodeFormatI(opcode, rs, rt, address);
                WriteWord(ins);
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        public bool TryReadBranchZ(uint opcode)
        {
            var startIndex = code.Index;
            if (TryReadRegister(out int rs)
                && TryReadComma()
                && TryReadAndLookupLabel(out uint address))
            {
                uint ins = OperationEncoder.EncodeFormatI(opcode, rs, 0, address);
                WriteWord(ins);
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        public bool TryReadLoadStore(uint opcode)
        {
            var startIndex = code.Index;
            if (TryReadRegister(out int rt)
                && TryReadComma()
                && TryReadValueWithOffset(out uint value, out int rs))
            {
                uint ins = OperationEncoder.EncodeFormatI(opcode, rs, rt, value);
                WriteWord(ins);
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        public bool TryReadJump(bool link)
        {
            var startIndex = code.Index;
            if (TryReadAndLookupLabel(out uint address))
            {
                uint ins = OperationEncoder.EncodeFormatJ(address, link);
                WriteWord(ins);
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        public bool TryReadAsciiString(out byte[] value)
        {
            var bytes = new List<byte>();
            if (code.Current == '"')
            {
                bool isEscape = false;
                while (code.MoveNext() && IsAscii())
                {
                    if (code.Current == '\\')
                    {
                        if (!isEscape)
                        {
                            isEscape = true;
                        }
                        else
                        {
                            bytes.Add((byte)'\\');
                            isEscape = false;
                        }
                    }
                    else if (code.Current == 'n' && isEscape)
                    {
                        bytes.Add((byte)'\n');
                        isEscape = false;
                    }
                    else if (code.Current == 't' && isEscape)
                    {
                        bytes.Add((byte)'\t');
                        isEscape = false;
                    }
                    else if (code.Current == 'r' && isEscape)
                    {
                        bytes.Add((byte)'\r');
                        isEscape = false;
                    }
                    else if (code.Current == '"' && !isEscape)
                    {
                        code.MoveNext();
                        value = bytes.ToArray();
                        return true;
                    }
                    else
                    {
                        bytes.Add((byte)code.Current);
                        isEscape = false;
                    }
                }
            }

            value = null;
            return false;
        }

        public bool TryReadAndLookupLabel(out uint labelAddress)
        {
            var startIndex = code.Index;
            if (TryReadName(out string name))
            {
                if (labels.TryGetValue(name, out labelAddress))
                {
                    return true;
                }
                else
                {
                    AddError(startIndex, code.Index, Resources.LabelNotDefined, name);
                }
            }
            labelAddress = 0u;
            return false;
        }

        public bool TryReadLabelDefinition()
        {
            var startIndex = code.Index;
            if (TryReadName(out string name))
            {
                if (code.Current == ':')
                {
                    code.MoveNext();
                    DefineLabel(startIndex, code.Index, name);
                    return true;
                }
            }
            code.Index = startIndex;
            return false;
        }

        public bool TryReadValueWithOffset(out uint value, out int offsetRegister)
        {
            if (TryReadUnsigned(out value)
                && code.Current == '(' && code.MoveNext()
                && TryReadRegister(out offsetRegister)
                && code.Current == ')')
            {
                code.MoveNext();
                return true;
            }
            offsetRegister = 0;
            return false;
        }

        public bool TryReadUnsigned(out uint value)
        {
            var startIndex = code.Index;

            while (IsDigit() && code.MoveNext()) ;
            string number = code[startIndex..code.Index];
            if (uint.TryParse(number, out value))
            {
                return true;
            }

            code.Index = startIndex;
            value = 0;
            return false;
        }

        public bool TryReadSigned(out int value)
        {
            var startIndex = code.Index;
            if ((IsDigit() || code.Current == '-') && code.MoveNext())
            {
                while (IsDigit() && code.MoveNext()) ;
                string number = code[startIndex..code.Index];
                if (int.TryParse(number, out value))
                {
                    return true;
                }
            }
            code.Index = startIndex;
            value = 0;
            return false;
        }

        public bool TryReadRegister(out int register)
        {
            var startIndex = code.Index;
            if (code.Current == '$' && code.MoveNext())
            {
                if (IsDigit())
                {
                    if (code.MoveNext() && IsDigit())
                    {
                        // case 1: two-digit register reference
                        string number = code[(startIndex + 1)..(startIndex + 3)];
                        if (int.TryParse(number, out register) && 0 <= register && register < 32)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        // case 2: one-digit register reference
                        string number = code[(startIndex + 1)..(startIndex + 2)];
                        if (int.TryParse(number, out register))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (code.MoveNext() && code.MoveNext())
                    {
                        string reference = code[(startIndex + 1)..(startIndex + 3)];
                        int index = Array.IndexOf(registerNames, reference);
                        if (index != -1)
                        {
                            register = index;
                            return true;
                        }
                    }
                }
            }

            code.Index = startIndex;
            register = -1;
            return false;
        }

        public bool TryReadName(out string name)
        {
            var startIndex = code.Index;
            if (IsNameFirstChar() && code.MoveNext())
            {
                while (IsNameSecondChar() && code.MoveNext()) ;
                name = code[startIndex..code.Index];
                return true;
            }
            name = null;
            return false;
        }

        public bool TryReadComma()
        {
            var startIndex = code.Index;
            TryReadWhitespace();
            if (code.Current == ',' && code.MoveNext())
            {
                TryReadWhitespace();
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        public bool TryReadWhitespace()
        {
            var result = false;
            while (char.IsWhiteSpace(code.Current) && code.MoveNext())
            {
                result = true;
            }
            return result;
        }

        public void WriteWord(uint word)
        {
            // get next greater or equal multiple of 4
            memoryAddress += 3;
            memoryAddress %= 4;

            if (writeEnable)
            {
                memory.StoreWord(memoryAddress, word);
            }

            memoryAddress += 4;
        }

        public void WriteByte(byte value)
        {
            if (writeEnable)
            {
                memory[memoryAddress] = value;
            }

            memoryAddress++;
        }

        public void DefineLabel(int startIndex, int endIndex, string name)
        {
            if (labels.ContainsKey(name))
            {
                AddError(startIndex, endIndex, Resources.LabelDefinedTwice, name);
            }
            else
            {
                labels[name] = memoryAddress;
            }
        }

        public void AddError(int startIndex, int endIndex, string message, params object[] args)
        {
            if (args.Length != 0)
            {
                message = string.Format(message, args);
            }

            // If an error is encountered, we stop writing bytes and clear the memory.
            // We still go through the rest of the code to create error and info messages.
            memory.Reset();
            writeEnable = false;
            messages.Add(new Message
            {
                StartIndex = startIndex,
                EndIndex = endIndex,
                IsError = true,
                Content = message,
            });
        }

        public void AddInfo(int startIndex, int endIndex, string message, params object[] args)
        {
            if (args.Length != 0)
            {
                message = string.Format(message, args);
            }

            messages.Add(new Message
            {
                StartIndex = startIndex,
                EndIndex = endIndex,
                IsError = false,
                Content = message,
            });
        }

        public bool IsDigit()
        {
            return '0' <= code.Current && code.Current <= '9';
        }

        public bool IsNameFirstChar()
        {
            return char.IsLetter(code.Current) || code.Current == '_';
        }

        public bool IsNameSecondChar()
        {
            return char.IsLetterOrDigit(code.Current) || code.Current == '_';
        }

        public bool IsAscii()
        {
            return code.Current < 128;
        }
    }
}
