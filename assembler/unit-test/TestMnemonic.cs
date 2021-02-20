using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace Mips.Assembler.UnitTest
{
    [TestClass]
    public class TestMnemonic
    {
        public static IEnumerable<object[]> WhitespaceTestData => new[]
        {
            new object[] {"", 0, 0},
            new object[] {" ", 0, 1},
            new object[] {" a", 0, 1},
            new object[] {"a ", 0, 0},
            new object[] {" a ", 0, 1},
            new object[] {"  a", 0, 2},
        };

        [TestMethod]
        [DynamicData(nameof(WhitespaceTestData))]
        public void TestWhitespace(string code, int startIndex, int expectedIndex)
        {
            int actual = Whitespace.Instance.TryRead(code, startIndex);
            Assert.AreEqual(expectedIndex, actual);
        }

        public static IEnumerable<object[]> CommaTestData => new[]
        {
            new object[] {"", 0, 0},
            new object[] {" ", 0, 0},
            new object[] {",", 0, 1},
            new object[] {" , ", 0, 3},
            new object[] {" , abc", 0, 3},
            new object[] {"a , ", 0, 0},
            new object[] {"a , ", 1, 4},
        };

        [TestMethod]
        [DynamicData(nameof(CommaTestData))]
        public void TestComma(string code, int startIndex, int expectedIndex)
        {
            int actual = Comma.Instance.TryRead(code, startIndex);
            Assert.AreEqual(expectedIndex, actual);
        }

        public static IEnumerable<object[]> LabelTestData => new[]
        {
            new object[] {"", 0, 0, null},
            new object[] {" ", 0, 0, null},
            new object[] {"l:", 0, 2, "l"},
            new object[] {"l", 0, 0, null},
            new object[] {"label:", 0, 6, "label"},
            new object[] {"label :", 0, 0, null},
            new object[] {":", 0, 0, null},
        };

        [TestMethod]
        [DynamicData(nameof(LabelTestData))]
        public void TestLabel(string code, int startIndex, int expectedIndex, string expectedName)
        {
            var tokenBrokerMock = new Mock<ITokenBroker>(MockBehavior.Loose);
            var target = new Label(tokenBrokerMock.Object);

            int actual = target.TryRead(code, startIndex);

            Assert.AreEqual(expectedIndex, actual);
            if (expectedName != null)
            {
                tokenBrokerMock.Verify(x => x.DefineLabel(expectedName, It.IsAny<int>()), Times.Once);
            }
            tokenBrokerMock.VerifyNoOtherCalls();
        }

        public static IEnumerable<object[]> RegisterTestData => new[]
        {
            new object[] {"", 0, 0, null},
            new object[] {" ", 0, 0, null},

            new object[] {"$1  ", 0, 2, 1},
            new object[] {"$1,$2", 0, 2, 1},

            new object[] {"$0", 0, 2, 0},
            new object[] {"$1", 0, 2, 1},
            new object[] {"$31", 0, 3, 31},
            new object[] {"$32", 0, 0, null},

            new object[] {"$v0", 0, 3, 2},
            new object[] {"$v1", 0, 3, 3},
            new object[] {"$a0", 0, 3, 4},
            new object[] {"$a3", 0, 3, 7},
            new object[] {"$s0", 0, 3, 16},
            new object[] {"$s7", 0, 3, 23},
            new object[] {"$t0", 0, 3, 8},
            new object[] {"$t7", 0, 3, 15},
            new object[] {"$t8", 0, 3, 24},

            new object[] {"$sp", 0, 3, 29},
            new object[] {"$fp", 0, 3, 30},
            new object[] {"$ra", 0, 3, 31},
        };

        [TestMethod]
        [DynamicData(nameof(RegisterTestData))]
        public void TestRegister(string code, int startIndex, int expectedIndex, int? expectedRegisterIndex)
        {
            var parameterQueueMock = new Mock<IParameterQueue>(MockBehavior.Loose);
            var target = new Register(parameterQueueMock.Object);

            int actual = target.TryRead(code, startIndex);

            Assert.AreEqual(expectedIndex, actual);
            if (expectedRegisterIndex != null)
            {
                parameterQueueMock.Verify(x => x.AddSigned((int)expectedRegisterIndex));
            }
            parameterQueueMock.VerifyNoOtherCalls();
        }
    }
}
