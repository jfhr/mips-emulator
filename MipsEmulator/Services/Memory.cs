using System;

namespace MipsEmulator.Services
{
    /// <summary>
    /// Represents the memory.
    /// </summary>
    /// <remarks>
    /// Memory is divided into 1 MiB pages. Only the first page 
    /// is available initially, more will be allocated automatically 
    /// on demand.
    /// All operations with words use big-endian byte order.
    /// </remarks>
    public class Memory
    {
        /// <summary>
        /// Size of a memory page in bytes (1 MiB).
        /// </summary>
        private const int pageSize = 1024 * 1024;

        /// <summary>
        /// Number of pages.
        /// </summary>
        private const int nPages = 1024 * 4;

        private byte[][] pages;

        /// <summary>
        /// Access a byte in memory by address.
        /// </summary>
        public byte this[uint index]
        {
            get
            {
                var (page, intIndex) = GetAndAllocPage(index);
                return pages[page][intIndex];
            }

            set
            {
                var (page, intIndex) = GetAndAllocPage(index);
                pages[page][intIndex] = value;
            }
        }

        /// <summary>
        /// Create a new memory instance.
        /// </summary>
        public Memory()
        {
            pages = new byte[nPages][];
        }

        /// <summary>
        /// Get the page index of an address, and allocate the page
        /// if it doesn't already exist.
        /// </summary>
        /// <param name="index">
        /// Absolute address of a byte in memory.
        /// </param>
        /// <returns>
        /// (page index, index in the page)
        /// </returns>
        private (int, int) GetAndAllocPage(uint index)
        {
            int page = (int)(index / pageSize);
            int intIndex = (int)(index % pageSize);
            if (pages[page] == null)
            {
                pages[page] = new byte[pageSize];
            }
            return (page, intIndex);
        }

        /// <summary>
        /// Get the page index of a word address, and allocate the page
        /// if it doesn't already exist.
        /// </summary>
        /// <remarks>
        /// If <paramref name="index"/> does not point to a word border,
        /// the next lower multiple of 4 will be used.
        /// </remarks>
        /// <param name="index">
        /// Absolute address of a word in memory.
        /// </param>
        /// <returns>
        /// (page index, index in the page)
        /// </returns>
        private (int, int) GetWordAndAllocPage(uint index)
        {
            var (page, intIndex) = GetAndAllocPage(index);
            intIndex -= intIndex % 4;
            return (page, intIndex);
        }

        /// <summary>
        /// Gets the 4 bytes representing a uint value as big-endian.
        /// Big-endian means the most significant byte is at index 0.
        /// </summary>
        private byte[] GetBytesBigEndian(uint value)
        {
            byte[] platformBytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                byte[] bigEndian = new byte[4];
                bigEndian[0] = platformBytes[3];
                bigEndian[1] = platformBytes[2];
                bigEndian[2] = platformBytes[1];
                bigEndian[3] = platformBytes[0];
                return bigEndian;
            }
            return platformBytes;
        }

        /// <summary>
        /// Converts 4 bytes to a uint in a big-endian way.
        /// Big-endian means the most significant byte is at index 0.
        /// </summary>
        private uint ToUint32BigEndian(byte[] value, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] bigEndian = new byte[4];
                bigEndian[0] = value[3 + startIndex];
                bigEndian[1] = value[2 + startIndex];
                bigEndian[2] = value[1 + startIndex];
                bigEndian[3] = value[0 + startIndex];
                return BitConverter.ToUInt32(bigEndian, 0);
            }
            return BitConverter.ToUInt32(value, startIndex);
        }

        /// <summary>
        /// Loads the 4-byte word at the specified index.
        /// Word addresses must be multiples of 4.
        /// If <paramref name="index"/> isn't one,
        /// the next lower multiple of 4 will be used.
        /// </summary>
        /// <remarks>
        /// Because words are 4 bytes long and start at an address
        /// divisible by 4, a word is always entirely inside one page.
        /// We do not have to account for the possiblity that one word
        /// might be distributed across two pages.
        /// </remarks>
        public uint LoadWord(uint index)
        {
            var (page, intIndex) = GetWordAndAllocPage(index);
            return ToUint32BigEndian(pages[page], intIndex);
        }


        /// <summary>
        /// Stores a 4-byte word at the specified index.
        /// Word addresses must be multiples of 4.
        /// If <paramref name="index"/> isn't one,
        /// the next lower multiple of 4 will be used.
        /// </summary>
        public void StoreWord(uint index, uint word)
        {
            var (page, intIndex) = GetWordAndAllocPage(index);
            var bytes = GetBytesBigEndian(word);
            for (int i = 0; i < 4; i++)
            {
                pages[page][intIndex + i] = bytes[i];
            }
        }
    }
}
