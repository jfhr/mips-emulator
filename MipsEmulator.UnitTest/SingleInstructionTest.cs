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

        /// <summary>
        /// Asserts that the CPU branched after the first instruction 
        /// and throws an <see cref="AssertFailedException"/> if it didn't.
        /// </summary>
        private void AssertDidBranch(uint offset)
        {
            // Since we are only testing single instructions, we know the PC will be at 4
            // after the first instruction, unless it branched.
            Assert.AreNotEqual(4u, target.Pc, "CPU did not branch when it should have branched.");
            uint expectedPc = (offset << 2) + 4u;
            Assert.AreEqual(expectedPc, target.Pc, "CPU branched to a different offset than it should have.");
        }

        /// <summary>
        /// Asserts that the CPU did not branch after the first instruction 
        /// and throws an <see cref="AssertFailedException"/> if it did.
        /// </summary>
        private void AssertDidNotBranch()
        {
            Assert.AreEqual(4u, target.Pc, "CPU branched when it shouldn't have.");
        }

        /// <summary>
        /// Asserts that the CPU branched after the first instruction 
        /// and linked back and throws an <see cref="AssertFailedException"/> 
        /// if it didn't.
        /// </summary>
        private void AssertDidBranchAndLink(uint offset)
        {
            AssertDidBranch(offset);
            Assert.AreEqual(4u, target.Registers.Ra, "CPU did not link back when it should have.");
        }

        /// <summary>
        /// Encodes the instruction into a word and stores it in memory
        /// as the first instruction.
        /// </summary>
        private void PushInstruction(IInstructionFormat ins)
        {
            target.Memory.StoreWord(0, encoder.EncodeInstruction(ins));
        }


        [TestInitialize]
        public void Initialize()
        {
            target = new Cpu();
            encoder = new OperationEncoder();
        }


        [TestMethod]
        public void ADD()
        {
            target.Registers[4] = 123;
            target.Registers[5] = 234;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_0000));

            target.FIE();

            Assert.AreEqual(123u + 234u, target.Registers[6]);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void ADD_Overflow()
        {
            target.Registers[4] = int.MaxValue;
            target.Registers[5] = 234;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_0000));

            target.FIE();
        }

        [TestMethod]
        public void ADDI()
        {
            target.Registers[4] = 123;
            PushInstruction(new FormatI(0b00_1000, 4, 6, 234));

            target.FIE();

            Assert.AreEqual(123u + 234u, target.Registers[6]);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void ADDI_Overflow()
        {
            target.Registers[4] = int.MaxValue;
            PushInstruction(new FormatI(0b00_1000, 4, 6, 234));

            target.FIE();
        }

        [TestMethod]
        public void ADDIU()
        {
            target.Registers[4] = 123;
            PushInstruction(new FormatI(0b00_1001, 4, 6, 234));

            target.FIE();

            Assert.AreEqual(123u + 234u, target.Registers[6]);
        }

        [TestMethod]
        public void ADDIU_Overflow()
        {
            target.Registers[4] = int.MaxValue;
            PushInstruction(new FormatI(0b00_1001, 4, 6, 234));

            target.FIE();

            Assert.AreEqual(int.MaxValue + 234u, target.Registers[6]);
        }


        [TestMethod]
        public void ADDU()
        {
            target.Registers[4] = 123;
            target.Registers[5] = 234;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_0001));

            target.FIE();

            Assert.AreEqual(123u + 234u, target.Registers[6]);
        }

        [TestMethod]
        public void ADDU_Overflow()
        {
            target.Registers[4] = int.MaxValue;
            target.Registers[5] = 234;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_0001));

            target.FIE();

            Assert.AreEqual(int.MaxValue + 234u, target.Registers[6]);
        }

        [TestMethod]
        public void AND()
        {
            target.Registers[4] = 0b1101_0011;
            target.Registers[5] = 0b1001_1010;
            const uint expected = 0b1001_0010;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_0100));

            target.FIE();

            Assert.AreEqual(expected, target.Registers[6]);
        }

        [TestMethod]
        public void ANDI()
        {
            target.Registers[4] = 0b1101_0011;
            const uint inputVal = 0b1001_1010;
            const uint expected = 0b1001_0010;
            PushInstruction(new FormatI(0b00_1100, 4, 6, inputVal));

            target.FIE();

            Assert.AreEqual(expected, target.Registers[6]);
        }

        [TestMethod]
        public void BEQ_IsEqual()
        {
            target.Registers[4] = 123;
            target.Registers[5] = 123;
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0100, 4, 5, offset));

            target.FIE();

            AssertDidBranch(offset);
        }

        [TestMethod]
        public void BEQ_IsNotEqual()
        {
            target.Registers[4] = 123;
            target.Registers[5] = 900;
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0100, 4, 5, offset));

            target.FIE();

            AssertDidNotBranch();
        }

        [TestMethod]
        public void BGEZ_IsGreaterThanZero()
        {
            target.Registers[4] = 1;
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0001, 4, 0b0_0001, offset));

            target.FIE();

            AssertDidBranch(offset);
        }

        [TestMethod]
        public void BGEZ_IsZero()
        {
            target.Registers[4] = 0;
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0001, 4, 0b0_0001, offset));

            target.FIE();

            AssertDidBranch(offset);
        }

        [TestMethod]
        public void BGEZ_IsLessThanZero()
        {
            target.Registers[4] = 0xFFFFFFFF;  // -1
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0001, 4, 0b0_0001, offset));

            target.FIE();

            AssertDidNotBranch();
        }

        [TestMethod]
        public void BGEZAL_IsGreaterThanZero()
        {
            target.Registers[4] = 1;
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0001, 4, 0b1_0001, offset));

            target.FIE();

            AssertDidBranchAndLink(offset);
        }

        [TestMethod]
        public void BGEZAL_IsZero()
        {
            target.Registers[4] = 0;
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0001, 4, 0b1_0001, offset));

            target.FIE();

            AssertDidBranchAndLink(offset);
        }

        [TestMethod]
        public void BGEZAL_IsLessThanZero()
        {
            target.Registers[4] = 0xFFFFFFFF;  // -1
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0001, 4, 0b1_0001, offset));

            target.FIE();

            AssertDidNotBranch();
        }

        [TestMethod]
        public void BGTZ_IsGreaterThanZero()
        {
            target.Registers[4] = 1;
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0111, 4, 0b0_0000, offset));

            target.FIE();

            AssertDidBranch(offset);
        }

        [TestMethod]
        public void BGTZ_IsZero()
        {
            target.Registers[4] = 0;
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0111, 4, 0b0_0000, offset));

            target.FIE();

            AssertDidNotBranch();
        }

        [TestMethod]
        public void BGTZ_IsLessThanZero()
        {
            target.Registers[4] = 0xFFFFFFFF;  // -1
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0111, 4, 0b0_0000, offset));

            target.FIE();

            AssertDidNotBranch();
        }

        [TestMethod]
        public void BLEZ_IsGreaterThanZero()
        {
            target.Registers[4] = 1;
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0110, 4, 0b0_0001, offset));

            target.FIE();

            AssertDidNotBranch();
        }

        [TestMethod]
        public void BLEZ_IsZero()
        {
            target.Registers[4] = 0;
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0110, 4, 0b0_0001, offset));

            target.FIE();

            AssertDidBranch(offset);
        }

        [TestMethod]
        public void BLEZ_IsLessThanZero()
        {
            target.Registers[4] = 0xFFFFFFFF;  // -1
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0110, 4, 0b0_0001, offset));

            target.FIE();

            AssertDidBranch(offset);
        }

        [TestMethod]
        public void BLTZ_IsGreaterThanZero()
        {
            target.Registers[4] = 1;
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0001, 4, 0b0_0000, offset));

            target.FIE();

            AssertDidNotBranch();
        }

        [TestMethod]
        public void BLTZ_IsZero()
        {
            target.Registers[4] = 0;
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0001, 4, 0b0_0000, offset));

            target.FIE();

            AssertDidNotBranch();
        }

        [TestMethod]
        public void BLTZ_IsLessThanZero()
        {
            target.Registers[4] = 0xFFFFFFFF;  // -1
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0001, 4, 0b0_0000, offset));

            target.FIE();

            AssertDidBranch(offset);
        }

        [TestMethod]
        public void BLTZAL_IsGreaterThanZero()
        {
            target.Registers[4] = 1;
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0001, 4, 0b1_0000, offset));

            target.FIE();

            AssertDidNotBranch();
        }

        [TestMethod]
        public void BLTZAL_IsZero()
        {
            target.Registers[4] = 0;
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0001, 4, 0b1_0000, offset));

            target.FIE();

            AssertDidNotBranch();
        }

        [TestMethod]
        public void BLTZAL_IsLessThanZero()
        {
            target.Registers[4] = 0xFFFFFFFF;  // -1
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0001, 4, 0b1_0000, offset));

            target.FIE();

            AssertDidBranchAndLink(offset);
        }

        [TestMethod]
        public void BNE_IsEqual()
        {
            target.Registers[4] = 123;
            target.Registers[5] = 123;
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0101, 4, 5, offset));

            target.FIE();

            AssertDidNotBranch();
        }

        [TestMethod]
        public void BNE_IsNotEqual()
        {
            target.Registers[4] = 123;
            target.Registers[5] = 900;
            const uint offset = 1024;
            PushInstruction(new FormatI(0b00_0101, 4, 5, offset));

            target.FIE();

            AssertDidBranch(offset);
        }

        [TestMethod]
        public void DIV()
        {
            // 220 : 12 = 18 R 4
            target.Registers[4] = 220;
            target.Registers[5] = 12;
            PushInstruction(new FormatR(4, 5, 0, 0, 0b01_1010));

            target.FIE();

            Assert.AreEqual(18u, target.Registers.Lo);
            Assert.AreEqual(4u, target.Registers.Hi);
        }

        [TestMethod]
        public void DIV_NumeratorIsNegative()
        {
            target.Registers[4] = unchecked((uint)-220);
            target.Registers[5] = 12;
            PushInstruction(new FormatR(4, 5, 0, 0, 0b01_1010));

            target.FIE();

            Assert.AreEqual(unchecked((uint)-18), target.Registers.Lo);
            Assert.AreEqual(unchecked((uint)-4), target.Registers.Hi);
        }

        [TestMethod]
        public void DIV_DenominatorIsNegative()
        {
            target.Registers[4] = 220;
            target.Registers[5] = unchecked((uint)-12);
            PushInstruction(new FormatR(4, 5, 0, 0, 0b01_1010));

            target.FIE();

            Assert.AreEqual(unchecked((uint)-18), target.Registers.Lo);
            Assert.AreEqual(4u, target.Registers.Hi);
        }
        [TestMethod]
        public void DIV_BothAreNegative()
        {
            target.Registers[4] = unchecked((uint)-220);
            target.Registers[5] = unchecked((uint)-12);
            PushInstruction(new FormatR(4, 5, 0, 0, 0b01_1010));

            target.FIE();

            Assert.AreEqual(18u, target.Registers.Lo);
            Assert.AreEqual(unchecked((uint)-4), target.Registers.Hi);
        }

        [TestMethod]
        public void DIVU_NumeratorIsNegative()
        {
            // 4,294,967,295 : 12 = 357,913,941 R 3
            target.Registers[4] = 4294967295;
            target.Registers[5] = 12;
            PushInstruction(new FormatR(4, 5, 0, 0, 0b01_1011));

            target.FIE();

            Assert.AreEqual(357_913_941u, target.Registers.Lo);
            Assert.AreEqual(3u, target.Registers.Hi);
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
        public void LB()
        {
            const uint address = 0xF00BA;
            const byte value = 0xAB;
            target.Memory[address] = value;
            target.Registers[4] = address;
            PushInstruction(new FormatI(0b10_0000, 4, 6, 0));

            target.FIE();

            Assert.AreEqual(value, target.Registers[6]);
        }

        [TestMethod]
        public void LUI()
        {
            const uint value = 0xabcd;
            const uint expected = value << 16;
            PushInstruction(new FormatI(0b00_1111, 0, 6, value));

            target.FIE();

            Assert.AreEqual(expected, target.Registers[6]);
        }

        [TestMethod]
        public void LW()
        {
            const uint address = 0xF00BA;
            const uint value = 0xDEADBEEF;
            target.Memory.StoreWord(address, value);
            target.Registers[4] = address;
            PushInstruction(new FormatI(0b10_0011, 4, 6, 0));

            target.FIE();

            Assert.AreEqual(value, target.Registers[6]);
        }

        [TestMethod]
        public void MFHI()
        {
            const uint value = 0xDEADBEEF;
            target.Registers.Hi = value;
            PushInstruction(new FormatR(0, 0, 6, 0, 0b01_0000));

            target.FIE();

            Assert.AreEqual(value, target.Registers[6]);
        }
        
        [TestMethod]
        public void MFLO()
        {
            const uint value = 0xDEADBEEF;
            target.Registers.Lo = value;
            PushInstruction(new FormatR(0, 0, 6, 0, 0b01_0010));

            target.FIE();

            Assert.AreEqual(value, target.Registers[6]);
        }
        
        [TestMethod]
        public void MULT()
        {
            // 0x1BADC0DE * 0x0DEDF00D = 0x1818C9B_2028EB46
            target.Registers[4] = 0x1BADC0DE;
            target.Registers[5] = 0x0DEDF00D;
            PushInstruction(new FormatR(4, 5, 0, 0, 0b01_1000));

            target.FIE();

            Assert.AreEqual(0x2028EB46u, target.Registers.Lo);
            Assert.AreEqual(0x1818C9Bu, target.Registers.Hi);
        }

        [TestMethod]
        public void MULT_OneFactorIsNegative()
        {
            target.Registers[4] = unchecked((uint)-0x1BADC0DE);
            target.Registers[5] = 0x0DEDF00D;
            PushInstruction(new FormatR(4, 5, 0, 0, 0b01_1000));

            target.FIE();
            
            Assert.AreEqual(0xDFD714BAu, target.Registers.Lo);
            Assert.AreEqual(0xFE7E7364u, target.Registers.Hi);
        }

        [TestMethod]
        public void MULT_BothAreNegative()
        {
            target.Registers[4] = unchecked((uint)-0x1BADC0DE);
            target.Registers[5] = unchecked((uint)-0x0DEDF00D);
            PushInstruction(new FormatR(4, 5, 0, 0, 0b01_1000));

            target.FIE();

            Assert.AreEqual(0x2028EB46u, target.Registers.Lo);
            Assert.AreEqual(0x1818C9Bu, target.Registers.Hi);
        }

        [TestMethod]
        public void MULTU_FactorIsNegative()
        {
            // 0xDEADBEEF * 0xFEEDC0DE = 0xDDBF320E_4F21D342
            target.Registers[4] = 0xDEADBEEF;
            target.Registers[5] = 0xFEEDC0DE;
            PushInstruction(new FormatR(4, 5, 0, 0, 0b01_1001));

            target.FIE();

            Assert.AreEqual(0xDDBF320Eu, target.Registers.Hi);
            Assert.AreEqual(0x4F21D342u, target.Registers.Lo);
        }
    }
}
