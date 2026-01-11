// ============================================================================
// Made By Hakan Emre ÖZKAN
// For more follow my itch.io account (Heodev) - To begin, begin
// ============================================================================

using System;
using GoodJobInternshipCase.Config;

namespace GoodJobInternshipCase.Board
{
    /// <summary>
    /// Manages all board logic including group detection, blasting, falling, and spawning.
    /// Pure logic class (no MonoBehaviour) for better testability and separation of concerns.
    /// </summary>
    public class BoardManager
    {
        // Board data
        private BlockData[] _boardData;
        private int _rows;
        private int _columns;
        private int _boardSize;

        // Subsystems
        private readonly GroupDetector _groupDetector;
        private readonly FallProcessor _fallProcessor;
        private readonly SpawnProcessor _spawnProcessor;
        private readonly ShuffleProcessor _shuffleProcessor;
        private readonly DeadlockDetector _deadlockDetector;

        // Pre-allocated buffers
        private GroupInfo[] _groups;
        private FallData[] _fallBuffer;
        private SpawnData[] _spawnBuffer;
        private int[] _destroyBuffer;
        private int[] _tempBuffer;

        // State
        private int _groupCount;
        private bool _groupsDirty;
        private GameConfig _config;

        // Events
        public event Action<int[], int> OnBlocksDestroyed;      // indices, count
        public event Action<FallData[], int> OnBlocksFalling;   // data, count
        public event Action<SpawnData[], int> OnBlocksSpawned;  // data, count
        public event Action OnGroupsUpdated;
        public event Action OnDeadlockDetected;
        public event Action OnShuffleComplete;
        public event Action OnBoardReady;

        public BoardManager()
        {
            _groupDetector = new GroupDetector();
            _fallProcessor = new FallProcessor();
            _spawnProcessor = new SpawnProcessor();
            _shuffleProcessor = new ShuffleProcessor();
            _deadlockDetector = new DeadlockDetector();
        }

        /// <summary>
        /// Initialize board with configuration
        /// </summary>
        public void Initialize(GameConfig config)
        {
            _config = config;
            _rows = config.Rows;
            _columns = config.Columns;
            _boardSize = _rows * _columns;

            // Pre-allocate all arrays
            _boardData = new BlockData[_boardSize];
            _groups = new GroupInfo[_boardSize / 2 + 1];
            _fallBuffer = new FallData[_boardSize];
            _spawnBuffer = new SpawnData[_boardSize];
            _destroyBuffer = new int[_boardSize];
            _tempBuffer = new int[_columns];

            // Initialize subsystems
            _groupDetector.Initialize(_rows, _columns);
            _fallProcessor.Initialize(_rows, _columns);
            _spawnProcessor.Initialize(_rows, _columns, config.ColorCount);
            _shuffleProcessor.Initialize(_rows, _columns);
            _deadlockDetector.Initialize(_rows, _columns);

            _groupsDirty = true;
        }

        /// <summary>
        /// Generate initial board state
        /// </summary>
        public void GenerateBoard()
        {
            _spawnProcessor.FillBoard(_boardData);
            _groupsDirty = true;
            UpdateGroupsIfDirty();

            // Check for initial deadlock
            if (!_deadlockDetector.HasValidMoves(_boardData))
            {
                PerformShuffle();
            }

            OnBoardReady?.Invoke();
        }

        /// <summary>
        /// Reset board to initial state
        /// </summary>
        public void Reset()
        {
            GenerateBoard();
        }

        /// <summary>
        /// Attempt to blast group at given board index.
        /// Returns true if blast was successful.
        /// </summary>
        public bool TryBlastAt(int boardIndex)
        {
            UpdateGroupsIfDirty();

            if (boardIndex < 0 || boardIndex >= _boardSize)
                return false;

            if (_boardData[boardIndex].IsEmpty)
                return false;

            if (!_groupDetector.TryGetGroupAt(_boardData, _groups, _groupCount, boardIndex, out GroupInfo group))
                return false;

            if (group.Size < GroupInfo.MinGroupSize)
                return false;

            // Get all blocks in group
            int destroyCount = _groupDetector.GetGroupMembers(_boardData, group.GroupId, _destroyBuffer);

            // Clear blocks from board
            for (int i = 0; i < destroyCount; i++)
            {
                _boardData[_destroyBuffer[i]].Clear();
            }

            // Notify listeners
            OnBlocksDestroyed?.Invoke(_destroyBuffer, destroyCount);

            _groupsDirty = true;
            return true;
        }

