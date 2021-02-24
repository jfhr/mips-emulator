using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mips.Assembler.Mnemonics;
using Mips.Assembler.Services;
using Moq;
using System.Collections.Generic;

namespace Mips.Assembler.UnitTest
{
    [TestClass]
    public class TestSimpleMnemonics
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
            var tokenBrokerMock = new Mock<ILabelRegistry>(MockBehavior.Loose);
            var target = new Label(tokenBrokerMock.Object);

            int actual = target.TryRead(code, startIndex);

            Assert.AreEqual(expectedIndex, actual);
            if (expectedName != null)
            {
                tokenBrokerMock.Verify(x => x.DefineLabel(expectedName, It.IsAny<int>()), Times.Once);
            }
            tokenBrokerMock.VerifyNoOtherCalls();
        }
    }
}
