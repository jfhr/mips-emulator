using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace Mips.Emulator.ComponentTest
{
    [TestClass]
    public class PerformanceTests
    {
        [TestMethod]
        public void TestRandomAddPerformance()
        {
            // Arrange
            const int iterations = 1_000_000;

            var rng = new Random();
            var cpu = new Cpu();
            var sw = new Stopwatch();
            uint index;

            for (index = 0; index < iterations * 4; index += 4)
            {
                uint value = (uint)rng.Next() & 0x0000_FFFF;
                // create addiu instruction with random value
                var ins = OperationEncoder.EncodeFormatI(0b001001, 8, 8, value);
                cpu.Memory.StoreWord(index, ins);
            }

            cpu.Memory.StoreWord(index, Cpu.TerminateInstruction);

            // Act
            sw.Start();
            cpu.CycleUntilTerminate();
            sw.Stop();

            // Announce results
            Console.WriteLine($"Ran {iterations:#,0} addiu instructions in {sw.ElapsedMilliseconds} ms");
        }
    }
}
