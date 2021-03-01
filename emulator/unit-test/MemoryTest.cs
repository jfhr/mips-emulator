using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mips.Emulator.UnitTest
{
    [TestClass]
    public class MemoryTest
    {
        [TestMethod]
        public void Read_0()
        {
            var target = new Memory();

            byte value = target[0];

            Assert.AreEqual(0, value);
        }

        [TestMethod]
        public void Write_0()
        {
            var target = new Memory();

            target[0] = 210;

            Assert.AreEqual(210, target[0]);
        }

        [TestMethod]
        public void Read_0xDEADBEEF()
        {
            var target = new Memory();

            byte value = target[0xDEADBEEF];

            Assert.AreEqual(0, value);
        }

        [TestMethod]
        public void Write_0xDEADBEEF()
        {
            var target = new Memory();

            target[0xDEADBEEF] = 189;

            Assert.AreEqual(189, target[0xDEADBEEF]);
        }

        [TestMethod]
        public void LoadWord_0()
        {
            var target = new Memory();

            uint value = target.LoadWord(0);

            Assert.AreEqual((uint)0, value);
        }

        [TestMethod]
        public void StoreWord_0()
        {
            var target = new Memory();

            target.StoreWord(0, 0xDEADBEEF);

            Assert.AreEqual(0xDEADBEEF, target.LoadWord(0));
            Assert.AreEqual(0xDE, target[0]);
            Assert.AreEqual(0xAD, target[1]);
            Assert.AreEqual(0xBE, target[2]);
            Assert.AreEqual(0xEF, target[3]);
            Assert.AreEqual(0, target[4]);
        }

        [TestMethod]
        public void StoreWord_25002()
        {
            var target = new Memory();

            // 25002 is not div by 4, so should store at 25000 instead.
            target.StoreWord(25002, 0xDEADBEEF);

            Assert.AreEqual(0xDEADBEEF, target.LoadWord(25000));
            Assert.AreEqual(0xDEADBEEF, target.LoadWord(25003));
            Assert.AreEqual(0xDE, target[25000]);
            Assert.AreEqual(0xAD, target[25001]);
            Assert.AreEqual(0xBE, target[25002]);
            Assert.AreEqual(0xEF, target[25003]);
            Assert.AreEqual(0, target[25004]);
        }

        [TestMethod]
        public void Reset()
        {
            var target = new Memory();
            target.StoreWord(0xF00BA, 123u);
            target[0xDEADBEEF] = 189;

            target.Reset();

            Assert.AreEqual(0, target[0xDEADBEEF]);
            Assert.AreEqual(0u, target.LoadWord(0xF00BA));
        }
    }
}
