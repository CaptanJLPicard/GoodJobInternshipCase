// ============================================================================
// Made By Hakan Emre ÖZKAN
// For more follow my itch.io account (Heodev) - To begin, begin
// ============================================================================

namespace GoodJobInternshipCase.Board
{
    /// <summary>
    /// Detects deadlock situations where no valid moves exist.
    /// Uses early-exit optimization for fast detection.
    /// </summary>
    public class DeadlockDetector
    {
        private int _rows;
        private int _columns;
        private int _boardSize;

        /// <summary>
        /// Initialize detector with board dimensions
        /// </summary>
        public void Initialize(int rows, int columns)
        {
            _rows = rows;
            _columns = columns;
            _boardSize = rows * columns;
        }

        /// <summary>
        /// Check if any valid moves exist on the board.
        /// Returns true if at least one valid group (2+ adjacent same-color blocks) exists.
        /// Uses early-exit for performance.
        /// </summary>
        public bool HasValidMoves(BlockData[] board)
        {
            // A valid move exists if any two adjacent blocks share the same color
            // We only need to find ONE such pair to confirm no deadlock

            for (int i = 0; i < _boardSize; i++)
            {
                if (board[i].IsEmpty)
                    continue;

                byte color = board[i].ColorIndex;
                int row = i / _columns;
                int col = i % _columns;

                // Check RIGHT neighbor (avoid checking left to prevent duplicate checks)
                if (col < _columns - 1)
                {
                    int rightIndex = i + 1;
                    if (board[rightIndex].ColorIndex == color)
                        return true; // Found valid group!
                }

                // Check BOTTOM neighbor (avoid checking up to prevent duplicate checks)
                if (row < _rows - 1)
                {
                    int bottomIndex = i + _columns;
                    if (board[bottomIndex].ColorIndex == color)
                        return true; // Found valid group!
                }
            }

            return false; // Deadlock!
        }

        /// <summary>
        /// Count total number of valid groups on the board.
        /// More expensive than HasValidMoves, use sparingly.
        /// </summary>
        public int CountValidGroups(BlockData[] board, GroupInfo[] groups, int groupCount)
        {
            int validCount = 0;
            for (int i = 0; i < groupCount; i++)
            {
                if (groups[i].Size >= GroupInfo.MinGroupSize)
                {
                    validCount++;
                }
            }
            return validCount;
        }
    }
}
