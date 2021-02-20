namespace Mips.Assembler
{
    public interface IParameterQueue
    {
        void AddSigned(int value);
        void AddUnsigned(uint value);
        void Clear();
        bool TryGetSigned(out int value);
        bool TryGetUnsigned(out uint value);
    }
}