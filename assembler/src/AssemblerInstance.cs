using Mips.Assembler.Properties;
using Mips.Emulator;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Mips.Assembler
{
    /// <summary>
    /// Instance of an assembler. Create a new instance if the code changes.
    /// </summary>
    public class AssemblerInstance : IAssemblerResult
    {
        private readonly StringEnumerator code;
        private readonly IMemory memory;

        private readonly List<Message> messages = new();
        private readonly Dictionary<string, uint> labels = new();
        private bool writeEnable = false;
        private bool secondPass = false;
        private uint memoryAddress = 0;

        public IEnumerable<Message> Messages => messages;
        public IReadOnlyDictionary<string, uint> Labels => labels;

        /// <summary>
        /// Creates a new code instance.
        /// </summary>
        public AssemblerInstance(string code, IMemory memory)
        {
            this.code = new(code);
            this.code.MoveNext();
            this.memory = memory;
        }

        /// <summary>
        /// Run the assembler.
        /// </summary>
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
            code.Reset();
            code.MoveNext();

            Pass();
            WriteWord(Cpu.TerminateInstruction);
            if (AnyErrors())
            {
                memory.Reset();
            }
        }

        /// <summary>
        /// Run one assembler pass.
        /// </summary>
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

        /// <summary>
        /// Try to read and process an assembler instruction.
        /// </summary>
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
                        MipsAsm.InstructionSyntaxType.BranchAlways => TryReadBranchAlways(),
                        MipsAsm.InstructionSyntaxType.DivMult => TryReadDivMult(info.FunctionOrOpcode),
                        MipsAsm.InstructionSyntaxType.Jump => TryReadJump(info.FunctionOrOpcode),
                        MipsAsm.InstructionSyntaxType.LoadI => TryReadLoadI(info.FunctionOrOpcode),
                        MipsAsm.InstructionSyntaxType.LoadStore => TryReadLoadStore(info.FunctionOrOpcode),
                        MipsAsm.InstructionSyntaxType.LI => TryReadLI(),
                        MipsAsm.InstructionSyntaxType.LA => TryReadLA(),
                        MipsAsm.InstructionSyntaxType.RJumpOrMove => TryReadJumpRMoveTo(info.FunctionOrOpcode),
                        MipsAsm.InstructionSyntaxType.Move => TryReadMove(),
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

        /// <summary>
        /// Try to read and process a special instruction.
        /// </summary>
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
                    ".globl" => TryReadAndLookupLabel(out uint _, out string _),
                    ".word" => TryReadWordAndWrite(),
                    ".space" => TryReadIntAndWriteAsSpace(),
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

        /// <summary>
        /// Try to read a word and write it to memory.
        /// </summary>
        public bool TryReadIntAndWriteAsSpace()
        {
            var startIndex = code.Index;
            if (TryReadUnsigned(32, out uint value))
            {
                WriteBytes(new byte[value]);
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        /// <summary>
        /// Try to read a word and write it to memory.
        /// </summary>
        public bool TryReadWordAndWrite()
        {
            var startIndex = code.Index;
            if (TryReadSigned(32, out int value))
            {
                WriteWord(unchecked((uint)value));
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        /// <summary>
        /// Try to read a string and write it to memory.
        /// </summary>
        public bool TryReadStringAndWrite()
        {
            if (TryReadAsciiString(out byte[] value))
            {
                WriteBytes(value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Try to read a string and write it to memory zero-terminated.
        /// </summary>
        public bool TryReadStringAndWriteWithZero()
        {
            if (TryReadAsciiString(out byte[] value))
            {
                WriteBytes(value);
                WriteByte(0);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Format R arithmetic-logical: add, addu, and, nor, or, sub, subu, xor, slt, sltu
        /// </summary>
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

        /// <summary>
        /// Format R div/mult: div, divu, mult, multu
        /// </summary>
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

        /// <summary>
        /// Format R shift: sll, sra, srl
        /// </summary>
        public bool TryReadShift(uint function)
        {
            var startIndex = code.Index;
            if (TryReadRegister(out int rd)
                && TryReadComma()
                && TryReadRegister(out int rt)
                && TryReadComma()
                && TryReadUnsigned(5, out uint shamt))
            {
                uint ins = OperationEncoder.EncodeFormatR(0, rt, rd, (int)shamt, function);
                WriteWord(ins);
                return true;
            }
            code.Index = startIndex;
            return false;
        }


        /// <summary>
        /// Format R shift value: sllv, srav, srlv
        /// </summary>
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

        /// <summary>
        /// Format R jump-register/move: jalr, jr, mfhi, mflo
        /// </summary>
        public bool TryReadJumpRMoveTo(uint function)
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

        /// <summary>
        /// Format R register move: move
        /// </summary>
        public bool TryReadMove()
        {
            var startIndex = code.Index;
            if (TryReadRegister(out int rd)
                && TryReadComma()
                && TryReadRegister(out int rs))
            {
                // move $d,$s is implemented as add $d,$0,$s
                uint ins = OperationEncoder.EncodeFormatR(rs, 0, rd, 0, Functions.Add);
                WriteWord(ins);
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        /// <summary>
        /// Format I arithmetic-logical: addi, addiu, andi, ori, xori, slti, sltiu
        /// </summary>
        public bool TryReadArithLogI(uint opcode)
        {
            var startIndex = code.Index;
            if (TryReadRegister(out int rt)
                && TryReadComma()
                && TryReadRegister(out int rs)
                && TryReadComma()
                && TryReadSigned(16, out int immed))
            {
                uint ins = OperationEncoder.EncodeFormatI(opcode, rs, rt, unchecked((uint)immed));
                WriteWord(ins);
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        /// <summary>
        /// Format I load: lui
        /// </summary>
        public bool TryReadLoadI(uint opcode)
        {
            var startIndex = code.Index;
            if (TryReadRegister(out int rt)
                && TryReadComma()
                && TryReadSigned(16, out int immed))
            {
                uint ins = OperationEncoder.EncodeFormatI(opcode, 0, rt, unchecked((uint)immed));
                WriteWord(ins);
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        /// <summary>
        /// Format I branch: beq, bne
        /// </summary>
        public bool TryReadBranch(uint opcode)
        {
            var startIndex = code.Index;
            if (TryReadRegister(out int rs)
                && TryReadComma()
                && TryReadRegister(out int rt)
                && TryReadComma()
                && TryReadAndLookupLabel(out uint targetAddress, out string labelName))
            {
                uint offset = CalculateBranchOffset(targetAddress, startIndex, labelName);
                uint ins = OperationEncoder.EncodeFormatI(opcode, rs, rt, offset);
                WriteWord(ins);
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        /// <summary>
        /// Format I branch zero: bgtz, blez, beqz
        /// </summary>
        public bool TryReadBranchZ(uint opcode)
        {
            var startIndex = code.Index;
            if (TryReadRegister(out int rs)
                && TryReadComma()
                && TryReadAndLookupLabel(out uint targetAddress, out string labelName))
            {
                uint offset = CalculateBranchOffset(targetAddress, startIndex, labelName);
                uint ins = OperationEncoder.EncodeFormatI(opcode, rs, 0, offset);
                WriteWord(ins);
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        /// <summary>
        /// Format I branch always: b
        /// </summary>
        public bool TryReadBranchAlways()
        {
            var startIndex = code.Index;
            if (TryReadAndLookupLabel(out uint targetAddress, out string labelName))
            {
                // b is implemented as beq $0,$0
                uint offset = CalculateBranchOffset(targetAddress, startIndex, labelName);
                uint ins = OperationEncoder.EncodeFormatI(Opcodes.Beq, 0, 0, offset);
                WriteWord(ins);
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        /// <summary>
        /// Format I load/store: lb, lbu, lh, lhu, lw, sb, sh, sw, lui
        /// </summary>
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

        /// <summary>
        /// Format I load address: la
        /// </summary>
        public bool TryReadLA()
        {
            var startIndex = code.Index;
            if (TryReadRegister(out int reg)
                && TryReadComma()
                && TryReadAndLookupLabel(out uint labelAddress, out string _))
            {
                WriteLoad32Instructions(reg, labelAddress);
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        /// <summary>
        /// Format I load 32: li
        /// </summary>
        public bool TryReadLI()
        {
            var startIndex = code.Index;
            if (TryReadRegister(out int reg)
                && TryReadComma()
                && TryReadSigned(32, out int value))
            {
                WriteLoad32Instructions(reg, unchecked((uint)value));
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        /// <summary>
        /// Format J: j, jal
        /// </summary>
        public bool TryReadJump(uint opcode)
        {
            var startIndex = code.Index;
            bool link = opcode == Opcodes.Jal;
            if (TryReadAndLookupLabel(out uint address, out string _))
            {
                uint ins = OperationEncoder.EncodeFormatJ(address, link);
                WriteWord(ins);
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        /// <summary>
        /// Try to read an ASCII string delimited by double quotation marks. 
        /// The usual escape sequences are allowed.
        /// </summary>
        public bool TryReadAsciiString(out byte[] value)
        {
            List<byte> bytes = new();
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

        /// <summary>
        /// Try to read a register reference and lookup its value.
        /// </summary>
        public bool TryReadAndLookupLabel(out uint labelAddress, out string labelName)
        {
            var startIndex = code.Index;
            if (TryReadName(out labelName))
            {
                if (labels.TryGetValue(labelName, out labelAddress))
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
                    AddError(startIndex, code.Index, Resources.LabelNotDefined, labelName);
                }
            }
            labelAddress = 0u;
            return false;
        }

        /// <summary>
        /// Try to read a label definition and register it.
        /// </summary>
        public bool TryReadLabelDefinition()
        {
            var startIndex = code.Index;
            if (TryReadName(out string name))
            {
                if (code.Current == ':')
                {
                    code.MoveNext();
                    if (!secondPass)
                    {
                        DefineLabel(startIndex, code.Index, name);
                    }
                    return true;
                }
            }
            code.Index = startIndex;
            return false;
        }

        /// <summary>
        /// Try to read an immediate value with register offset (e.g. 100($t0) ).
        /// </summary>
        public bool TryReadValueWithOffset(out uint value, out int offsetRegister)
        {
            if (TryReadSigned(16, out int signedValue))
            {
                value = unchecked((uint)signedValue);
            }
            else
            {
                value = 0;
            }

            if (code.Current == '(' && code.MoveNext()
                && TryReadRegister(out offsetRegister)
                && code.Current == ')')
            {
                code.MoveNext();
                return true;
            }
            offsetRegister = 0;
            return false;
        }

        /// <summary>
        /// Try to read a unsigned value that fits into <paramref name="bits"/> bits.
        /// </summary>
        public bool TryReadUnsigned(int bits, out uint value)
        {
            var startIndex = code.Index;

            while (IsDigit() && code.MoveNext()) ;
            string number = code[startIndex..code.Index];
            if (uint.TryParse(number, out value))
            {
                if (FitsUnsigned(bits, value))
                {
                    return true;
                }
                AddError(startIndex, code.Index, Resources.UnsignedOverflow, number, bits);
            }

            code.Index = startIndex;
            value = 0;
            return false;
        }

        /// <summary>
        /// Try to read a signed value that fits into <paramref name="bits"/> bits.
        /// </summary>
        public bool TryReadSigned(int bits, out int value)
        {
            var startIndex = code.Index;
            var numberStartIndex = startIndex;
            var style = NumberStyles.Integer;

            if (code.Current == '-')
            {
                code.MoveNext();
            }
            if (TryRead0X())
            {
                numberStartIndex += 2;
                style = NumberStyles.HexNumber;
                while (IsHexDigit() && code.MoveNext()) ;
            }
            else
            {
                while (IsDigit() && code.MoveNext()) ;
            }

            string number = code[numberStartIndex..code.Index];
            if (int.TryParse(number, style, null, out value))
            {
                if (FitsSigned(bits, value))
                {
                    return true;
                }
                AddError(startIndex, code.Index, Resources.SignedOverflow, number, bits);
            }
            // In case the value is too large for an int, but fits in a uint,
            // we then cast it (unchecked) to an int
            else if (uint.TryParse(number, style, null, out uint unsignedValue))
            {
                if (FitsUnsigned(bits, unsignedValue))
                {
                    value = unchecked((int)unsignedValue);
                    return true;
                }
                AddError(startIndex, code.Index, Resources.UnsignedOverflow, number, bits);
            }
            code.Index = startIndex;
            value = 0;
            return false;            
        }

        public bool TryRead0X()
        {
            var startIndex = code.Index;
            if (code.Current == '0'
                && code.MoveNext()
                && (code.Current == 'x' || code.Current == 'X'))
            {
                return true;
            }
            code.Index = startIndex;
            return false;
        }

        /// <summary>
        /// Try to read a register reference. Returns the register index.
        /// </summary>
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
                        string reference = code[(startIndex + 1)..(startIndex + 3)].ToLower();
                        int index = Array.IndexOf(Constants.RegisterNames, reference);
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

        /// <summary>
        /// Try to read a special name (starts with a dot).
        /// </summary>
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

        /// <summary>
        /// Try to read a (instruction or label) name.
        /// </summary>
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

        /// <summary>
        /// Try to read a comma, optionally surrounded by whitespace.
        /// </summary>
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

        /// <summary>
        /// Skip whitespace and comments, if there are any.
        /// </summary>
        public void SkipWhitespaceAndComments()
        {
            while (TryReadWhitespace() || TryReadComment()) ;
        }

        /// <summary>
        /// Try to read a single line comment.
        /// </summary>
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

        /// <summary>
        /// Try to read whitespace characters (including tab and newline).
        /// </summary>
        public bool TryReadWhitespace()
        {
            var result = false;
            while (char.IsWhiteSpace(code.Current) && code.MoveNext())
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Write two instructions to load a 32 bit immediate value into a register (lui, ori).
        /// </summary>
        private void WriteLoad32Instructions(int reg, uint value)
        {
            uint upper = (value & 0xFFFF_0000) >> 16;
            uint lower = value & 0x0000_FFFF;
            uint lui = OperationEncoder.EncodeFormatI(Opcodes.Lui, 0, reg, upper);
            uint ori = OperationEncoder.EncodeFormatI(Opcodes.Ori, reg, reg, lower);
            WriteWord(lui);
            WriteWord(ori);
        }

        /// <summary>
        /// Calculates if <paramref name="number"/> fits into <paramref name="bits"/> bits.
        /// </summary>
        private static bool FitsUnsigned(int bits, uint number)
        {
            if (bits < 0 || bits > 32)
            {
                throw new ArgumentException("Must be in the range [0, 32]", nameof(bits));
            }
            if (bits == 32)
            {
                return true;
            }
            uint max = 1u << bits;
            return number < max;
        }

        /// <summary>
        /// Calculates if <paramref name="number"/> fits into <paramref name="bits"/> bits.
        /// </summary>
        private static bool FitsSigned(int bits, int number)
        {
            if (bits < 0 || bits > 32)
            {
                throw new ArgumentException("Must be in the range [0, 32]", nameof(bits));
            }
            if (bits == 32)
            {
                return true;
            }
            int max = 1 << (bits - 1);
            int min = -max - 1;
            return min < number && number < max;
        }

        /// <summary>
        /// Calculate the offset from the current memory address to <paramref name="targetAddress"/> 
        /// for branch instructions.
        /// </summary>
        private uint CalculateBranchOffset(uint targetAddress, int startIndex, string labelName)
        {
            // subtract 4 bc the pc is incremented before the instruction is executed
            int offset = (int)(targetAddress - memoryAddress) - 4;
            if (offset < short.MinValue || offset > short.MaxValue)
            {
                AddError(startIndex, code.Index, Resources.BranchTooFarAwayFromLabel, labelName);
            }

            // return value as unsigned 16-bit
            uint unsigned = unchecked((uint)offset);
            // last 2 bits are always unset, so we leave those out
            unsigned >>= 2;
            unsigned &= 0x0000_FFFF;
            return unsigned;
        }

        /// <summary>
        /// Write a word into memory at the current address (4-bit aligned).
        /// </summary>
        public void WriteWord(uint word)
        {
            // get next greater or equal multiple of 4
            memoryAddress -= (memoryAddress % 4);

            if (writeEnable)
            {
                memory.StoreWord(memoryAddress, word);
            }

            memoryAddress += 4;
        }

        /// <summary>
        /// Write a byte into memory at the current address.
        /// </summary>
        public void WriteByte(byte value)
        {
            if (writeEnable)
            {
                memory[memoryAddress] = value;
            }

            memoryAddress++;
        }


        /// <summary>
        /// Write a byte sequence into memory at the current address.
        /// </summary>
        public void WriteBytes(byte[] values)
        {
            foreach (var b in values)
            {
                WriteByte(b);
            }
        }

        /// <summary>
        /// Add a label definition for the current memory address.
        /// </summary>
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

        /// <summary>
        /// Add an error message.
        /// </summary>
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
            messages.Add(new Message(startIndex, endIndex, true, message));
        }

        /// <summary>
        /// Add an info (syntax help) message.
        /// </summary>
        public void AddInfo(int startIndex, int endIndex, string message, params object[] args)
        {
            if (args.Length != 0)
            {
                message = string.Format(message, args);
            }

            messages.Add(new Message(startIndex, endIndex, false, message));
        }

        /// <summary>
        /// Indicates if any error messages have been recorded.
        /// </summary>
        public bool AnyErrors()
        {
            return messages.Any(x => x.IsError);
        }

        /// <summary>
        /// Indicates if the current character is an ASCII digit.
        /// </summary>
        public bool IsDigit()
        {
            return '0' <= code.Current && code.Current <= '9';
        }

        /// <summary>
        /// Indicates if the current character is an ASCII hexadecimal digit.
        /// </summary>
        public bool IsHexDigit()
        {
            return IsDigit() || ('a' <= code.Current && code.Current <= 'z') || ('A' <= code.Current && code.Current <= 'Z');
        }

        /// <summary>
        /// Indicates if the current character can be the first character of a name (letters and underscore).
        /// </summary>
        public bool IsNameFirstChar()
        {
            return char.IsLetter(code.Current) || code.Current == '_';
        }

        /// <summary>
        /// Indicates if the current character can be the second character of a name (letters, numbers and underscore).
        /// </summary>
        public bool IsNameSecondChar()
        {
            return char.IsLetterOrDigit(code.Current) || code.Current == '_';
        }

        /// <summary>
        /// Indicates if the current character is ASCII.
        /// </summary>
        public bool IsAscii()
        {
            return code.Current < 128;
        }
    }
}
