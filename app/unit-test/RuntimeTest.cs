using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Mips.App.UnitTest
{
    [TestClass]
    public class RuntimeTest
    {
        public static IEnumerable<object[]> CountLinesToIndexTestData => new[]
        {
            new object[] {"", 0, 0},
            new object[] {"foo\nbar", 0, 0},
            new object[] {"foo\nbar", 5, 1},
            new object[] {"\n\n\n\n", 3, 4},
            new object[] {"\n\n\n\n", 0, 1},
            new object[] {"a\n", 0, 0},
        };


        [TestMethod]
        [DynamicData(nameof(CountLinesToIndexTestData))]
        public void TestCountLinesToIndex(string text, int index, int expectedNumberOfLines)
        {
            Runtime.Code = text;
            int actual = Runtime.CountLinesToIndex(index);
            Assert.AreEqual(expectedNumberOfLines, actual);
        }
    }
}
