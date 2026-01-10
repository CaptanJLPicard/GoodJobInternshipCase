namespace GoodJobInternshipCase.Board
{
    /// <summary>
    /// Lightweight struct representing a single block on the board.
    /// Uses value types to avoid GC allocations.
    /// </summary>
    [System.Serializable]
    public struct BlockData
    {
        /// <summary>
        /// Color index (0-5 for 6 colors, 255 = empty cell)
        /// </summary>
        public byte ColorIndex;

        /// <summary>
        /// Icon state based on group size (0=Default, 1=A, 2=B, 3=C)
        /// </summary>
        public byte IconState;

        /// <summary>
        /// Group ID for group detection (0 = no group/single block)
        /// </summary>
        public ushort GroupId;

        /// <summary>
        /// Check if this cell is empty
        /// </summary>
        public readonly bool IsEmpty => ColorIndex == EmptyCell;

        /// <summary>
        /// Check if this is a valid colored block
        /// </summary>
        public readonly bool IsValid => ColorIndex < MaxColors;

        /// <summary>
        /// Constant for empty cell marker
        /// </summary>
        public const byte EmptyCell = 255;

        /// <summary>
        /// Maximum number of colors supported
        /// </summary>
        public const byte MaxColors = 6;

        /// <summary>
        /// Create a new block with specified color
        /// </summary>
        public static BlockData Create(byte colorIndex)
        {
            return new BlockData
            {
                ColorIndex = colorIndex,
                IconState = 0,
                GroupId = 0
            };
        }

        /// <summary>
        /// Create an empty cell
        /// </summary>
        public static BlockData CreateEmpty()
        {
            return new BlockData
            {
                ColorIndex = EmptyCell,
                IconState = 0,
                GroupId = 0
            };
        }

        /// <summary>
        /// Reset this block to empty state
        /// </summary>
        public void Clear()
        {
            ColorIndex = EmptyCell;
            IconState = 0;
            GroupId = 0;
        }
    }
}
