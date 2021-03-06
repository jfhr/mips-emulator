namespace Mips.Assembler
{
    public record Message(int StartIndex, int EndIndex, bool IsError, string Content);
}
