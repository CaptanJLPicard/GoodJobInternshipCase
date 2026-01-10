using System;

namespace BlastPuzzle.Board
{
    /// <summary>
    /// Detects groups of same-colored adjacent blocks using flood fill algorithm.
    /// Uses pre-allocated arrays to avoid GC allocations during gameplay.
    /// </summary>
    public class GroupDetector
    {
        private int[] _visitStack;
        private bool[] _visited;
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

            _visitStack = new int[_boardSize];
            _visited = new bool[_boardSize];
        }

        /// <summary>
        /// Detect all groups on the board and update block GroupIds.
        /// Returns the number of valid groups found.
        /// </summary>
        public int DetectAllGroups(BlockData[] board, GroupInfo[] groups)
        {
            // Reset visited flags
            Array.Clear(_visited, 0, _boardSize);

            int groupCount = 0;
            ushort currentGroupId = 1;

            for (int i = 0; i < _boardSize; i++)
            {
                if (_visited[i] || board[i].IsEmpty)
                    continue;

                // Flood fill from this block
                int groupSize = FloodFill(board, i, currentGroupId);

                if (groupSize >= GroupInfo.MinGroupSize)
                {
                    groups[groupCount] = new GroupInfo
                    {
                        GroupId = currentGroupId,
                        ColorIndex = board[i].ColorIndex,
                        Size = (ushort)groupSize,
                        StartIndex = i
                    };
                    groupCount++;
                    currentGroupId++;
                }
                else
                {
                    // Single block or invalid, clear group ID
                    board[i].GroupId = 0;
                }
            }

            return groupCount;
        }

        /// <summary>
        /// Flood fill algorithm using iterative stack (no recursion).
        /// Returns the size of the group found.
        /// </summary>
        private int FloodFill(BlockData[] board, int startIndex, ushort groupId)
        {
            int stackTop = 0;
            _visitStack[stackTop++] = startIndex;
            _visited[startIndex] = true;

            byte targetColor = board[startIndex].ColorIndex;
            int count = 0;

            while (stackTop > 0)
            {
                int current = _visitStack[--stackTop];
                board[current].GroupId = groupId;
                count++;

                int row = current / _columns;
                int col = current % _columns;

                // Check UP neighbor
                if (row > 0)
                {
                    int neighbor = current - _columns;
                    if (!_visited[neighbor] && board[neighbor].ColorIndex == targetColor)
                    {
                        _visited[neighbor] = true;
                        _visitStack[stackTop++] = neighbor;
                    }
                }

                // Check DOWN neighbor
                if (row < _rows - 1)
                {
                    int neighbor = current + _columns;
                    if (!_visited[neighbor] && board[neighbor].ColorIndex == targetColor)
                    {
                        _visited[neighbor] = true;
                        _visitStack[stackTop++] = neighbor;
                    }
                }

                // Check LEFT neighbor
                if (col > 0)
                {
                    int neighbor = current - 1;
                    if (!_visited[neighbor] && board[neighbor].ColorIndex == targetColor)
                    {
                        _visited[neighbor] = true;
                        _visitStack[stackTop++] = neighbor;
                    }
                }

                // Check RIGHT neighbor
                if (col < _columns - 1)
                {
                    int neighbor = current + 1;
                    if (!_visited[neighbor] && board[neighbor].ColorIndex == targetColor)
                    {
                        _visited[neighbor] = true;
                        _visitStack[stackTop++] = neighbor;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Get all block indices that belong to a specific group.
        /// </summary>
        public int GetGroupMembers(BlockData[] board, ushort groupId, int[] outputIndices)
        {
            int count = 0;
            for (int i = 0; i < _boardSize; i++)
            {
                if (board[i].GroupId == groupId)
                {
                    outputIndices[count++] = i;
                }
            }
            return count;
        }

        /// <summary>
        /// Find the group that contains a specific block index.
        /// Returns null group info if block is not in a valid group.
        /// </summary>
        public bool TryGetGroupAt(BlockData[] board, GroupInfo[] groups, int groupCount, int blockIndex, out GroupInfo group)
        {
            if (blockIndex < 0 || blockIndex >= _boardSize || board[blockIndex].IsEmpty)
            {
                group = default;
                return false;
            }

            ushort targetGroupId = board[blockIndex].GroupId;
            if (targetGroupId == 0)
            {
                group = default;
                return false;
            }

            for (int i = 0; i < groupCount; i++)
            {
                if (groups[i].GroupId == targetGroupId)
                {
                    group = groups[i];
                    return true;
                }
            }

            group = default;
            return false;
        }
    }
}
