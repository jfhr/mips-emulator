namespace Mips.Emulator
{
    public interface IMemory
    {
        byte this[uint index] { get; set; }

        uint LoadWord(uint index);
        void StoreWord(uint index, uint word);
    }
}