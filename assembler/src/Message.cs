namespace Mips.Assembler
{
    public class Message
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public bool IsError { get; set; }
        public string Content { get; set; }
    }
}
