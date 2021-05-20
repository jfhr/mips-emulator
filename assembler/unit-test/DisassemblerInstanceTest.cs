using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mips.Emulator;
using System;
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

            new object[] {OperationEncoder.EncodeFormatR(5, 6, 4, 0, Functions.Add), "add $a0,$a1,$a2"},
            new object[] {OperationEncoder.EncodeFormatR(5, 6, 8, 0, Functions.Add), "add $t0,$a1,$a2"},
            new object[] {OperationEncoder.EncodeFormatR(5, 6, 8, 0, Functions.Addu), "addu $t0,$a1,$a2"},
            new object[] {OperationEncoder.EncodeFormatR(5, 6, 8, 0, Functions.Sub), "sub $t0,$a1,$a2"},
            new object[] {OperationEncoder.EncodeFormatR(5, 6, 8, 0, Functions.Subu), "subu $t0,$a1,$a2"},
            new object[] {OperationEncoder.EncodeFormatR(5, 6, 8, 0, Functions.And), "and $t0,$a1,$a2"},
            new object[] {OperationEncoder.EncodeFormatR(5, 6, 8, 0, Functions.Or), "or $t0,$a1,$a2"},
            new object[] {OperationEncoder.EncodeFormatR(5, 6, 8, 0, Functions.Xor), "xor $t0,$a1,$a2"},
            new object[] {OperationEncoder.EncodeFormatR(5, 6, 8, 0, Functions.Nor), "nor $t0,$a1,$a2"},
            new object[] {OperationEncoder.EncodeFormatR(5, 6, 8, 0, Functions.Slt), "slt $t0,$a1,$a2"},
            new object[] {OperationEncoder.EncodeFormatR(5, 6, 8, 0, Functions.Sltu), "sltu $t0,$a1,$a2"},

            new object[] {OperationEncoder.EncodeFormatR(9, 10, 0, 0, Functions.Div), "div $t1,$t2"},
            new object[] {OperationEncoder.EncodeFormatR(9, 10, 0, 0, Functions.Divu), "divu $t1,$t2"},
            new object[] {OperationEncoder.EncodeFormatR(9, 10, 0, 0, Functions.Mult), "mult $t1,$t2"},
            new object[] {OperationEncoder.EncodeFormatR(9, 10, 0, 0, Functions.Multu), "multu $t1,$t2"},

            new object[] {OperationEncoder.EncodeFormatR(0, 10, 9, 4, Functions.Sll), "sll $t1,$t2,4"},
            new object[] {OperationEncoder.EncodeFormatR(0, 10, 9, 4, Functions.Sra), "sra $t1,$t2,4"},
            new object[] {OperationEncoder.EncodeFormatR(0, 10, 9, 4, Functions.Srl), "srl $t1,$t2,4"},

            new object[] {OperationEncoder.EncodeFormatR(11, 10, 9, 0, Functions.Sllv), "sllv $t1,$t2,$t3"},
            new object[] {OperationEncoder.EncodeFormatR(11, 10, 9, 0, Functions.Srav), "srav $t1,$t2,$t3"},
            new object[] {OperationEncoder.EncodeFormatR(11, 10, 9, 0, Functions.Srlv), "srlv $t1,$t2,$t3"},

            new object[] {OperationEncoder.EncodeFormatR(9, 0, 0, 0, Functions.Jr), "jr $t1"},
            new object[] {OperationEncoder.EncodeFormatR(9, 0, 0, 0, Functions.Jalr), "jalr $t1"},
            new object[] {OperationEncoder.EncodeFormatR(9, 0, 0, 0, Functions.Mfhi), "mfhi $t1"},
            new object[] {OperationEncoder.EncodeFormatR(9, 0, 0, 0, Functions.Mflo), "mflo $t1"},

            new object[] {OperationEncoder.EncodeFormatI(Opcodes.Addi, 10, 9, 123), "addi $t1,$t2,123"},
            new object[] {OperationEncoder.EncodeFormatI(Opcodes.Addiu, 10, 9, 123), "addiu $t1,$t2,123"},
            new object[] {OperationEncoder.EncodeFormatI(Opcodes.Andi, 10, 9, 123), "andi $t1,$t2,123"},
            new object[] {OperationEncoder.EncodeFormatI(Opcodes.Ori, 10, 9, 123), "ori $t1,$t2,123"},
            new object[] {OperationEncoder.EncodeFormatI(Opcodes.Xori, 10, 9, 123), "xori $t1,$t2,123"},
            new object[] {OperationEncoder.EncodeFormatI(Opcodes.Slti, 10, 9, 123), "slti $t1,$t2,123"},
            new object[] {OperationEncoder.EncodeFormatI(Opcodes.Sltiu, 10, 9, 123), "sltiu $t1,$t2,123"},

            new object[] {OperationEncoder.EncodeFormatI(Opcodes.Beq, 9, 10, 123), "beq $t1,$t2,l0"},
            new object[] {OperationEncoder.EncodeFormatI(Opcodes.Bne, 9, 10, 123), "bne $t1,$t2,l0"},

            new object[] {OperationEncoder.EncodeFormatI(Opcodes.Bgtz, 9, 0, 123), "bgtz $t1,l0"},
            new object[] {OperationEncoder.EncodeFormatI(Opcodes.Blez, 9, 0, 123), "blez $t1,l0"},

            new object[] {OperationEncoder.EncodeFormatI(Opcodes.Lb, 10, 9, 123), "lb $t1,123($t2)"},
            new object[] {OperationEncoder.EncodeFormatI(Opcodes.Lbu, 10, 9, 123), "lbu $t1,123($t2)"},
            new object[] {OperationEncoder.EncodeFormatI(Opcodes.Lh, 10, 9, 123), "lh $t1,123($t2)"},
            new object[] {OperationEncoder.EncodeFormatI(Opcodes.Lhu, 10, 9, 123), "lhu $t1,123($t2)"},
            new object[] {OperationEncoder.EncodeFormatI(Opcodes.Lw, 10, 9, 123), "lw $t1,123($t2)"},
            new object[] {OperationEncoder.EncodeFormatI(Opcodes.Sb, 10, 9, 123), "sb $t1,123($t2)"},
            new object[] {OperationEncoder.EncodeFormatI(Opcodes.Sh, 10, 9, 123), "sh $t1,123($t2)"},
            new object[] {OperationEncoder.EncodeFormatI(Opcodes.Sw, 10, 9, 123), "sw $t1,123($t2)"},

            new object[] {OperationEncoder.EncodeFormatJ(123, false), "j l0"},
            new object[] {OperationEncoder.EncodeFormatJ(123, true), "jal l0"},
        };

        [TestMethod]
        [DynamicData(nameof(SingleInstructionTestData))]
        public void DisassembleSingleInstruction(uint word, string expectedCode)
        {
            var binary = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(binary, word);
            var instance = new DisassemblerInstance(binary);
            var result = instance.TryReadInstruction(out string actualCode);

            if (expectedCode != null)
            {
                Assert.IsTrue(result);
                Assert.AreEqual(expectedCode, actualCode);
                Assert.AreEqual(0, instance.Errors.Count());
            }
            else
            {
                Assert.IsFalse(result);
            }
        }

        public static IEnumerable<object[]> ProgramTestData => new[]
        {
            new object[] {
                new uint[]
                {
                    0x00044021,
                    0x240a0000,
                    0x81090000,
                    0x11200003,
                    0x21080001,
                    0x214a0001,
                    0x0401fffb,
                    0x000a1021,
                }
            }
        };

        [TestMethod]
        [DynamicData(nameof(ProgramTestData))]
        public void DisassembleAndReassemble(uint[] inputWords)
        {
            var input = new byte[inputWords.Length * 4];

            // Don't use Buffer.BlockCopy here bc it would mess up endianness
            for (int i = 0; i < inputWords.Length; i++)
            {
                BinaryPrimitives.WriteUInt32BigEndian(input.AsSpan()[(i * 4)..], inputWords[i]);
            }

            var disassembler = new DisassemblerInstance(input);
            disassembler.Disassemble();

            // for debugging
            Console.WriteLine(disassembler.Code);

            var memory = new Memory();
            var assembler = new AssemblerInstance(disassembler.Code, memory);
            assembler.Assemble();

            // not a very nice way to get the assembly output, but works for now
            var asmOutput = new byte[inputWords.Length];
            for (uint i = 0; i < asmOutput.Length; i++)
            {
                asmOutput[i] = memory[i];
            }

            CollectionAssert.AreEqual(input, asmOutput);
        }
    }
}
