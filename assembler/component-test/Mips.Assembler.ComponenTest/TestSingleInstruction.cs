using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mips.Assembler.Mnemonics;
using Mips.Assembler.Services;
using Mips.Emulator;

namespace Mips.Assembler.ComponentTest
{
    [TestClass]
    public class TestSingleInstruction
    {
        private IMemory memory;
        private IErrorMessageHelper errorMessageHelper;
        private IParameterQueue parameterQueue;
        private IBinaryCodeWriter binaryCodeWriter;
        private ILabelRegistry labelRegistry;

        private Whitespace whitespace;
        private Comma comma;
        private Register register;

        [TestInitialize]
        public void Initialize()
        {
            memory = new Memory();
            errorMessageHelper = new ErrorMessageHelper();
            parameterQueue = new ParameterQueue();
            binaryCodeWriter = new BinaryCodeWriter(memory);
            labelRegistry = new LabelRegistry(errorMessageHelper, binaryCodeWriter);

            whitespace = new Whitespace();
            comma = new Comma(whitespace);
            register = new Register(parameterQueue);
        }

        [TestMethod]
        public void TestUnknownInstruction()
        {
            var target = new ArithLogInstruction("add", 0b100000, parameterQueue, labelRegistry, binaryCodeWriter, whitespace, comma, register);

            int index = target.TryRead("foo $1,$1,$1", 0);
            Assert.AreEqual(0, index);
            uint ins = memory.LoadWord(0);
            Assert.AreEqual(0u, ins);
        }

        [TestMethod]
        public void TestAdd()
        {
            var target = new ArithLogInstruction("add", 0b100000, parameterQueue, labelRegistry, binaryCodeWriter, whitespace, comma, register);

            int index = target.TryRead("add $1,$1,$1", 0);
            Assert.AreEqual(12, index);
            uint ins = memory.LoadWord(0);
            Assert.AreEqual(0b00000000_00100001_00001000_00100000u, ins);
        }

        [TestMethod]
        public void TestAdd_Illegal()
        {
            var target = new ArithLogInstruction("add", 0b100000, parameterQueue, labelRegistry, binaryCodeWriter, whitespace, comma, register);

            int index = target.TryRead("add $1,$1", 0);
            Assert.AreEqual(0, index);
            uint ins = memory.LoadWord(0);
            Assert.AreEqual(0u, ins);
        }

        [TestMethod]
        public void TestMult()
        {
            var target = new DivMultInstruction("mult", 0b011000, parameterQueue, labelRegistry, binaryCodeWriter, whitespace, comma, register);

            int index = target.TryRead("mult $1,$1", 0);
            Assert.AreEqual(10, index);
            uint ins = memory.LoadWord(0);
            Assert.AreEqual(0b00000000_00100001_00000000_00011000u, ins);
        }

        [TestMethod]
        public void TestMult_Illegal()
        {
            var target = new DivMultInstruction("mult", 0b011000, parameterQueue, labelRegistry, binaryCodeWriter, whitespace, comma, register);

            int index = target.TryRead("mult $1,1", 0);
            Assert.AreEqual(0, index);
            uint ins = memory.LoadWord(0);
            Assert.AreEqual(0u, ins);
        }

        [TestMethod]
        public void TestJr()
        {
            var target = new RJumpOrMoveInstruction("jr", 0b001000, parameterQueue, labelRegistry, binaryCodeWriter, whitespace, comma, register);

            int index = target.TryRead("jr $1", 0);
            Assert.AreEqual(5, index);
            uint ins = memory.LoadWord(0);
            Assert.AreEqual(0b00000000_00100000_00000000_00001000u, ins);
        }

        [TestMethod]
        public void TestJr_Illegal()
        {
            var target = new RJumpOrMoveInstruction("jr", 0b001000, parameterQueue, labelRegistry, binaryCodeWriter, whitespace, comma, register);

            int index = target.TryRead("jr 100", 0);
            Assert.AreEqual(0, index);
            uint ins = memory.LoadWord(0);
            Assert.AreEqual(0u, ins);
        }
    }
}
