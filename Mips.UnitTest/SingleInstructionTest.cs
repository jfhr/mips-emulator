using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mips.Services;
using System;

namespace Mips.UnitTest
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
            uint expectedPc = (uint)((int)offset << 2) + 4;
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
        /// Asserts that the CPU jumped after the first instruction 
        /// and throws an <see cref="AssertFailedException"/> if it didn't.
        /// </summary>
        private void AssertDidJump(uint offset)
        {
            uint expectedPc = offset << 2;
            Assert.AreEqual(expectedPc, target.Pc, "CPU jumped to a different location than it should have.");
        }

        /// <summary>
        /// Asserts that the CPU jumped after the first instruction 
        /// and linked back and throws an <see cref="AssertFailedException"/> 
        /// if it didn't.
        /// </summary>
        private void AssertDidJumpAndLink(uint offset)
        {
            AssertDidJump(offset);
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
            uint offset = 0xBEE5;
            PushInstruction(new FormatJ(offset, false));

            target.FIE();

            AssertDidJump(offset);
        }

        [TestMethod]
        public void J_0()
        {
            uint offset = 0;
            PushInstruction(new FormatJ(offset, false));

            target.FIE();

            AssertDidJump(offset);
        }


        [TestMethod]
        public void JAL()
        {
            uint offset = 0xBEE5;
            PushInstruction(new FormatJ(offset, true));

            target.FIE();

            AssertDidJumpAndLink(offset);
        }

        [TestMethod]
        public void JAL_0()
        {
            uint offset = 0;
            PushInstruction(new FormatJ(offset, true));

            target.FIE();

            AssertDidJumpAndLink(offset);
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
        public void MULTU()
        {
            // 0x1BADC0DE * 0x0DEDF00D = 0x1818C9B_2028EB46
            target.Registers[4] = 0x1BADC0DE;
            target.Registers[5] = 0x0DEDF00D;
            PushInstruction(new FormatR(4, 5, 0, 0, 0b01_1001));

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

        [TestMethod]
        public void OR()
        {
            target.Registers[4] = 0b1101_0011;
            target.Registers[5] = 0b1001_1010;
            const uint expected = 0b1101_1011;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_0101));

            target.FIE();

            Assert.AreEqual(expected, target.Registers[6]);
        }

        [TestMethod]
        public void ORI()
        {
            target.Registers[4] = 0b1101_0011;
            const uint inputVal = 0b1001_1010;
            const uint expected = 0b1101_1011;
            PushInstruction(new FormatI(0b00_1101, 4, 6, inputVal));

            target.FIE();

            Assert.AreEqual(expected, target.Registers[6]);
        }

        [TestMethod]
        public void SB()
        {
            const uint address = 0xF00BA;
            const byte value = 0xAB;
            target.Registers[4] = address;
            target.Registers[6] = value;
            PushInstruction(new FormatI(0b10_1000, 4, 6, 0));

            target.FIE();

            Assert.AreEqual(value, target.Memory[address]);
        }

        [TestMethod]
        public void SLL()
        {
            const uint value = 0b1101_0011;
            const int shamt = 1;
            const uint expected = 0b1_1010_0110;
            target.Registers[4] = value;
            PushInstruction(new FormatR(0, 4, 6, shamt, 0b00_0000));

            target.FIE();

            Assert.AreEqual(expected, target.Registers[6]);
        }

        [TestMethod]
        public void SLL_ShiftBy3()
        {
            const uint value = 0b1101_0011;
            const int shamt = 3;
            const uint expected = 0b110_1001_1000;
            target.Registers[4] = value;
            PushInstruction(new FormatR(0, 4, 6, shamt, 0b00_0000));

            target.FIE();

            Assert.AreEqual(expected, target.Registers[6]);
        }

        [TestMethod]
        public void SLLV()
        {
            const uint value = 0b1101_0011;
            const int shamt = 1;
            const uint expected = 0b1_1010_0110;
            target.Registers[4] = value;
            target.Registers[5] = shamt;
            PushInstruction(new FormatR(5, 4, 6, 0, 0b00_0100));

            target.FIE();

            Assert.AreEqual(expected, target.Registers[6]);
        }

        [TestMethod]
        public void SLLV_ShiftBy3()
        {
            const uint value = 0b1101_0011;
            const int shamt = 3;
            const uint expected = 0b110_1001_1000;
            target.Registers[4] = value;
            target.Registers[5] = shamt;
            PushInstruction(new FormatR(5, 4, 6, 0, 0b00_0100));

            target.FIE();

            Assert.AreEqual(expected, target.Registers[6]);
        }

        [TestMethod]
        public void SLT_IsLessThan()
        {
            const uint greaterValue = 7800;
            const uint lesserValue = 2400;
            target.Registers[4] = lesserValue;
            target.Registers[5] = greaterValue;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_1010));

            target.FIE();

            Assert.AreEqual(1u, target.Registers[6]);
        }

        [TestMethod]
        public void SLT_IsGreaterThan()
        {
            const uint greaterValue = 7800;
            const uint lesserValue = 2400;
            target.Registers[4] = greaterValue;
            target.Registers[5] = lesserValue;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_1010));

            target.FIE();

            Assert.AreEqual(0u, target.Registers[6]);
        }

        [TestMethod]
        public void SLT_AreEqual()
        {
            const uint value = 2400;
            target.Registers[4] = value;
            target.Registers[5] = value;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_1010));

            target.FIE();

            Assert.AreEqual(0u, target.Registers[6]);
        }
        [TestMethod]

        public void SLT_NegativeIsLessThanPositive()
        {
            const uint greaterValue = 2400;
            const uint lesserValue = unchecked((uint)-7800);
            target.Registers[4] = lesserValue;
            target.Registers[5] = greaterValue;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_1010));

            target.FIE();

            Assert.AreEqual(1u, target.Registers[6]);
        }

        [TestMethod]
        public void SLT_NegativeIsLessThanNegative()
        {
            const uint greaterValue = unchecked((uint)-2400);
            const uint lesserValue = unchecked((uint)-7800);
            target.Registers[4] = greaterValue;
            target.Registers[5] = lesserValue;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_1010));

            target.FIE();

            Assert.AreEqual(0u, target.Registers[6]);
        }

        [TestMethod]
        public void SLTI_IsLessThan()
        {
            const uint greaterValue = 7800;
            const uint lesserValue = 2400;
            target.Registers[4] = lesserValue;
            PushInstruction(new FormatI(0b00_1010, 4, 6, greaterValue));

            target.FIE();

            Assert.AreEqual(1u, target.Registers[6]);
        }

        [TestMethod]
        public void SLTI_IsGreaterThan()
        {
            const uint greaterValue = 7800;
            const uint lesserValue = 2400;
            target.Registers[4] = greaterValue;
            PushInstruction(new FormatI(0b00_1010, 4, 6, lesserValue));

            target.FIE();

            Assert.AreEqual(0u, target.Registers[6]);
        }

        [TestMethod]
        public void SLTI_AreEqual()
        {
            const uint value = 7800;
            target.Registers[4] = value;
            PushInstruction(new FormatI(0b00_1010, 4, 6, value));

           target.FIE();

            Assert.AreEqual(0u, target.Registers[6]);
        }

        [TestMethod]
        public void SLTU_IsLessThan()
        {
            const uint greaterValue = 7800;
            const uint lesserValue = 2400;
            target.Registers[4] = lesserValue;
            target.Registers[5] = greaterValue;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_1011));

            target.FIE();

            Assert.AreEqual(1u, target.Registers[6]);
        }

        [TestMethod]
        public void SLTU_IsGreaterThan()
        {
            const uint greaterValue = 7800;
            const uint lesserValue = 2400;
            target.Registers[4] = greaterValue;
            target.Registers[5] = lesserValue;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_1011));

            target.FIE();

            Assert.AreEqual(0u, target.Registers[6]);
        }

        [TestMethod]
        public void SLTU_AreEqual()
        {
            const uint value = 2400;
            target.Registers[4] = value;
            target.Registers[5] = value;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_1011));

            target.FIE();

            Assert.AreEqual(0u, target.Registers[6]);
        }

        [TestMethod]

        public void SLTU_NegativeIsGreaterThanPositive()
        {
            // the 'negative' number is greater bc it is interpreted as unsigned
            const uint greaterValue = unchecked((uint)-7800);
            const uint lesserValue = 2400;
            target.Registers[4] = lesserValue;
            target.Registers[5] = greaterValue;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_1011));

            target.FIE();

            Assert.AreEqual(1u, target.Registers[6]);
        }

        [TestMethod]
        public void SLTU_NegativeIsLessThanNegative()
        {
            const uint greaterValue = unchecked((uint)-2400);
            const uint lesserValue = unchecked((uint)-7800);
            target.Registers[4] = greaterValue;
            target.Registers[5] = lesserValue;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_1011));

            target.FIE();

            Assert.AreEqual(0u, target.Registers[6]);
        }

        [TestMethod]
        public void SLTIU_IsLessThan()
        {
            const uint greaterValue = 7800;
            const uint lesserValue = 2400;
            target.Registers[4] = lesserValue;
            PushInstruction(new FormatI(0b00_1011, 4, 6, greaterValue));

            target.FIE();

            Assert.AreEqual(1u, target.Registers[6]);
        }

        [TestMethod]
        public void SLTIU_IsGreaterThan()
        {
            const uint greaterValue = 7800;
            const uint lesserValue = 2400;
            target.Registers[4] = greaterValue;
            PushInstruction(new FormatI(0b00_1011, 4, 6, lesserValue));

            target.FIE();

            Assert.AreEqual(0u, target.Registers[6]);
        }

        [TestMethod]
        public void SLTIU_AreEqual()
        {
            const uint value = 7800;
            target.Registers[4] = value;
            PushInstruction(new FormatI(0b00_1011, 4, 6, value));

            target.FIE();

            Assert.AreEqual(0u, target.Registers[6]);
        }
        
        [TestMethod]
        public void SRA()
        {
            const uint value = 0b0000_1101_0011;
            const int shamt = 3;
            const uint expected = 0b0000_0001_1010;
            target.Registers[4] = value;
            PushInstruction(new FormatR(0, 4, 6, shamt, 0b00_0011));

            target.FIE();

            Assert.AreEqual(expected, target.Registers[6]);
        }

        [TestMethod]
        public void SRA_Negative()
        {
            // the sign bit is shifted in
            const uint value = 0b1000_0000_0000_0000_0000_0000_1101_0011;
            const int shamt = 3;
            const uint expected = 0b1111_0000_0000_0000_0000_0000_0001_1010;
            target.Registers[4] = value;
            PushInstruction(new FormatR(0, 4, 6, shamt, 0b00_0011));

            target.FIE();

            Assert.AreEqual(expected, target.Registers[6]);
        }

        [TestMethod]
        public void SRL()
        {
            const uint value = 0b0000_1101_0011;
            const int shamt = 3;
            const uint expected = 0b0000_0001_1010;
            target.Registers[4] = value;
            PushInstruction(new FormatR(0, 4, 6, shamt, 0b00_0010));

            target.FIE();

            Assert.AreEqual(expected, target.Registers[6]);
        }

        [TestMethod]
        public void SRL_Negative()
        {
            // zeroes are shifted in
            const uint value = 0b1000_0000_0000_0000_0000_0000_1101_0011;
            const int shamt = 3;
            const uint expected = 0b0001_0000_0000_0000_0000_0000_0001_1010;
            target.Registers[4] = value;
            PushInstruction(new FormatR(0, 4, 6, shamt, 0b00_0010));

            target.FIE();

            Assert.AreEqual(expected, target.Registers[6]);
        }

        [TestMethod]
        public void SRLV()
        {
            const uint value = 0b0000_1101_0011;
            const int shamt = 3;
            const uint expected = 0b0000_0001_1010;
            target.Registers[4] = value;
            target.Registers[5] = shamt;
            PushInstruction(new FormatR(5, 4, 6, 0, 0b00_0110));

            target.FIE();

            Assert.AreEqual(expected, target.Registers[6]);
        }

        [TestMethod]
        public void SRLV_Negative()
        {
            // zeroes are shifted in
            const uint value = 0b1000_0000_0000_0000_0000_0000_1101_0011;
            const int shamt = 3;
            const uint expected = 0b0001_0000_0000_0000_0000_0000_0001_1010;
            target.Registers[4] = value;
            target.Registers[5] = shamt;
            PushInstruction(new FormatR(5, 4, 6, 0, 0b00_0110));

            target.FIE();

            Assert.AreEqual(expected, target.Registers[6]);
        }

        [TestMethod]
        public void SUB()
        {
            target.Registers[4] = 234;
            target.Registers[5] = 123;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_0010));

            target.FIE();

            Assert.AreEqual(234u - 123u, target.Registers[6]);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void SUB_Overflow()
        {
            target.Registers[4] = unchecked((uint)int.MinValue);
            target.Registers[5] = 234;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_0010));

            target.FIE();
        }

        [TestMethod]
        public void SUBU()
        {
            target.Registers[4] = 234;
            target.Registers[5] = 123;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_0011));

            target.FIE();

            Assert.AreEqual(234u - 123u, target.Registers[6]);
        }

        [TestMethod]
        public void SUBU_Overflow()
        {
            uint minuend = unchecked((uint)int.MinValue);
            target.Registers[4] = minuend;
            target.Registers[5] = 234;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_0011));

            target.FIE();

            Assert.AreEqual(minuend - 234u, target.Registers[6]);
        }
               
        [TestMethod]
        public void SW()
        {
            const uint address = 0xF00BA;
            const uint value = 0xABCDEF01;
            target.Registers[4] = address;
            target.Registers[6] = value;
            PushInstruction(new FormatI(0b10_1011, 4, 6, 0));

            target.FIE();

            Assert.AreEqual(value, target.Memory.LoadWord(address));
        }

        [TestMethod]
        public void XOR()
        {
            target.Registers[4] = 0b1101_0011;
            target.Registers[5] = 0b1001_1010;
            const uint expected = 0b0100_1001;
            PushInstruction(new FormatR(4, 5, 6, 0, 0b10_0110));

            target.FIE();

            Assert.AreEqual(expected, target.Registers[6]);
        }

        [TestMethod]
        public void XORI()
        {
            target.Registers[4] = 0b1101_0011;
            const uint inputVal = 0b1001_1010;
            const uint expected = 0b0100_1001;
            PushInstruction(new FormatI(0b00_1110, 4, 6, inputVal));

            target.FIE();

            Assert.AreEqual(expected, target.Registers[6]);
        }
    }
}
