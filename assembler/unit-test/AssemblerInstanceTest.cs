using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mips.Emulator;
using Moq;
using System.Collections.Generic;

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

            new object[] {"\"\"", new byte[0]},
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
    }
}
