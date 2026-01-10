namespace BlastPuzzle.Board
{
    /// <summary>
    /// Stores information about a detected group of same-colored blocks.
    /// </summary>
    [System.Serializable]
    public struct GroupInfo
    {
        /// <summary>
        /// Unique identifier for this group
        /// </summary>
        public ushort GroupId;

        /// <summary>
        /// Color index of blocks in this group
        /// </summary>
        public byte ColorIndex;

        /// <summary>
        /// Number of blocks in this group
        /// </summary>
        public ushort Size;

        /// <summary>
        /// Board index of the first block found in this group
        /// </summary>
        public int StartIndex;

        /// <summary>
        /// Check if this group is valid (has at least 2 blocks)
        /// </summary>
        public readonly bool IsValid => Size >= MinGroupSize;

        /// <summary>
        /// Minimum blocks required to form a blastable group
        /// </summary>
        public const int MinGroupSize = 2;
    }

    /// <summary>
    /// Data for block falling animation
    /// </summary>
    [System.Serializable]
    public struct FallData
    {
        /// <summary>
        /// Original board index
        /// </summary>
        public int FromIndex;

        /// <summary>
        /// Target board index after fall
        /// </summary>
        public int ToIndex;

        /// <summary>
        /// Number of rows to fall
        /// </summary>
        public int FallDistance;
    }

    /// <summary>
    /// Data for spawning new blocks
    /// </summary>
    [System.Serializable]
    public struct SpawnData
    {
        /// <summary>
        /// Target board index for spawned block
        /// </summary>
        public int TargetIndex;

        /// <summary>
        /// Color of the spawned block
        /// </summary>
        public byte ColorIndex;

        /// <summary>
        /// Spawn row position (negative = above board)
        /// </summary>
        public int SpawnRow;
    }
}
