namespace BlastPuzzle.Board
{
    /// <summary>
    /// Processes block falling/gravity after blocks are destroyed.
    /// Blocks above empty cells fall down to fill gaps.
    /// </summary>
    public class FallProcessor
    {
        private int _rows;
        private int _columns;
        private int _boardSize;

        /// <summary>
        /// Initialize processor with board dimensions
        /// </summary>
        public void Initialize(int rows, int columns)
        {
            _rows = rows;
            _columns = columns;
            _boardSize = rows * columns;
        }

        /// <summary>
        /// Process all falling blocks after destruction.
        /// Returns the number of blocks that will fall.
        /// </summary>
        public int ProcessFalls(BlockData[] board, FallData[] output)
        {
            int fallCount = 0;

            // Process column by column (cache-friendly for vertical operations)
            for (int col = 0; col < _columns; col++)
            {
                int writeRow = _rows - 1; // Start from bottom

                // Scan from bottom to top
                for (int row = _rows - 1; row >= 0; row--)
                {
                    int index = row * _columns + col;

                    if (!board[index].IsEmpty)
                    {
                        int targetIndex = writeRow * _columns + col;

                        if (index != targetIndex)
                        {
                            // Record fall data for animation
                            output[fallCount] = new FallData
                            {
                                FromIndex = index,
                                ToIndex = targetIndex,
                                FallDistance = writeRow - row
                            };
                            fallCount++;

                            // Move data in board array
                            board[targetIndex] = board[index];
                            board[index] = BlockData.CreateEmpty();
                        }

                        writeRow--;
                    }
                }
            }

            return fallCount;
        }

        /// <summary>
        /// Count empty cells at the top of each column (for spawn calculation)
        /// </summary>
        public int CountEmptyCellsPerColumn(BlockData[] board, int[] outputPerColumn)
        {
            int totalEmpty = 0;

            for (int col = 0; col < _columns; col++)
            {
                int emptyCount = 0;

                // Count from top until we hit a non-empty block
                for (int row = 0; row < _rows; row++)
                {
                    int index = row * _columns + col;
                    if (board[index].IsEmpty)
                        emptyCount++;
                    else
                        break;
                }

                outputPerColumn[col] = emptyCount;
                totalEmpty += emptyCount;
            }

            return totalEmpty;
        }
    }
}
