using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;

namespace Mips.Assembler.UnitTest
{
    [TestClass]
    public class DisassemblerInstanceTest
    {
        public static IEnumerable<object[]> SingleInstructionTestData => new[]
        {
            new object[] {0b000000_00000_00000_00000_00000_000001u, null},
            new object[] {0b000000_00101_00110_00100_00000_100000u, "add $a0,$a1,$a2"},
            new object[] {0b000000_00101_00110_01000_00000_100000u, "add $t0,$a1,$a2"},
            //new object[] {0u, "sll $0,$0,0"},
        };

        [TestMethod]
        [DynamicData(nameof(SingleInstructionTestData))]
        public void DisassembleSingleInstruction(uint word, string expectedCode)
        {
            var binary = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(binary, word);
            var instance = new DisassemblerInstance(binary);
            var result = instance.TryReadInstruction();

            if (expectedCode != null)
            {
                var actualCode = instance.Code.TrimEnd();
                Assert.IsTrue(result);
                Assert.AreEqual(expectedCode, actualCode);
                Assert.AreEqual(0, instance.Errors.Count());
            }
            else
            {
                Assert.IsFalse(result);
            }
        }
    }
}
