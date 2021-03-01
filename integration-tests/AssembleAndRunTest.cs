using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mips.Assembler;
using Mips.Emulator;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mips.IntegrationTest
{
    [TestClass]
    public class AssembleAndRunTest
    {
        public static IEnumerable<object[]> PrimeTestData => new[]
        {
            new object[] {0u, false},
            new object[] {1u, false},
            new object[] {2u, true},
            new object[] {3u, true},
            new object[] {4u, false},
            new object[] {127u, true},
            new object[] {128u, false},
        };

        /// <summary>
        /// Scenario: naive primality test.
        /// If the unsigned number in $a0 is prime, set $v0 to 1.
        /// Otherwise, set $v0 to 0.
        /// </summary>
        [TestMethod]
        [DynamicData(nameof(PrimeTestData))]
        public void NaivePrimalityTest(uint number, bool isPrime)
        {
            var cpu = new Cpu();
            string code = File.ReadAllText("NaivePrimalityTest.asm");

            var assemblyResult = MipsAsm.Assemble(code, cpu.Memory);
            AssertNoErrors(assemblyResult, code);
            cpu.Registers.A0 = number;
            cpu.CycleUntilTerminate();

            if (isPrime)
            {
                Assert.AreEqual(1u, cpu.Registers.V0);
            }
            else
            {
                Assert.AreEqual(0u, cpu.Registers.V0);
            }
        }

        private void AssertNoErrors(IAssemblerResult assemblyResult, string code)
        {
            var errors = assemblyResult.Messages.Where(x => x.IsError);
            if (errors.Any())
            {
                var message = string.Join(", ", errors.Select(x =>
                {
                    var affectedCode = code[x.StartIndex..x.EndIndex];
                    return $"{x.Content} at {affectedCode}";
                }));
                Assert.Fail(message);
            }
        }
    }
}
