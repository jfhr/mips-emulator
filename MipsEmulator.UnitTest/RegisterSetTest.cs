using Microsoft.VisualStudio.TestTools.UnitTesting;
using MipsEmulator.Services;

namespace MipsEmulator.UnitTest
{
    [TestClass]
    public class RegisterSetTest
    {
        /// <summary>
        /// Register 0 should always read 0 and ignore writes.
        /// </summary>
        [TestMethod]
        public void WriteTo0()
        {
            var target = new RegisterSet();

            target[0] = 1234;

            Assert.AreEqual((uint)0, target[0]);
        }

        /// <summary>
        /// Register 1 and all others should be writable.
        /// </summary>
        [TestMethod]
        public void WriteTo1()
        {
            var target = new RegisterSet();

            target[1] = 1234;

            Assert.AreEqual((uint)1234, target[1]);
        }
    }
}
