using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mips.Services;
using System.Collections.Generic;

namespace Mips.UnitTest
{
    [TestClass]
    public class OperationCodeTest
    {
        public static IEnumerable<object[]> TestData => new object[][] {
            new object[] { 0b0000_0000_0000_0000_0000_0000_0000_0000u, new FormatR(0, 0, 0, 0, 0u) },
            new object[] { 0b0000_0000_0000_0000_0000_0000_0001_1010u, new FormatR(0, 0, 0, 0, 0b1_1010u) },
            new object[] { 0b0000_0000_0000_0000_0000_0000_0011_1010u, new FormatR(0, 0, 0, 0, 0b11_1010u) },
            new object[] { 0b0000_0000_0000_0000_0000_0000_0111_1010u, new FormatR(0, 0, 0, 1, 0b11_1010u) },
            new object[] { 0b0000_0010_0001_0000_1000_0100_0010_0000u, new FormatR(0b10000, 0b10000, 0b10000, 0b10000, 0b10_0000u) },
            new object[] { 0b0010_0010_1010_1010_1000_0000_0000_0000u, new FormatI(0b0010_00, 0b10101, 0b01010, 0x8000)}
        };

        [TestMethod, DynamicData(nameof(TestData))]
        public void OperationEncodeDecode(uint word, IInstructionFormat instruction)
        {
            var decoder = new OperationDecoder();
            var encoder = new OperationEncoder();

            // test decoder
            IInstructionFormat actualInstruction = decoder.DecodeInstruction(word);
            Assert.AreEqual(instruction, actualInstruction, "Decoding failed");

            // test encoder
            uint actualWord = encoder.EncodeInstruction(instruction);
            Assert.AreEqual(word, actualWord, "Encoding failed");
        }
    }
}
