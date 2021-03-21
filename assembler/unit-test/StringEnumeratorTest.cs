using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mips.Assembler.UnitTest
{
    [TestClass]
    public class StringEnumeratorTest
    {
        [TestMethod]
        public void TestSingleLine()
        {
            const string text = "Hi";
            StringEnumerator target = new(text);

            Assert.AreEqual(2, target.Length);
            Assert.AreEqual('\0', target.Current);

            Assert.IsTrue(target.MoveNext());
            Assert.AreEqual('H', target.Current);
            Assert.IsTrue(target.MoveNext());
            Assert.AreEqual('i', target.Current);
            Assert.IsTrue(target.MoveNext());
            Assert.IsFalse(target.MoveNext());

            Assert.IsTrue(target.MovePrevious());
            Assert.AreEqual('i', target.Current);
            Assert.AreEqual(1, target.Index);
        }

        [TestMethod]
        public void TestMultiLine()
        {
            const string text = "Hi\nFi\nLo";
            StringEnumerator target = new(text);

            Assert.AreEqual(8, target.Length);
            Assert.AreEqual(-1, target.LineNumber);

            Assert.IsTrue(target.MoveNext());  // H
            Assert.IsTrue(target.MoveNext());  // i
            Assert.IsTrue(target.MoveNext());  // \n
            Assert.AreEqual(0, target.LineNumber);

            Assert.IsTrue(target.MoveNext());  // F
            Assert.AreEqual(1, target.LineNumber);
            Assert.IsTrue(target.MovePrevious());  // \n
            Assert.AreEqual(0, target.LineNumber);

            Assert.IsTrue(target.MoveTo(7));
            Assert.AreEqual(2, target.LineNumber);

            target.Reset();
            Assert.AreEqual(-1, target.LineNumber);
        }
    }
}
