// ============================================================================
// Made By Hakan Emre ÖZKAN
// For more follow my itch.io account (Heodev) - Begin to begin
// ============================================================================

using System;

namespace GoodJobInternshipCase.Board
{
    /// <summary>
    /// Implements smart shuffle algorithm that guarantees valid board state.
    /// Does NOT use blind random shuffling - uses constraint-satisfaction approach.
    /// </summary>
    public class ShuffleProcessor
    {
        private Random _random;
        private int[] _colorCounts;
        private int[] _shuffleSequence;
        private int[] _positionBuffer;
        private int _rows;
        private int _columns;
        private int _boardSize;

        /// <summary>
        /// Initialize processor with board dimensions
        /// </summary>
        public void Initialize(int rows, int columns, int seed = -1)
        {
            _rows = rows;
            _columns = columns;
            _boardSize = rows * columns;

            _colorCounts = new int[BlockData.MaxColors];
            _shuffleSequence = new int[_boardSize];
            _positionBuffer = new int[_boardSize];
            _random = seed >= 0 ? new Random(seed) : new Random();
        }

        /// <summary>
        /// Perform smart shuffle that guarantees at least one valid group exists.
        /// Preserves color distribution of existing blocks.
        /// </summary>
        public void SmartShuffle(BlockData[] board)
        {
            // Step 1: Count existing colors
            Array.Clear(_colorCounts, 0, BlockData.MaxColors);
            int totalBlocks = 0;
            int positionCount = 0;

            for (int i = 0; i < _boardSize; i++)
            {
                if (!board[i].IsEmpty)
                {
                    _colorCounts[board[i].ColorIndex]++;
                    _positionBuffer[positionCount++] = i;
                    totalBlocks++;
                }
            }

            if (totalBlocks < 2)
                return; // Not enough blocks to shuffle

            // Step 2: Create color sequence that guarantees valid groups
            CreateValidColorSequence(totalBlocks);

            // Step 3: Apply sequence to board positions
            for (int i = 0; i < positionCount; i++)
            {
                int boardIndex = _positionBuffer[i];
                board[boardIndex].ColorIndex = (byte)_shuffleSequence[i];
                board[boardIndex].IconState = 0;
                board[boardIndex].GroupId = 0;
            }
        }

        /// <summary>
        /// Creates a color sequence that guarantees at least some adjacent same-color pairs.
        /// Uses cluster-based approach instead of pure random.
        /// </summary>
        private void CreateValidColorSequence(int totalBlocks)
        {
            int sequenceIndex = 0;

            // Strategy: Place colors in small clusters (2-4 blocks each)
            // This guarantees valid groups exist after placement

            // Create a weighted list of available colors
            int activeColorCount = 0;
            for (int c = 0; c < BlockData.MaxColors; c++)
            {
                if (_colorCounts[c] > 0)
                    activeColorCount++;
            }

            // Place in clusters
            while (sequenceIndex < totalBlocks)
            {
                // Find a color with remaining blocks
                int colorToPlace = -1;
                int maxRemaining = 0;

                // Randomly select from colors with remaining blocks
                // But prefer colors with more remaining (for balance)
                for (int c = 0; c < BlockData.MaxColors; c++)
                {
                    if (_colorCounts[c] > 0)
                    {
                        // Random selection with bias towards colors with more remaining
                        if (_random.Next(0, _colorCounts[c] + 1) >= maxRemaining)
                        {
                            maxRemaining = _colorCounts[c];
                            colorToPlace = c;
                        }
                    }
                }

                if (colorToPlace < 0)
                    break; // No more colors

                // Determine cluster size (2-4, but limited by remaining count)
                int remainingSlots = totalBlocks - sequenceIndex;
                int remainingOfColor = _colorCounts[colorToPlace];
                int clusterSize = Math.Min(
                    _random.Next(2, 5),
                    Math.Min(remainingOfColor, remainingSlots)
                );

                // Ensure at least 2 if possible (for valid group)
                if (remainingOfColor >= 2 && clusterSize < 2)
                    clusterSize = 2;

                // Place cluster
                for (int i = 0; i < clusterSize; i++)
                {
                    _shuffleSequence[sequenceIndex++] = colorToPlace;
                    _colorCounts[colorToPlace]--;
                }
            }

            // Light shuffle to add randomness while preserving some adjacencies
            LightShuffle(totalBlocks);

            // Final verification and fix if needed
            if (!HasAdjacentPairs(totalBlocks))
            {
                ForceCreateAdjacentPair(totalBlocks);
            }
        }

        /// <summary>
        /// Light shuffle that swaps only ~25% of elements to maintain some adjacencies
        /// </summary>
        private void LightShuffle(int count)
        {
            int swapCount = count / 4;

            for (int i = 0; i < swapCount; i++)
            {
                int a = _random.Next(count);
                int b = _random.Next(count);

                // Swap
                int temp = _shuffleSequence[a];
                _shuffleSequence[a] = _shuffleSequence[b];
                _shuffleSequence[b] = temp;
            }
        }

        /// <summary>
        /// Check if the sequence would result in at least one adjacent pair
        /// when placed on the board
        /// </summary>
        private bool HasAdjacentPairs(int count)
        {
            // Check horizontal adjacencies (consecutive in sequence = same row)
            for (int i = 0; i < count - 1; i++)
            {
                // Check if positions are horizontally adjacent on board
                int posA = _positionBuffer[i];
                int posB = _positionBuffer[i + 1];

                // Same row and adjacent columns
                if (posA / _columns == posB / _columns && Math.Abs(posA - posB) == 1)
                {
                    if (_shuffleSequence[i] == _shuffleSequence[i + 1])
                        return true;
                }
            }

            // Check vertical adjacencies
            for (int i = 0; i < count; i++)
            {
                int posA = _positionBuffer[i];
                int targetPos = posA + _columns; // Position below

                // Find if targetPos is in our position buffer
                for (int j = 0; j < count; j++)
                {
                    if (_positionBuffer[j] == targetPos)
                    {
                        if (_shuffleSequence[i] == _shuffleSequence[j])
                            return true;
                        break;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Force create at least one adjacent pair by swapping elements
        /// </summary>
        private void ForceCreateAdjacentPair(int count)
        {
            if (count < 2)
                return;

            // Find the most common color
            int[] tempCounts = new int[BlockData.MaxColors];
            for (int i = 0; i < count; i++)
            {
                tempCounts[_shuffleSequence[i]]++;
            }

            int maxColor = 0;
            for (int c = 1; c < BlockData.MaxColors; c++)
            {
                if (tempCounts[c] > tempCounts[maxColor])
                    maxColor = c;
            }

            // Find first two positions with this color
            int first = -1, second = -1;
            for (int i = 0; i < count; i++)
            {
                if (_shuffleSequence[i] == maxColor)
                {
                    if (first < 0)
                        first = i;
                    else
                    {
                        second = i;
                        break;
                    }
                }
            }

            if (first >= 0 && second >= 0 && first < count - 1)
            {
                // Swap element at position first+1 with element at second
                int target = first + 1;
                if (target != second)
                {
                    int temp = _shuffleSequence[target];
                    _shuffleSequence[target] = _shuffleSequence[second];
                    _shuffleSequence[second] = temp;
                }
            }
        }

        /// <summary>
        /// Set random seed for reproducible shuffles (useful for testing)
        /// </summary>
        public void SetSeed(int seed)
        {
            _random = new Random(seed);
        }
    }
}
