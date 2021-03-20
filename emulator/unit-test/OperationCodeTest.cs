using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Mips.Emulator.UnitTest
{
    [TestClass]
    public class OperationCodeTest
    {
        public static IEnumerable<object[]> FormatRTestData => new[]
        {
            new object[] { 0b0000_0000_0000_0000_0000_0000_0000_0000u, 0, 0, 0, 0, 0u },
            new object[] { 0b0000_0000_0000_0000_0000_0000_0001_1010u, 0, 0, 0, 0, 0b1_1010u },
            new object[] { 0b0000_0000_0000_0000_0000_0000_0011_1010u, 0, 0, 0, 0, 0b11_1010u },
            new object[] { 0b0000_0000_0000_0000_0000_0000_0111_1010u, 0, 0, 0, 1, 0b11_1010u },
            new object[] { 0b0000_0010_0001_0000_1000_0100_0010_0000u, 0b10000, 0b10000, 0b10000, 0b10000, 0b10_0000u },
        };

        [TestMethod, DynamicData(nameof(FormatRTestData))]
        public void TestEncodeFormatR(uint word, int rs, int rt, int rd, int shamt, uint function)
        {
            uint actualWord = OperationEncoder.EncodeFormatR(rs, rt, rd, shamt, function);
            Assert.AreEqual(word, actualWord);
        }

        [TestMethod, DynamicData(nameof(FormatRTestData))]
        public void TestDecodeFormatR(uint word, int rs, int rt, int rd, int shamt, uint function)
        {
            OperationDecoder.DecodeFormatR(word, out int actualRs, out int actualRt, out int actualRd, out int actualShamt, out uint actualFunction);
            Assert.AreEqual(rs, actualRs);
            Assert.AreEqual(rt, actualRt);
            Assert.AreEqual(rd, actualRd);
            Assert.AreEqual(shamt, actualShamt);
            Assert.AreEqual(function, actualFunction);
        }


        public static IEnumerable<object[]> FormatITestData => new[]
        {
            new object[] { 0b0010_0010_1010_1010_1000_0000_0000_0000u, 0b0010_00u, 0b10101, 0b01010, 0x8000u},
        };

        [TestMethod, DynamicData(nameof(FormatITestData))]
        public void TestEncodeFormatI(uint word, uint opcode, int rs, int rt, uint value)
        {
            uint actualWord = OperationEncoder.EncodeFormatI(opcode, rs, rt, value);
            Assert.AreEqual(word, actualWord);
        }

        [TestMethod, DynamicData(nameof(FormatITestData))]
        public void TestDecodeFormatI(uint word, uint opcode, int rs, int rt, uint value)
        {
            OperationDecoder.DecodeFormatI(word, out uint actualOpcode, out int actualRs, out int actualRt, out uint actualValue);
            Assert.AreEqual(opcode, actualOpcode);
            Assert.AreEqual(rs, actualRs);
            Assert.AreEqual(rt, actualRt);
            Assert.AreEqual(value, actualValue);
        }


        public static IEnumerable<object[]> FormatJTestData => new[]
        {
            new object[] { 0x08DEADBEu, 0xDEADBEu, false},
            new object[] { 0x0CDEADBEu, 0xDEADBEu, true},
        };

        [TestMethod, DynamicData(nameof(FormatJTestData))]
        public void TestEncodeFormatJ(uint word, uint address, bool link)
        {
            uint actualWord = OperationEncoder.EncodeFormatJ(address, link);
            Assert.AreEqual(word, actualWord);
        }

        [TestMethod, DynamicData(nameof(FormatJTestData))]
        public void TestDecodeFormatJ(uint word, uint address, bool link)
        {
            OperationDecoder.DecodeFormatJ(word, out uint actualAddress, out bool actualLink);
            Assert.AreEqual(address, actualAddress);
            Assert.AreEqual(link, actualLink);
        }
    }
}
