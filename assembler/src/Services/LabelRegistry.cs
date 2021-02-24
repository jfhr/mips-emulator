using System;
using System.Collections.Generic;

namespace Mips.Assembler.Services
{
    public class LabelRegistry : ILabelRegistry
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

        public LabelRegistry(IErrorMessageHelper errorMessageHelper, IBinaryCodeWriter binaryCodeWriter)
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
}
