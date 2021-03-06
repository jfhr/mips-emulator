using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mips.Emulator.UnitTest
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
            RegisterSet target = new();

            target[0] = 1234;

            Assert.AreEqual(0u, target[0]);
            Assert.AreEqual(0u, target.Zero);
        }

        [TestMethod]
        public void WriteToAt()
        {
            RegisterSet target = new();

            target.At = 1234;

            Assert.AreEqual(1234u, target[1]);
            Assert.AreEqual(1234u, target.At);
        }

        [TestMethod]
        public void WriteToV0()
        {
            RegisterSet target = new();

            target.V0 = 1234;

            Assert.AreEqual(1234u, target[2]);
            Assert.AreEqual(1234u, target.V0);
        }


        [TestMethod]
        public void WriteToV1()
        {
            RegisterSet target = new();

            target.V1 = 1234;

            Assert.AreEqual(1234u, target[3]);
            Assert.AreEqual(1234u, target.V1);
        }


        [TestMethod]
        public void WriteToA0()
        {
            RegisterSet target = new();

            target.A0 = 1234;

            Assert.AreEqual(1234u, target[4]);
            Assert.AreEqual(1234u, target.A0);
        }


        [TestMethod]
        public void WriteToA1()
        {
            RegisterSet target = new();

            target.A1 = 1234;

            Assert.AreEqual(1234u, target[5]);
            Assert.AreEqual(1234u, target.A1);
        }


        [TestMethod]
        public void WriteToA2()
        {
            RegisterSet target = new();

            target.A2 = 1234;

            Assert.AreEqual(1234u, target[6]);
            Assert.AreEqual(1234u, target.A2);
        }


        [TestMethod]
        public void WriteToA3()
        {
            RegisterSet target = new();

            target.A3 = 1234;

            Assert.AreEqual(1234u, target[7]);
            Assert.AreEqual(1234u, target.A3);
        }


        [TestMethod]
        public void WriteToT0()
        {
            RegisterSet target = new();

            target.T0 = 1234;

            Assert.AreEqual(1234u, target[8]);
            Assert.AreEqual(1234u, target.T0);
        }


        [TestMethod]
        public void WriteToT1()
        {
            RegisterSet target = new();

            target.T1 = 1234;

            Assert.AreEqual(1234u, target[9]);
            Assert.AreEqual(1234u, target.T1);
        }


        [TestMethod]
        public void WriteToT2()
        {
            RegisterSet target = new();

            target.T2 = 1234;

            Assert.AreEqual(1234u, target[10]);
            Assert.AreEqual(1234u, target.T2);
        }


        [TestMethod]
        public void WriteToT3()
        {
            RegisterSet target = new();

            target.T3 = 1234;

            Assert.AreEqual(1234u, target[11]);
            Assert.AreEqual(1234u, target.T3);
        }


        [TestMethod]
        public void WriteToT4()
        {
            RegisterSet target = new();

            target.T4 = 1234;

            Assert.AreEqual(1234u, target[12]);
            Assert.AreEqual(1234u, target.T4);
        }


        [TestMethod]
        public void WriteToT5()
        {
            RegisterSet target = new();

            target.T5 = 1234;

            Assert.AreEqual(1234u, target[13]);
            Assert.AreEqual(1234u, target.T5);
        }


        [TestMethod]
        public void WriteToT6()
        {
            RegisterSet target = new();

            target.T6 = 1234;

            Assert.AreEqual(1234u, target[14]);
            Assert.AreEqual(1234u, target.T6);
        }


        [TestMethod]
        public void WriteToT7()
        {
            RegisterSet target = new();

            target.T7 = 1234;

            Assert.AreEqual(1234u, target[15]);
            Assert.AreEqual(1234u, target.T7);
        }


        [TestMethod]
        public void WriteToS0()
        {
            RegisterSet target = new();

            target.S0 = 1234;

            Assert.AreEqual(1234u, target[16]);
            Assert.AreEqual(1234u, target.S0);
        }


        [TestMethod]
        public void WriteToS1()
        {
            RegisterSet target = new();

            target.S1 = 1234;

            Assert.AreEqual(1234u, target[17]);
            Assert.AreEqual(1234u, target.S1);
        }


        [TestMethod]
        public void WriteToS2()
        {
            RegisterSet target = new();

            target.S2 = 1234;

            Assert.AreEqual(1234u, target[18]);
            Assert.AreEqual(1234u, target.S2);
        }


        [TestMethod]
        public void WriteToS3()
        {
            RegisterSet target = new();

            target.S3 = 1234;

            Assert.AreEqual(1234u, target[19]);
            Assert.AreEqual(1234u, target.S3);
        }


        [TestMethod]
        public void WriteToS4()
        {
            RegisterSet target = new();

            target.S4 = 1234;

            Assert.AreEqual(1234u, target[20]);
            Assert.AreEqual(1234u, target.S4);
        }


        [TestMethod]
        public void WriteToS5()
        {
            RegisterSet target = new();

            target.S5 = 1234;

            Assert.AreEqual(1234u, target[21]);
            Assert.AreEqual(1234u, target.S5);
        }


        [TestMethod]
        public void WriteToS6()
        {
            RegisterSet target = new();

            target.S6 = 1234;

            Assert.AreEqual(1234u, target[22]);
            Assert.AreEqual(1234u, target.S6);
        }


        [TestMethod]
        public void WriteToS7()
        {
            RegisterSet target = new();

            target.S7 = 1234;

            Assert.AreEqual(1234u, target[23]);
            Assert.AreEqual(1234u, target.S7);
        }


        [TestMethod]
        public void WriteToT8()
        {
            RegisterSet target = new();

            target.T8 = 1234;

            Assert.AreEqual(1234u, target[24]);
            Assert.AreEqual(1234u, target.T8);
        }


        [TestMethod]
        public void WriteToT9()
        {
            RegisterSet target = new();

            target.T9 = 1234;

            Assert.AreEqual(1234u, target[25]);
            Assert.AreEqual(1234u, target.T9);
        }


        [TestMethod]
        public void WriteToK0()
        {
            RegisterSet target = new();

            target.K0 = 1234;

            Assert.AreEqual(1234u, target[26]);
            Assert.AreEqual(1234u, target.K0);
        }


        [TestMethod]
        public void WriteToK1()
        {
            RegisterSet target = new();

            target.K1 = 1234;

            Assert.AreEqual(1234u, target[27]);
            Assert.AreEqual(1234u, target.K1);
        }


        [TestMethod]
        public void WriteToGp()
        {
            RegisterSet target = new();

            target.Gp = 1234;

            Assert.AreEqual(1234u, target[28]);
            Assert.AreEqual(1234u, target.Gp);
        }


        [TestMethod]
        public void WriteToSp()
        {
            RegisterSet target = new();

            target.Sp = 1234;

            Assert.AreEqual(1234u, target[29]);
            Assert.AreEqual(1234u, target.Sp);
        }


        [TestMethod]
        public void WriteToFp()
        {
            RegisterSet target = new();

            target.Fp = 1234;

            Assert.AreEqual(1234u, target[30]);
            Assert.AreEqual(1234u, target.Fp);
        }


        [TestMethod]
        public void WriteToRa()
        {
            RegisterSet target = new();

            target.Ra = 1234;

            Assert.AreEqual(1234u, target[31]);
            Assert.AreEqual(1234u, target.Ra);
        }


        [TestMethod]
        public void WriteToLo()
        {
            RegisterSet target = new();

            target.Lo = 1234;

            Assert.AreEqual(1234u, target[32]);
            Assert.AreEqual(1234u, target.Lo);
        }


        [TestMethod]
        public void WriteToHi()
        {
            RegisterSet target = new();

            target.Hi = 1234;

            Assert.AreEqual(1234u, target[33]);
            Assert.AreEqual(1234u, target.Hi);
        }

    }
}
