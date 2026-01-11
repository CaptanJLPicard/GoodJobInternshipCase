// ============================================================================
// Made By Hakan Emre ÖZKAN
// For more follow my itch.io account (Heodev) - Begin to begin
// ============================================================================

using System;

namespace GoodJobInternshipCase.Board
{
    /// <summary>
    /// Spawns new blocks to fill empty cells at the top of columns.
    /// New blocks appear above the board and fall into position.
    /// </summary>
    public class SpawnProcessor
    {
        private Random _random;
        private int _rows;
        private int _columns;
        private int _colorCount;

        /// <summary>
        /// Initialize processor with board dimensions
        /// </summary>
        public void Initialize(int rows, int columns, int colorCount, int seed = -1)
        {
            _rows = rows;
            _columns = columns;
            _colorCount = Math.Clamp(colorCount, 1, BlockData.MaxColors);
            _random = seed >= 0 ? new Random(seed) : new Random();
        }

        /// <summary>
        /// Update color count (for dynamic difficulty)
        /// </summary>
        public void SetColorCount(int colorCount)
        {
            _colorCount = Math.Clamp(colorCount, 1, BlockData.MaxColors);
        }

        /// <summary>
        /// Spawn new blocks to fill empty cells at the top of the board.
        /// Returns the number of blocks spawned.
        /// </summary>
        public int SpawnNewBlocks(BlockData[] board, SpawnData[] output)
        {
            int spawnCount = 0;

            for (int col = 0; col < _columns; col++)
            {
                int emptyCount = 0;

                // Count empty cells from top
                for (int row = 0; row < _rows; row++)
                {
                    int index = row * _columns + col;
                    if (board[index].IsEmpty)
                        emptyCount++;
                    else
                        break;
                }

                // Spawn blocks for empty cells
                for (int i = 0; i < emptyCount; i++)
                {
                    int targetIndex = i * _columns + col;
                    byte color = (byte)_random.Next(0, _colorCount);

                    // Set block data
                    board[targetIndex] = BlockData.Create(color);

                    // Record spawn data for animation
                    output[spawnCount] = new SpawnData
                    {
                        TargetIndex = targetIndex,
                        ColorIndex = color,
                        SpawnRow = -(emptyCount - i) // Negative = above board
                    };
                    spawnCount++;
                }
            }

            return spawnCount;
        }

        /// <summary>
        /// Fill entire board with random blocks (for initialization)
        /// </summary>
        public void FillBoard(BlockData[] board)
        {
            int boardSize = _rows * _columns;

            for (int i = 0; i < boardSize; i++)
            {
                byte color = (byte)_random.Next(0, _colorCount);
                board[i] = BlockData.Create(color);
            }
        }

        /// <summary>
        /// Set random seed for reproducible spawns (useful for testing)
        /// </summary>
        public void SetSeed(int seed)
        {
            _random = new Random(seed);
        }
    }
}
