using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mips.Assembler.UnitTest
{
    [TestClass]
    public class TestParameterQueue
    {
        [TestMethod]
        public void TestSignedOnly()
        {
            var target = new ParameterQueue();

            target.AddSigned(200);
            target.AddSigned(-200);

            Assert.IsTrue(target.TryGetSigned(out int shouldBe200));
            Assert.AreEqual(200, shouldBe200);
            Assert.IsTrue(target.TryGetSigned(out int shouldBeNegative200));
            Assert.AreEqual(-200, shouldBeNegative200);
        }

        [TestMethod]
        public void TestUnsignedOnly()
        {
            var target = new ParameterQueue();

            target.AddUnsigned(100);
            target.AddUnsigned(200);

            Assert.IsTrue(target.TryGetUnsigned(out uint shouldBe100));
            Assert.AreEqual(100u, shouldBe100);
            Assert.IsTrue(target.TryGetUnsigned(out uint shouldBe200));
            Assert.AreEqual(200u, shouldBe200);
        }

        [TestMethod]
        public void TestMixed()
        {
            var target = new ParameterQueue();

            target.AddSigned(-200);
            target.AddUnsigned(200);
            target.AddSigned(300);
            target.AddUnsigned(300);

            Assert.IsTrue(target.TryGetSigned(out int shouldBeNegative200));
            Assert.AreEqual(-200, shouldBeNegative200);
            Assert.IsTrue(target.TryGetUnsigned(out uint shouldBe200));
            Assert.AreEqual(200u, shouldBe200);
            Assert.IsTrue(target.TryGetSigned(out int shouldBe300));
            Assert.AreEqual(300, shouldBe300);
            Assert.IsTrue(target.TryGetUnsigned(out uint shouldBe300u));
            Assert.AreEqual(300u, shouldBe300u);

            Assert.IsFalse(target.TryGetSigned(out int _));
            Assert.IsFalse(target.TryGetUnsigned(out uint _));
        }
    }
}
