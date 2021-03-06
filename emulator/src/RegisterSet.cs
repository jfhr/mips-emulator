namespace Mips.Emulator
{
    public class RegisterSet
    {
        private readonly uint[] reg;

        public uint this[int index]
        {
            get => reg[index];
            set
            {
                // no write to zero
                if (index != 0)
                {
                    reg[index] = value;
                }
            }
        }

        public RegisterSet()
        {
            reg = new uint[34];
        }


        /// <summary>
        /// Zero register. Read always returns 0.
        /// </summary>
        public uint Zero { get => this[0]; set => this[0] = value; }

        /// <summary>
        /// [1] Assembly temporary register. Do not use when writing assembly code.
        /// </summary>
        public uint At { get => this[1]; set => this[1] = value; }

        /// <summary> [2] Return value. </summary>
        public uint V0 { get => this[2]; set => this[2] = value; }
        /// <summary> [3] Return value. </summary>
        public uint V1 { get => this[3]; set => this[3] = value; }

        /// <summary> [4] Subroutine argument. </summary>
        public uint A0 { get => this[4]; set => this[4] = value; }
        /// <summary> [5] Subroutine argument. </summary>
        public uint A1 { get => this[5]; set => this[5] = value; }
        /// <summary> [6] Subroutine argument. </summary>
        public uint A2 { get => this[6]; set => this[6] = value; }
        /// <summary> [7] Subroutine argument. </summary>
        public uint A3 { get => this[7]; set => this[7] = value; }

        /// <summary> [8] Temporary value, may be changed by subroutine. </summary>
        public uint T0 { get => this[8]; set => this[8] = value; }
        /// <summary> [9] Temporary value, may be changed by subroutine. </summary>
        public uint T1 { get => this[9]; set => this[9] = value; }
        /// <summary> [10] Temporary value, may be changed by subroutine. </summary>
        public uint T2 { get => this[10]; set => this[10] = value; }
        /// <summary> [11] Temporary value, may be changed by subroutine. </summary>
        public uint T3 { get => this[11]; set => this[11] = value; }
        /// <summary> [12] Temporary value, may be changed by subroutine. </summary>
        public uint T4 { get => this[12]; set => this[12] = value; }
        /// <summary> [13] Temporary value, may be changed by subroutine. </summary>
        public uint T5 { get => this[13]; set => this[13] = value; }
        /// <summary> [14] Temporary value, may be changed by subroutine. </summary>
        public uint T6 { get => this[14]; set => this[14] = value; }
        /// <summary> [15] Temporary value, may be changed by subroutine. </summary>
        public uint T7 { get => this[15]; set => this[15] = value; }

        /// <summary> [16] Saved values, should be preserved by subroutine. </summary>
        public uint S0 { get => this[16]; set => this[16] = value; }
        /// <summary> [17] Saved values, should be preserved by subroutine. </summary>
        public uint S1 { get => this[17]; set => this[17] = value; }
        /// <summary> [18] Saved values, should be preserved by subroutine. </summary>
        public uint S2 { get => this[18]; set => this[18] = value; }
        /// <summary> [19] Saved values, should be preserved by subroutine. </summary>
        public uint S3 { get => this[19]; set => this[19] = value; }
        /// <summary> [20] Saved values, should be preserved by subroutine. </summary>
        public uint S4 { get => this[20]; set => this[20] = value; }
        /// <summary> [21] Saved values, should be preserved by subroutine. </summary>
        public uint S5 { get => this[21]; set => this[21] = value; }
        /// <summary> [22] Saved values, should be preserved by subroutine. </summary>
        public uint S6 { get => this[22]; set => this[22] = value; }
        /// <summary> [23] Saved values, should be preserved by subroutine. </summary>
        public uint S7 { get => this[23]; set => this[23] = value; }


        /// <summary> [24] Temporary value, may be changed by subroutine. </summary>
        public uint T8 { get => this[24]; set => this[24] = value; }
        /// <summary> [25] Temporary value, may be changed by subroutine. </summary>
        public uint T9 { get => this[25]; set => this[25] = value; }

        /// <summary> [26] Reserved for special events. </summary>
        public uint K0 { get => this[26]; set => this[26] = value; }
        /// <summary> [27] Reserved for special events. </summary>
        public uint K1 { get => this[27]; set => this[27] = value; }

        /// <summary> [28] Global pointer. </summary>
        public uint Gp { get => this[28]; set => this[28] = value; }
        /// <summary> [29] Stack pointer. </summary>
        public uint Sp { get => this[29]; set => this[29] = value; }
        /// <summary> [30] Frame pointer. </summary>
        public uint Fp { get => this[30]; set => this[30] = value; }

        /// <summary> [31] Return address. </summary>
        public uint Ra { get => this[31]; set => this[31] = value; }


        /// <summary> 
        /// After multiplication, holds the lower 32 bit of the result. 
        /// After division, holds the integer quotient.
        /// </summary>
        public uint Lo { get => this[32]; set => this[32] = value; }

        /// <summary> 
        /// After multiplication, holds the upper 32 bit of the result. 
        /// After division, holds the remainder.
        /// </summary>
        public uint Hi { get => this[33]; set => this[33] = value; }
    }
}
