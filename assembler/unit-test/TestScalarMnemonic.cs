using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mips.Assembler.Mnemonics;
using Mips.Assembler.Services;
using Moq;
using System.Collections.Generic;

namespace Mips.Assembler.UnitTest
{
    [TestClass]
    public class TestScalarMnemonic
    {
        public static IEnumerable<object[]> RegisterTestData => new[]
        {
            new object[] {"", 0, 0, null},
            new object[] {" ", 0, 0, null},

            new object[] {"1", 0, 1, 1},
            new object[] {"-1", 0, 2, -1},
            new object[] {"-1", 0, 2, -1},
            new object[] {"100", 0, 3, 100},

            new object[] {"0x0", 0, 3, 0},
            new object[] {"0x1", 0, 3, 1},
            new object[] {"0xF", 0, 3, 15},
            new object[] {"0xf", 0, 3, 15},
            new object[] {"0xaBc", 0, 5, 0xABC},

            new object[] {"0x", 0, 0, null},
            new object[] {"0xg", 0, 0, null},
            new object[] {"1xf", 0, 0, null},
        };

        [TestMethod]
        [DynamicData(nameof(RegisterTestData))]
        public void TestScalar(string code, int startIndex, int expectedIndex, int? expectedValue)
        {
            var parameterQueueMock = new Mock<IParameterQueue>(MockBehavior.Loose);
            var target = Scalar.GetInstance(parameterQueueMock.Object);

            int actual = target.TryRead(code, startIndex);

            Assert.AreEqual(expectedIndex, actual);
            if (expectedValue != null)
            {
                parameterQueueMock.Verify(x => x.AddSigned((int)expectedValue));
            }
            parameterQueueMock.VerifyNoOtherCalls();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Scalar.ResetInstance();
        }
    }
}
