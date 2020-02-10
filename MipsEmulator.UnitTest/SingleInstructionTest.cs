using Microsoft.VisualStudio.TestTools.UnitTesting;
using MipsEmulator.Services;
using System;

namespace MipsEmulator.UnitTest
{
    [TestClass]
    public class SingleInstructionTest
    {
        private Cpu target;
        private OperationEncoder encoder;


        [TestInitialize]
        public void Initialize()
        {
            target = new Cpu();
            encoder = new OperationEncoder();
        }


        [TestMethod]
        public void J()
        {
            // jump to 100000 (2 trailing zeroes are implicit)
            target.Memory.StoreWord(0, 0b0000_1000_0000_0000_0000_0000_0000_1000);

            target.FIE();

            Assert.AreEqual((uint)0b1000_00, target.Pc);
        }

        [TestMethod]
        public void JAL()
        {
            // jump to 100000 (2 trailing zeroes are implicit)
            // opcode 000011 means jump and link
            // bc the sequence is (increment, then execute)
            // the CPU should link back to the instruction following the jal, here [4]
            target.Memory.StoreWord(0, 0b0000_1100_0000_0000_0000_0000_0000_1000);

            target.FIE();

            Assert.AreEqual((uint)0b1000_00, target.Pc);
            Assert.AreEqual((uint)4, target.Registers.Ra);
        }

        [TestMethod]
        public void JR()
        {
            target.Registers.S0 = 0xDEADBEEF;
            // jump to the address in register  10_0000  ($s0)
            target.Memory.StoreWord(0, 0b0000_0010_0000_0000_0000_0000_0000_1000);

            target.FIE();

            Assert.AreEqual(0xDEADBEEF, target.Pc);
        }

        [TestMethod]
        public void ADD()
        {
            target.Registers[4] = 123;
            target.Registers[5] = 234;
            var ins = new FormatR(4, 5, 6, 0, 0b10_0000);
            target.Memory.StoreWord(0, encoder.EncodeInstruction(ins));

            target.FIE();

            Assert.AreEqual(123u + 234u, target.Registers[6]);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void ADD_Overflow()
        {
            target.Registers[4] = int.MaxValue;
            target.Registers[5] = 234;
            var ins = new FormatR(4, 5, 6, 0, 0b10_0000);
            target.Memory.StoreWord(0, encoder.EncodeInstruction(ins));

            target.FIE();
        }

        [TestMethod]
        public void ADDI()
        {
            target.Registers[4] = 123;
            var ins = new FormatI(0b00_1000, 4, 6, 234);
            target.Memory.StoreWord(0, encoder.EncodeInstruction(ins));

            target.FIE();

            Assert.AreEqual(123u + 234u, target.Registers[6]);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void ADDI_Overflow()
        {
            target.Registers[4] = int.MaxValue;
            var ins = new FormatI(0b00_1000, 4, 6, 234);
            target.Memory.StoreWord(0, encoder.EncodeInstruction(ins));

            target.FIE();
        }

        [TestMethod]
        public void ADDIU()
        {
            target.Registers[4] = 123;
            var ins = new FormatI(0b00_1001, 4, 6, 234);
            target.Memory.StoreWord(0, encoder.EncodeInstruction(ins));

            target.FIE();

            Assert.AreEqual(123u + 234u, target.Registers[6]);
        }

        [TestMethod]
        public void ADDIU_Overflow()
        {
            target.Registers[4] = int.MaxValue;
            var ins = new FormatI(0b00_1001, 4, 6, 234);
            target.Memory.StoreWord(0, encoder.EncodeInstruction(ins));

            target.FIE();

            Assert.AreEqual(int.MaxValue + 234u, target.Registers[6]);
        }


        [TestMethod]
        public void ADDU()
        {
            target.Registers[4] = 123;
            target.Registers[5] = 234;
            var ins = new FormatR(4, 5, 6, 0, 0b10_0001);
            target.Memory.StoreWord(0, encoder.EncodeInstruction(ins));

            target.FIE();

            Assert.AreEqual(123u + 234u, target.Registers[6]);
        }

        [TestMethod]
        public void ADDU_Overflow()
        {
            target.Registers[4] = int.MaxValue;
            target.Registers[5] = 234;
            var ins = new FormatR(4, 5, 6, 0, 0b10_0001);
            target.Memory.StoreWord(0, encoder.EncodeInstruction(ins));

            target.FIE();

            Assert.AreEqual(int.MaxValue + 234u, target.Registers[6]);
        }

        [TestMethod]
        public void AND()
        {
            target.Registers[4] = 0b1101_0011;
            target.Registers[5] = 0b1001_1010;
            const uint expected = 0b1001_0010;
            var ins = new FormatR(4, 5, 6, 0, 0b10_0100);
            target.Memory.StoreWord(0, encoder.EncodeInstruction(ins));

            target.FIE();

            Assert.AreEqual(expected, target.Registers[6]);
        }

        [TestMethod]
        public void ANDI()
        {
            target.Registers[4] = 0b1101_0011;
            const uint inputVal = 0b1001_1010;
            const uint expected = 0b1001_0010;
            var ins = new FormatI(0b00_1100, 4, 6, inputVal);
            target.Memory.StoreWord(0, encoder.EncodeInstruction(ins));

            target.FIE();

            Assert.AreEqual(expected, target.Registers[6]);
        }

        [TestMethod]
        public void BEQ_IsEqual()
        {
            target.Registers[4] = 123;
            target.Registers[5] = 123;
            const uint offset = 1024;
            const uint expectedPc = (offset << 2) + 4;
            var ins = new FormatI(0b00_0100, 4, 5, offset);
            target.Memory.StoreWord(0, encoder.EncodeInstruction(ins));

            target.FIE();

            Assert.AreEqual(expectedPc, target.Pc);
        }

        [TestMethod]
        public void BEQ_IsNotEqual()
        {
            target.Registers[4] = 123;
            target.Registers[5] = 900;
            const uint offset = 1024;
            var ins = new FormatI(0b00_0100, 4, 5, offset);
            target.Memory.StoreWord(0, encoder.EncodeInstruction(ins));

            target.FIE();

            Assert.AreEqual(4u, target.Pc);
        }
    }
}