        /// <summary>
        /// Process falling blocks after blast.
        /// Returns number of blocks that fell.
        /// </summary>
        public int ProcessFalls()
        {
            int fallCount = _fallProcessor.ProcessFalls(_boardData, _fallBuffer);

            if (fallCount > 0)
            {
                OnBlocksFalling?.Invoke(_fallBuffer, fallCount);
                _groupsDirty = true;
            }

            return fallCount;
        }

        /// <summary>
        /// Spawn new blocks to fill empty cells.
        /// Returns number of blocks spawned.
        /// </summary>
        public int SpawnBlocks()
        {
            int spawnCount = _spawnProcessor.SpawnNewBlocks(_boardData, _spawnBuffer);

            if (spawnCount > 0)
            {
                OnBlocksSpawned?.Invoke(_spawnBuffer, spawnCount);
                _groupsDirty = true;
            }

            return spawnCount;
        }

        /// <summary>
        /// Check for deadlock and trigger shuffle if needed.
        /// Returns true if deadlock was detected.
        /// </summary>
        public bool CheckDeadlock()
        {
            UpdateGroupsIfDirty();

            if (!_deadlockDetector.HasValidMoves(_boardData))
            {
                OnDeadlockDetected?.Invoke();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Perform smart shuffle to resolve deadlock
        /// </summary>
        public void PerformShuffle()
        {
            _shuffleProcessor.SmartShuffle(_boardData);
            _groupsDirty = true;
            UpdateGroupsIfDirty();

            // Verify shuffle created valid state (safety check)
            if (!_deadlockDetector.HasValidMoves(_boardData))
            {
                // Extreme edge case - try again with different seed
                _shuffleProcessor.SetSeed(Environment.TickCount);
                _shuffleProcessor.SmartShuffle(_boardData);
                _groupsDirty = true;
                UpdateGroupsIfDirty();
            }

            OnShuffleComplete?.Invoke();
        }

        /// <summary>
        /// Update group detection if board state changed
        /// </summary>
        public void UpdateGroupsIfDirty()
        {
            if (!_groupsDirty)
                return;

            _groupCount = _groupDetector.DetectAllGroups(_boardData, _groups);
            UpdateIconStates();
            OnGroupsUpdated?.Invoke();

            _groupsDirty = false;
        }

        /// <summary>
        /// Force update all groups (use sparingly)
        /// </summary>
        public void ForceUpdateGroups()
        {
            _groupsDirty = true;
            UpdateGroupsIfDirty();
        }

        /// <summary>
        /// Update icon states based on group sizes
        /// </summary>
        private void UpdateIconStates()
        {
            // First reset all to default
            for (int i = 0; i < _boardSize; i++)
            {
                if (!_boardData[i].IsEmpty)
                    _boardData[i].IconState = 0;
            }

            // Then update based on group membership
            for (int g = 0; g < _groupCount; g++)
            {
                byte iconState = _config.CalculateIconState(_groups[g].Size);
                ushort groupId = _groups[g].GroupId;

                for (int i = 0; i < _boardSize; i++)
                {
                    if (_boardData[i].GroupId == groupId)
                    {
                        _boardData[i].IconState = iconState;
                    }
                }
            }
        }

        #region Accessors

        /// <summary>
        /// Get block data at specific index (read-only access)
        /// </summary>
        public BlockData GetBlockAt(int index)
        {
            if (index < 0 || index >= _boardSize)
                return BlockData.CreateEmpty();
            return _boardData[index];
        }

        /// <summary>
        /// Get block data at row, column position
        /// </summary>
        public BlockData GetBlockAt(int row, int column)
        {
            return GetBlockAt(row * _columns + column);
        }

        /// <summary>
        /// Convert world position to board index
        /// </summary>
        public int WorldToIndex(float worldX, float worldY, float cellSize, float originX, float originY)
        {
            int col = (int)((worldX - originX) / cellSize);
            int row = (int)((originY - worldY) / cellSize); // Y is inverted

            if (row < 0 || row >= _rows || col < 0 || col >= _columns)
                return -1;

            return row * _columns + col;
        }

        /// <summary>
        /// Get board dimensions
        /// </summary>
        public int Rows => _rows;
        public int Columns => _columns;
        public int BoardSize => _boardSize;

        /// <summary>
        /// Get current group count
        /// </summary>
        public int GroupCount => _groupCount;

        /// <summary>
        /// Get direct reference to board data (use carefully)
        /// </summary>
        public BlockData[] BoardData => _boardData;

        /// <summary>
        /// Get groups array (use carefully)
        /// </summary>
        public GroupInfo[] Groups => _groups;

        #endregion
    }
}
