using Mips.Emulator;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Mips.Assembler
{
    public class Assembler
    {
        private readonly string code;

        public Assembler(string code)
        {
            this.code = code;
        }
    }

    public enum ErrorMessageType
    {
        SyntaxError,
        LabelNotDefined,
        LabelDefinedMultipleTimes,
        NonAsciiCharacterInAsciiString,
    }

    public class BinaryCodeWriter : IBinaryCodeWriter
    {
        private readonly IMemory memory;

        public BinaryCodeWriter(IMemory memory)
        {
            this.memory = memory;
        }

        public uint CurrentAddress { get; private set; }

        public void WriteWord(uint value)
        {
            // get next multiple of 4
            CurrentAddress += 3;
            CurrentAddress &= 0xFFFFFFFC;

            memory.StoreWord(CurrentAddress, value);

            CurrentAddress += 4;
        }

        public void Write(byte[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                memory[CurrentAddress] = value[i];
                CurrentAddress++;
            }
        }
    }

    public class TokenBroker : ITokenBroker
    {
        private class LabelAction
        {
            public LabelAction(string labelName, Action<uint> action, int indexInCode)
            {
                LabelName = labelName;
                Action = action;
                IndexInCode = indexInCode;
            }

            public string LabelName { get; }
            public Action<uint> Action { get; }
            public int IndexInCode { get; }
        }

        private readonly List<LabelAction> labelActions = new List<LabelAction>();
        private readonly Dictionary<string, uint> labels = new Dictionary<string, uint>();
        private readonly IErrorMessageHelper errorMessageHelper;
        private readonly IBinaryCodeWriter binaryCodeWriter;

        public TokenBroker(IErrorMessageHelper errorMessageHelper, IBinaryCodeWriter binaryCodeWriter)
        {
            this.errorMessageHelper = errorMessageHelper;
            this.binaryCodeWriter = binaryCodeWriter;
        }

        public void PushInstruction(uint instruction)
        {
            binaryCodeWriter.WriteWord(instruction);
        }

        public void DefineLabel(string name, int indexInCode)
        {
            if (labels.ContainsKey(name))
            {
                errorMessageHelper.Add(ErrorMessageType.LabelDefinedMultipleTimes, indexInCode);
            }
            else
            {
                labels[name] = binaryCodeWriter.CurrentAddress;
            }
        }

        public void DoWithLabel(string labelName, Action<uint> action, int indexInCode)
        {
            if (labels.TryGetValue(labelName, out uint value))
            {
                action(value);
            }
            else
            {
                labelActions.Add(new LabelAction(labelName, action, indexInCode));
            }
        }

        public void DoRemainingLabels()
        {
            foreach (var a in labelActions)
            {
                if (labels.TryGetValue(a.LabelName, out uint value))
                {
                    a.Action(value);
                }
                else
                {
                    errorMessageHelper.Add(ErrorMessageType.LabelNotDefined, a.IndexInCode);
                }
            }
        }
    }

    /// <summary>
    /// Reads whitespace, including tab, newline, carriage return, etc.
    /// </summary>
    public class Whitespace : IMnemonic
    {
        public static IMnemonic Instance { get; } = new Whitespace();

        private Whitespace() { }

        public int TryRead(string code, int startIndex)
        {
            while (startIndex < code.Length && char.IsWhiteSpace(code, startIndex)) startIndex++;
            return startIndex;
        }
    }

    /// <summary>
    /// Reads a single comma surrounded by optional whitespace.
    /// </summary>
    public class Comma : IMnemonic
    {
        public static IMnemonic Instance { get; } = new Comma();

        private Comma() { }

        public int TryRead(string code, int startIndex)
        {
            int index = startIndex;
            index = Whitespace.Instance.TryRead(code, index);
            if (index < code.Length && code[index] == ',')
            {
                index++;
                index = Whitespace.Instance.TryRead(code, index);
                return index;
            }

            return startIndex;
        }
    }

    /// <summary>
    /// Reads a label declaration.
    /// </summary>
    public class Label : IMnemonic
    {
        private readonly ITokenBroker tokenBroker;

        public Label(ITokenBroker tokenBroker)
        {
            this.tokenBroker = tokenBroker;
        }

        public int TryRead(string code, int startIndex)
        {
            int index = startIndex;
            if (index < code.Length && char.IsLetter(code, index))
            {
                index++;
                while (index < code.Length && char.IsLetterOrDigit(code, index)) index++;
                if (index < code.Length && code[index] == ':')
                {
                    string name = code.Substring(startIndex, index - startIndex);
                    tokenBroker.DefineLabel(name, index);
                    return index + 1;
                }
            }
            return startIndex;
        }
    }

    /// <summary>
    /// Reads a register reference and places the register index in a parameter queue.
    /// </summary>
    public class Register : IMnemonic
    {
        /// <summary>
        /// Named register, the index of a name in the array is equal to the
        /// associated register index.
        /// </summary>
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

        private readonly IParameterQueue parameterQueue;

        public Register(IParameterQueue parameterQueue)
        {
            this.parameterQueue = parameterQueue;
        }

        public int TryRead(string code, int startIndex)
        {
            int index = startIndex;
            if (index < code.Length && code[startIndex] == '$')
            {
                index++;
                if (index + 1 < code.Length)
                {
                    string reference = code.Substring(index, 2);
                    // NumberStyles.None bc we don't allow leading or trailing stuff
                    if (int.TryParse(reference, NumberStyles.None, null, out int regIndex))
                    {
                        if (0 > regIndex || regIndex >= 32)
                        {
                            return startIndex;
                        }
                        parameterQueue.AddSigned(regIndex);
                        return index + 2;
                    }
                    int regNameIndex = Array.IndexOf(registerNames, reference);
                    if (regNameIndex != -1)
                    {
                        parameterQueue.AddSigned(regNameIndex);
                        return index + 2;
                    }
                }
                if (index < code.Length)
                {
                    string reference = code.Substring(index, 1);
                    if (int.TryParse(reference, NumberStyles.None, null, out int regIndex))
                    {
                        parameterQueue.AddSigned(regIndex);
                        return index + 1;
                    }
                }
            }

            return startIndex;
        }
    }

    public class Instruction : IMnemonic
    {
        private readonly string instructionName;
        private readonly IMnemonic[] parameters;

        public Instruction(string instructionName, params IMnemonic[] parameters)
        {
            this.instructionName = instructionName;
            this.parameters = parameters;
        }

        public int TryRead(string code, int startIndex)
        {
            // read instruction name
            for (int i = 0; i < instructionName.Length; i++)
            {
                if (startIndex + i >= code.Length || instructionName[i] != code[startIndex + i])
                {
                    return startIndex;
                }
            }

            // read parameters
            int index = startIndex + instructionName.Length;
            index = Whitespace.Instance.TryRead(code, index);

            for (int p = 0; p < parameters.Length; p++)
            {
                if (index == (index = parameters[p].TryRead(code, index)))
                {
                    // failed to read parameter
                    return startIndex;
                }

                // if this isn't the last parameter, try to read a comma
                if (p != parameters.Length - 1 && index == (index = Comma.Instance.TryRead(code, index)))
                {
                    // failed to read comma
                    return startIndex;
                }
            }

            return index;
        }
    }
}
