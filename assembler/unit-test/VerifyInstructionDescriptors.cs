using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using Moq;
using Mips.Emulator;

namespace Mips.Assembler.UnitTest
{
    [TestClass]
    public class VerifyInstructionDescriptors
    {
        private void EnsureCpuKnowsInstructionR(uint function)
        {
            uint ins = function;
            var memory = new Memory();
            memory.StoreWord(0, ins);
            var cpu = new Cpu(memory, new RegisterSet());

            try
            {
                cpu.CycleOnce();
            }
            catch (NotImplementedException)
            {
                Assert.Fail($"CPU doesn't know function {Convert.ToString(function, 2)}");
            }
            catch 
            {
                // other exception (e.g. DivideByZero) are ok
            }
        }

        private void EnsureCpuKnowsInstructionI(uint opcode)
        {
            uint ins = (opcode << 26);
            var memory = new Memory();
            memory.StoreWord(0, ins);
            var cpu = new Cpu(memory, new RegisterSet());

            try
            {
                cpu.CycleOnce();
            }
            catch (NotImplementedException)
            {
                Assert.Fail($"CPU doesn't know opcode {Convert.ToString(opcode, 2)}");
            }
            catch
            {
                // other exception (e.g. DivideByZero) are ok
            }
        }

        [TestMethod]
        public void InstructionDescriptorsLookOk()
        {
            var formatRTypes = new[]
            {
                InstructionSyntaxType.ArithLog,
                InstructionSyntaxType.DivMult,
                InstructionSyntaxType.Shift,
                InstructionSyntaxType.ShiftV,
                InstructionSyntaxType.RJumpOrMove,
            };

            var names = new HashSet<string>();
            var functions = new HashSet<uint>();
            var opcodes = new HashSet<uint>();

            foreach (var i in AssemblerData.Instructions)
            {
                Assert.IsTrue(i.Description.StartsWith(i.Name), 
                    "Syntax help must start with the instruction name.");
                Assert.IsTrue(i.Description.EndsWith("."),
                    "Syntax help must end with a period.");
                Assert.IsTrue(names.Add(i.Name),
                    $"Name {i.Name} occurs more than once.");

                if (formatRTypes.Contains(i.SyntaxType))
                {
                    Assert.IsTrue(functions.Add(i.FunctionOrOpcode),
                        $"Function {Convert.ToString(i.FunctionOrOpcode, 2)} occurs more than once.");
                    EnsureCpuKnowsInstructionR(i.FunctionOrOpcode);
                }
                else
                {
                    Assert.IsTrue(opcodes.Add(i.FunctionOrOpcode),
                        $"Opcode {Convert.ToString(i.FunctionOrOpcode, 2)} occurs more than once.");
                    EnsureCpuKnowsInstructionI(i.FunctionOrOpcode);
                }
            }
        }

        [TestMethod]
        public void CreateRuntimeOk()
        {
            new AssemblerRuntime(Mock.Of<IMemory>());
        }
    }
}
