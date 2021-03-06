using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mips.Emulator;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace Mips.Assembler.UnitTest
{
    [TestClass]
    public class AssemblerInstanceTest
    {
        private Mock<IMemory> memoryMock;

        [TestInitialize]
        public void Initialize()
        {
            memoryMock = new Mock<IMemory>(MockBehavior.Loose);
        }

        public static IEnumerable<object[]> RegisterTestData => new[]
        {
            new object[] {"", null},
            new object[] {" ", null},
            new object[] {"$", null},

            new object[] {"$0", 0},
            new object[] {"$1", 1},
            new object[] {"$10", 10},
            new object[] {"$31", 31},
            new object[] {"$32", null},

            new object[] {"$v0", 2},
            new object[] {"$v1", 3},
            new object[] {"$v2", null},
            new object[] {"$a0", 4},
        };

        [TestMethod]
        [DynamicData(nameof(RegisterTestData))]

        public void TryReadRegister(string code, int? expectedRegister)
        {
            var target = new AssemblerInstance(code, memoryMock.Object);

            bool result = target.TryReadRegister(out int actualRegister);
            if (expectedRegister == null)
            {
                Assert.IsFalse(result);
            }
            else
            {
                Assert.IsTrue(result);
                Assert.AreEqual(expectedRegister, actualRegister);
            }
        }

        public static IEnumerable<object[]> NameTestData => new[]
        {
            new object[] {"", null},
            new object[] {" ", null},
            new object[] {"1", null},
            new object[] {"1n", null},
            new object[] {" n", null},

            new object[] {"name", "name"},
            new object[] {"name ", "name"},
            new object[] {"n1", "n1"},
            new object[] {"n_0", "n_0"},
            new object[] {"n_0 n_1", "n_0"},
        };

        [TestMethod]
        [DynamicData(nameof(NameTestData))]

        public void TryReadName(string code, string expectedName)
        {
            var target = new AssemblerInstance(code, memoryMock.Object);

            bool result = target.TryReadName(out string actualName);
            if (expectedName == null)
            {
                Assert.IsFalse(result);
            }
            else
            {
                Assert.IsTrue(result);
                Assert.AreEqual(expectedName, actualName);
            }
        }

        public static IEnumerable<object[]> LabelTestData => new[]
        {
            new object[] {"", false},
            new object[] {" ", false},
            new object[] {"1", false},
            new object[] {"1n", false},
            new object[] {" n", false},
            new object[] {"name", false},

            new object[] {"name:", true},
            new object[] {"n1:", true},
            new object[] {"n_1:", true},
            new object[] {"n_1: ", true},
        };

        [TestMethod]
        [DynamicData(nameof(LabelTestData))]

        public void TryReadLabel(string code, bool expected)
        {
            var target = new AssemblerInstance(code, memoryMock.Object);

            bool result = target.TryReadLabelDefinition();

            Assert.AreEqual(expected, result);
        }

        public static IEnumerable<object[]> SignedTestData => new[]
        {
            new object[] {"", null},
            new object[] {" ", null},

            new object[] {"0", 0},
            new object[] {"1", 1},
            new object[] {"-1", -1},
            new object[] {"-101", -101},
            new object[] {"-101 ", -101},

            new object[] {"999999999999", null},
            new object[] {"-999999999999", null},
        };

        [TestMethod]
        [DynamicData(nameof(SignedTestData))]

        public void TryReadSigned(string code, int? expected)
        {
            var target = new AssemblerInstance(code, memoryMock.Object);

            bool result = target.TryReadSigned(32, out int actual);
            if (expected == null)
            {
                Assert.IsFalse(result);
            }
            else
            {
                Assert.IsTrue(result);
                Assert.AreEqual(expected, actual);
            }
        }

        public static IEnumerable<object[]> UnsignedTestData => new[]
        {
            new object[] {"", null},
            new object[] {" ", null},

            new object[] {"0", 0u},
            new object[] {"1", 1u},
            new object[] {"101", 101u},
            new object[] {"101 ", 101u},

            new object[] {"-1", null},
            new object[] {"999999999999", null},
            new object[] {"-999999999999", null},
        };

        [TestMethod]
        [DynamicData(nameof(UnsignedTestData))]

        public void TryReadUnsigned(string code, uint? expected)
        {
            var target = new AssemblerInstance(code, memoryMock.Object);

            bool result = target.TryReadUnsigned(32, out uint actual);
            if (expected == null)
            {
                Assert.IsFalse(result);
            }
            else
            {
                Assert.IsTrue(result);
                Assert.AreEqual(expected, actual);
            }
        }

        public static IEnumerable<object[]> ValueWithOffsetTestData => new[]
        {
            new object[] {"", null, null},
            new object[] {" ", null, null},

            new object[] {"($0)", 0u, 0},
            new object[] {"0($0)", 0u, 0},
            new object[] {"1($1)", 1u, 1},
            new object[] {"101($v0)", 101u, 2},

            new object[] {"101($v0", null, null},
            new object[] {"101$v0)", null, null},
        };

        [TestMethod]
        [DynamicData(nameof(ValueWithOffsetTestData))]

        public void TryReadValueWithOffset(string code, uint? expectedValue, int? expectedRegister)
        {
            var target = new AssemblerInstance(code, memoryMock.Object);

            bool result = target.TryReadValueWithOffset(out uint actualValue, out int actualRegister);
            if (expectedValue == null)
            {
                Assert.IsFalse(result);
            }
            else
            {
                Assert.IsTrue(result);
                Assert.AreEqual(expectedValue, actualValue);
                Assert.AreEqual(expectedRegister, actualRegister);
            }
        }

        public static IEnumerable<object[]> AsciiStringTestData => new[]
        {
            new object[] {"", null},
            new object[] {"\"", null},
            new object[] {"\"\\\"", null},

            new object[] {"\"\"", System.Array.Empty<byte>()},
            new object[] {"\"a\"", new byte[] { (byte)'a' } },
            new object[] {"\"\\\\\"", new byte[] { (byte)'\\' } },
            new object[] {"\"\\\"\"", new byte[] { (byte)'"' } },
        };

        [TestMethod]
        [DynamicData(nameof(AsciiStringTestData))]

        public void TryReadAsciiString(string code, byte[] expectedValue)
        {
            var target = new AssemblerInstance(code, memoryMock.Object);

            bool result = target.TryReadAsciiString(out byte[] actualValue);
            if (expectedValue == null)
            {
                Assert.IsFalse(result);
            }
            else
            {
                Assert.IsTrue(result);
                CollectionAssert.AreEqual(expectedValue, actualValue);
            }
        }

        public static IEnumerable<object[]> InstructionTestData => new[]
        {
            new object[] {"$3", null},

            new object[] {"add $3,$4,$5", OperationEncoder.EncodeFormatR(4, 5, 3, 0, Functions.Add)},
            new object[] {"move $3,$4", OperationEncoder.EncodeFormatR(4, 0, 3, 0, Functions.Add)},
            new object[] {"add $3$4$5", null},
            new object[] {"add $4,$5", null},

            new object[] {"addu $3,$4,$5", OperationEncoder.EncodeFormatR(4, 5, 3, 0, Functions.Addu)},
            new object[] {"and $3,$4,$5", OperationEncoder.EncodeFormatR(4, 5, 3, 0, Functions.And)},
            new object[] {"nor $3,$4,$5", OperationEncoder.EncodeFormatR(4, 5, 3, 0, Functions.Nor)},
            new object[] {"or $3,$4,$5", OperationEncoder.EncodeFormatR(4, 5, 3, 0, Functions.Or)},
            new object[] {"sub $3,$4,$5", OperationEncoder.EncodeFormatR(4, 5, 3, 0, Functions.Sub)},
            new object[] {"subu $3,$4,$5", OperationEncoder.EncodeFormatR(4, 5, 3, 0, Functions.Subu)},
            new object[] {"xor $3,$4,$5", OperationEncoder.EncodeFormatR(4, 5, 3, 0, Functions.Xor)},
            new object[] {"slt $3,$4,$5", OperationEncoder.EncodeFormatR(4, 5, 3, 0, Functions.Slt)},
            new object[] {"sltu $3,$4,$5", OperationEncoder.EncodeFormatR(4, 5, 3, 0, Functions.Sltu)},

            new object[] {"addi $3,$4,5", OperationEncoder.EncodeFormatI(Opcodes.Addi, 4, 3, 5u)},
            new object[] {"addiu $3,$4,5", OperationEncoder.EncodeFormatI(Opcodes.Addiu, 4, 3, 5u)},
            new object[] {"andi $3,$4,5", OperationEncoder.EncodeFormatI(Opcodes.Andi, 4, 3, 5u)},
            new object[] {"ori $3,$4,5", OperationEncoder.EncodeFormatI(Opcodes.Ori, 4, 3, 5u)},
            new object[] {"xori $3,$4,5", OperationEncoder.EncodeFormatI(Opcodes.Xori, 4, 3, 5u)},
            new object[] {"slti $3,$4,5", OperationEncoder.EncodeFormatI(Opcodes.Slti, 4, 3, 5u)},
            new object[] {"sltiu $3,$4,5", OperationEncoder.EncodeFormatI(Opcodes.Sltiu, 4, 3, 5u)},

            new object[] {"div $3,$4", OperationEncoder.EncodeFormatR(3, 4, 0, 0, Functions.Div)},
            new object[] {"divu $3,$4", OperationEncoder.EncodeFormatR(3, 4, 0, 0, Functions.Divu)},
            new object[] {"mult $3,$4", OperationEncoder.EncodeFormatR(3, 4, 0, 0, Functions.Mult)},
            new object[] {"multu $3,$4", OperationEncoder.EncodeFormatR(3, 4, 0, 0, Functions.Multu)},

            new object[] {"sll $3,$4,5", OperationEncoder.EncodeFormatR(0, 4, 3, 5, Functions.Sll)},
            new object[] {"sra $3,$4,5", OperationEncoder.EncodeFormatR(0, 4, 3, 5, Functions.Sra)},
            new object[] {"srl $3,$4,5", OperationEncoder.EncodeFormatR(0, 4, 3, 5, Functions.Srl)},

            new object[] {"sllv $3,$4,$5", OperationEncoder.EncodeFormatR(5, 4, 3, 0, Functions.Sllv)},
            new object[] {"srav $3,$4,$5", OperationEncoder.EncodeFormatR(5, 4, 3, 0, Functions.Srav)},
            new object[] {"srlv $3,$4,$5", OperationEncoder.EncodeFormatR(5, 4, 3, 0, Functions.Srlv)},

            new object[] {"jr $3", OperationEncoder.EncodeFormatR(3, 0, 0, 0, Functions.Jr)},
            new object[] {"jalr $3", OperationEncoder.EncodeFormatR(3, 0, 0, 0, Functions.Jalr)},
            new object[] {"mfhi $3", OperationEncoder.EncodeFormatR(3, 0, 0, 0, Functions.Mfhi)},
            new object[] {"mflo $3", OperationEncoder.EncodeFormatR(3, 0, 0, 0, Functions.Mflo)},

            new object[] {"l: beq $3,$4,l", OperationEncoder.EncodeFormatI(Opcodes.Beq, 3, 4, 0xFFFF)},
            new object[] {"l: bne $3,$4,l", OperationEncoder.EncodeFormatI(Opcodes.Bne, 3, 4, 0xFFFF)},

            new object[] {"l: bgtz $3,l", OperationEncoder.EncodeFormatI(Opcodes.Bgtz, 3, 0, 0xFFFF)},
            new object[] {"l: blez $3,l", OperationEncoder.EncodeFormatI(Opcodes.Blez, 3, 0, 0xFFFF)},
            new object[] {"l: beqz $3,l", OperationEncoder.EncodeFormatI(Opcodes.Beq, 3, 0, 0xFFFF)},

            new object[] {"lui $3,4", OperationEncoder.EncodeFormatI(Opcodes.Lui, 0, 3, 4u)},

            new object[] {"lb $3,4($5)", OperationEncoder.EncodeFormatI(Opcodes.Lb, 5, 3, 4u)},
            new object[] {"lbu $3,4($5)", OperationEncoder.EncodeFormatI(Opcodes.Lbu, 5, 3, 4u)},
            new object[] {"lh $3,4($5)", OperationEncoder.EncodeFormatI(Opcodes.Lh, 5, 3, 4u)},
            new object[] {"lhu $3,4($5)", OperationEncoder.EncodeFormatI(Opcodes.Lhu, 5, 3, 4u)},
            new object[] {"lw $3,4($5)", OperationEncoder.EncodeFormatI(Opcodes.Lw, 5, 3, 4u)},
            new object[] {"sb $3,4($5)", OperationEncoder.EncodeFormatI(Opcodes.Sb, 5, 3, 4u)},
            new object[] {"sh $3,4($5)", OperationEncoder.EncodeFormatI(Opcodes.Sh, 5, 3, 4u)},
            new object[] {"sw $3,4($5)", OperationEncoder.EncodeFormatI(Opcodes.Sw, 5, 3, 4u)},

            new object[] {"l: j l", OperationEncoder.EncodeFormatJ(0u, false)},
            new object[] {"l: jal l", OperationEncoder.EncodeFormatJ(0u, true)},
        };

        [TestMethod]
        [DynamicData(nameof(InstructionTestData))]

        public void AssembleSingleInstruction(string code, uint? expected)
        {
            var result = MipsAsm.Assemble(code, memoryMock.Object);

            var anyErrors = result.Messages.Any(x => x.IsError);
            if (expected != null)
            {
                memoryMock.Verify(x => x.StoreWord(0u, (uint)expected));
                memoryMock.Verify(x => x.StoreWord(4u, Cpu.TerminateInstruction));
                Assert.IsFalse(anyErrors);
            }
            else
            {
                Assert.IsTrue(anyErrors);
            }
        }

        public static IEnumerable<object[]> WordWriteInstructionData => new[]
        {
            new object[] {"l: la $3,l", new []
                {
                    OperationEncoder.EncodeFormatI(Opcodes.Lui, 0, 3, 0u),
                    OperationEncoder.EncodeFormatI(Opcodes.Ori, 3, 3, 0u),
                }
            },
            new object[] {"li $3,2882339107", new []
                {
                    OperationEncoder.EncodeFormatI(Opcodes.Lui, 0, 3, 0xABCD),
                    OperationEncoder.EncodeFormatI(Opcodes.Ori, 3, 3, 0x0123),
                }
            },
            new object[] {".word 123456789", new uint[] {0x075BCD15}},
            new object[] {".word -123456789", new uint[] {0xF8A432EB}},
            new object[] {".word 0xDEADBEEF", new uint[] {0xDEADBEEF}},
        };

        [TestMethod]
        [DynamicData(nameof(WordWriteInstructionData))]

        public void AssembleWordInstructions(string code, uint[] expected)
        {
            var result = MipsAsm.Assemble(code, memoryMock.Object);

            var anyErrors = result.Messages.Any(x => x.IsError);
            uint i = 0;
            for (; i < expected.Length; i++)
            {
                memoryMock.Verify(x => x.StoreWord(i * 4u, expected[i]));
            }
            memoryMock.Verify(x => x.StoreWord(i * 4u, Cpu.TerminateInstruction));
            Assert.IsFalse(anyErrors);
        }

        public static IEnumerable<object[]> ByteWriteInstructionData => new[]
        {
            new object[] {".ascii \"abcd\"", new byte[] {0x61, 0x62, 0x63, 0x64}},
            new object[] {".asciiz \"abcd\"", new byte[] {0x61, 0x62, 0x63, 0x64, 0x00}},
            new object[] {".space 1", new byte[] {0x00}},
            new object[] {".space 4", new byte[] {0x00, 0x00, 0x00, 0x00}},
        };

        [TestMethod]
        [DynamicData(nameof(ByteWriteInstructionData))]

        public void AssembleByteInstructions(string code, byte[] expected)
        {
            var result = MipsAsm.Assemble(code, memoryMock.Object);

            var anyErrors = result.Messages.Any(x => x.IsError);
            uint i = 0;
            for (; i < expected.Length; i++)
            {
                memoryMock.VerifySet(x => x[i] = expected[i]);
            }
            Assert.IsFalse(anyErrors);
        }
    }
}
