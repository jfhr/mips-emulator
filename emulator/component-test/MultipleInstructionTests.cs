using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mips.Emulator.ComponentTest
{
    /// <summary>
    /// The source code for the tested in programs is in the integration-tests project.
    /// </summary>
    [TestClass]
    public class MultipleInstructionTests
    {
        private static readonly Random random = new Random();
        private Cpu target;

        [TestInitialize]
        public void Initialize()
        {
            target = new Cpu();
        }

        /// <summary>
        /// Stores the program in memory starting at adress 0,
        /// and stores a break instruction after it.
        /// </summary>
        private void LoadProgram(uint[] program)
        {
            uint i;
            for (i = 0; i < program.Length; i++)
            {
                target.Memory.StoreWord(i * 4, program[i]);
            }
            target.Memory.StoreWord(i * 4, Cpu.TerminateInstruction);
        }

        /// <summary>
        /// Sets all registers to pseudo-random values.
        /// </summary>
        private void RandomizeRegisters()
        {
            for (int i = 0; i < 32; i++)
            {
                int randomValue = random.Next(int.MinValue, int.MaxValue);
                target.Registers[i] = (uint)randomValue;
            }
        }

        /// <summary>
        /// Loads up to 4 values into the $a0 - $a3 registers.
        /// </summary>
        private void LoadArguments(uint? a0 = null, uint? a1 = null, uint? a2 = null, uint? a3 = null)
        {
            if (a0 != null) target.Registers.A0 = a0.Value;
            if (a1 != null) target.Registers.A1 = a1.Value;
            if (a2 != null) target.Registers.A2 = a2.Value;
            if (a3 != null) target.Registers.A3 = a3.Value;
        }

        private static readonly uint[] primes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97 };
        public static IEnumerable<object[]> NaivePrimalityTestData()
        {
            for (uint i = 10; i < 100; i++)
            {
                yield return new object[] { i, primes.Contains(i) };
            }
        }

        [TestMethod, DynamicData(nameof(NaivePrimalityTestData), DynamicDataSourceType.Method)]
        public void NaivePrimalityTest(uint value, bool isValuePrime)
        {
            LoadProgram(new uint[]
            {
                0x00044042,
                0x1100000c,
                0x24090002,
                0x1089000c,
                0x308a0001,
                0x11400008,
                0x240a0003,
                0x108a0008,
                0x11090007,
                0x21290001,
                0x0089001a,
                0x00005010,
                0x11400001,
                0x0401fffa,
                0x24020000,
                0x04010002,
                0x24020001,
                0x04010000,
            });
            RandomizeRegisters();
            LoadArguments(value);

            target.CycleUntilTerminate();

            uint expected = isValuePrime ? 1u : 0u;
            Assert.AreEqual(expected, target.Registers.V0);
        }



        public static IEnumerable<object[]> StringLengthTestData => new object[][]
        {
            new object[] { "" },
            new object[] { "." },
            new object[] { "Hello World" },
            new object[] { "3f80bf9f-f59f-4e47-87e7-3c32247477eb" },
            new object[] { "3f80bf9f\nf59f\n" },
        };

        [TestMethod, DynamicData(nameof(StringLengthTestData), DynamicDataSourceType.Property)]
        public void StringLengthTest(string data)
        {
            const uint offset = 0x0F00;
            LoadProgram(new uint[]
            {
                0x00044021,
                0x240a0000,
                0x81090000,
                0x11200003,
                0x21080001,
                0x214a0001,
                0x0401fffb,
                0x000a1021,
            });
            RandomizeRegisters();
            LoadArguments(offset);
            byte[] dataAscii = Encoding.ASCII.GetBytes(data);
            for (uint i = 0; i < dataAscii.Length; i++)
            {
                target.Memory[i + offset] = dataAscii[i];
            }

            target.CycleUntilTerminate();

            Assert.AreEqual((uint)dataAscii.Length, target.Registers.V0);
        }
    }
}
