using Mips.Assembler.Properties;
using Mips.Emulator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mips.Assembler
{
    public class AssemblerInstance : IAssemblerResult
    {
        private readonly StringEnumerator code;
        private readonly IMemory memory;

        private readonly List<Message> messages = new List<Message>();
        private readonly Dictionary<string, uint> labels = new Dictionary<string, uint>();
        private bool writeEnable = false;
        private bool secondPass = false;
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

        public IEnumerable<Message> Messages => messages;
        public IReadOnlyDictionary<string, uint> Labels => labels;

        public AssemblerInstance(string code, IMemory memory)
        {
            this.code = new StringEnumerator(code);
            this.code.MoveNext();
            this.memory = memory;
        }

        public void Assemble()
        {
            Pass();
            if (AnyErrors())
            {
                return;
            }

            writeEnable = true;
            secondPass = true;
            memoryAddress = 0;
            Pass();
            WriteWord(Cpu.TerminateInstruction);
            if (AnyErrors())
            {
                memory.Reset();
            }
        }

        public void Pass()
        {
            SkipWhitespaceAndComments();
            while (TryReadSpecial() || TryReadInstruction() || TryReadLabelDefinition())
            {
                SkipWhitespaceAndComments();
            }

            // If there's still more to read, but nothing matches, it must be a syntax error
            if (code.MoveNext())
            {
                AddError(code.Index, code.Length - 1, Resources.SyntaxError);
            }
        }

        public bool TryReadInstruction()
        {
            var startIndex = code.Index;
            if (TryReadName(out string name))
            {
                TryReadWhitespace();
                name = name.ToLower();
                if (MipsAsm.Instructions.TryGetValue(name, out var info))
                {
                    bool success = info.Type switch
                    {
                        MipsAsm.InstructionSyntaxType.ArithLog => TryReadArithLog(info.FunctionOrOpcode),
                        MipsAsm.InstructionSyntaxType.ArithLogI => TryReadArithLogI(info.FunctionOrOpcode),
                        MipsAsm.InstructionSyntaxType.Branch => TryReadBranch(info.FunctionOrOpcode),
                        MipsAsm.InstructionSyntaxType.BranchZ => TryReadBranchZ(info.FunctionOrOpcode),
                        MipsAsm.InstructionSyntaxType.BranchAlways => TryReadBranchAlways(info.FunctionOrOpcode),
                        MipsAsm.InstructionSyntaxType.DivMult => TryReadDivMult(info.FunctionOrOpcode),
                        MipsAsm.InstructionSyntaxType.Jump => TryReadJump(info.FunctionOrOpcode),
                        MipsAsm.InstructionSyntaxType.LoadI => TryReadLoadI(info.FunctionOrOpcode),
                        MipsAsm.InstructionSyntaxType.LoadStore => TryReadLoadStore(info.FunctionOrOpcode),
                        MipsAsm.InstructionSyntaxType.RJumpOrMove => TryReadJumpROrMoveTo(info.FunctionOrOpcode),
                        MipsAsm.InstructionSyntaxType.Shift => TryReadShift(info.FunctionOrOpcode),
                        MipsAsm.InstructionSyntaxType.ShiftV => TryReadShiftV(info.FunctionOrOpcode),
                        _ => throw new NotImplementedException(),
                    };
                    if (success)
                    {
                        AddInfo(startIndex, code.Index, info.Help);
                        return true;
                    }
                    else
                    {
                        AddError(startIndex, code.Index, Resources.SyntaxError);
                    }
                }
            }

            code.Index = startIndex;
            return false;
        }

        public bool TryReadSpecial()
        {
            var startIndex = code.Index;
            if (TryReadSpecialName(out string name))
            {
                TryReadWhitespace();
                name = name.ToLower();
                bool success = name switch
                {
                    ".ascii" => TryReadStringAndWrite(),
                    ".asciiz" => TryReadStringAndWriteWithZero(),
                    // TODO maybe enforce .data and .text ?
                    ".data" => true,
                    ".text" => true,
                    ".globl" => TryReadAndLookupLabel(out uint _),
                    _ => false,
                };
                if (success)
                {
                    return true;
                }
                else
                {
                    AddError(startIndex, code.Index, Resources.SyntaxError);
                }
            }
            code.Index = startIndex;
            return false;
        }

        private bool TryReadStringAndWrite()
        {
            if (TryReadAsciiString(out byte[] value))
            {
                WriteBytes(value);
                return true;
            }
            return false;
        }

        private bool TryReadStringAndWriteWithZero()
        {
            if (TryReadAsciiString(out byte[] value))
            {
                WriteBytes(value);
                WriteByte(0);
                return true;
            }
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

        public bool TryReadLoadI(uint opcode)
        {
            var startIndex = code.Index;
            if (TryReadRegister(out int rt)
                && TryReadComma()
                && TryReadUnsigned(out uint immed))
            {
                uint ins = OperationEncoder.EncodeFormatI(opcode, 0, rt, immed);
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

        public bool TryReadBranchAlways(uint opcode)
        {
            var startIndex = code.Index;
            if (TryReadAndLookupLabel(out uint address))
            {
                uint ins = OperationEncoder.EncodeFormatI(opcode, 0, 0, address);
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

        public bool TryReadJump(uint opcode)
        {
            var startIndex = code.Index;
            bool link = opcode == 0b000011;
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
                else if (!secondPass)
                {
                    labelAddress = 0u;
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

        public bool TryReadSpecialName(out string name)
        {
            var startIndex = code.Index;
            if (code.Current == '.' && code.MoveNext() && TryReadName(out string innerName))
            {
                name = "." + innerName;
                return true;
            }
            name = null;
            code.Index = startIndex;
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

        public void SkipWhitespaceAndComments()
        {
            while (TryReadWhitespace() || TryReadComment()) ;
        }

        public bool TryReadComment()
        {
            if (code.Current == '#')
            {
                while (code.Current != '\n' && code.Current != '\0') code.MoveNext();
                TryReadWhitespace();
                return true;
            }
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

        public void WriteBytes(byte[] values)
        {
            foreach (var b in values)
            {
                WriteByte(b);
            }
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

        public bool AnyErrors()
        {
            return messages.Any(x => x.IsError);
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
