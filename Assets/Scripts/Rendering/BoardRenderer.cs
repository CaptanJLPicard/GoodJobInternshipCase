using System;
using System.Collections.Generic;
using UnityEngine;
using GoodJobInternshipCase.Board;
using GoodJobInternshipCase.Config;

namespace GoodJobInternshipCase.Rendering
{
    /// <summary>
    /// Renders the game board and coordinates block animations.
    /// Bridges between BoardManager logic and visual representation.
    /// </summary>
    public class BoardRenderer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BlockPool _blockPool;
        [SerializeField] private Transform _boardContainer;

        [Header("Settings")]
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private float _fallDuration = 0.3f;
        [SerializeField] private float _spawnDuration = 0.25f;
        [SerializeField] private float _blastDuration = 0.2f;
        [SerializeField] private float _shuffleDuration = 0.5f;

        // Board state
        private BlockVisual[] _activeBlocks;
        private int _rows;
        private int _columns;
        private Vector3 _boardOrigin;
        private GameConfig _config;

        // Animation tracking
        private List<BlockVisual> _animatingBlocks;
        private int _pendingAnimations;
        private bool _isAnimating;

        // Events
        public event Action OnAllAnimationsComplete;
        public event Action<int> OnBlockClicked;

        public bool IsAnimating => _isAnimating || _pendingAnimations > 0;

        private void Awake()
        {
            _animatingBlocks = new List<BlockVisual>(100);
        }

        private void Update()
        {
            // Update all animations in single pass
            if (_animatingBlocks.Count > 0)
            {
                float dt = Time.deltaTime;

                // Iterate backwards to safely remove completed animations
                for (int i = _animatingBlocks.Count - 1; i >= 0; i--)
                {
                    _animatingBlocks[i].UpdateAnimation(dt);
                }
            }
        }

        /// <summary>
        /// Initialize renderer with configuration
        /// </summary>
        public void Initialize(GameConfig config)
        {
            _config = config;
            _rows = config.Rows;
            _columns = config.Columns;
            _cellSize = config.TotalCellSize;
            _fallDuration = config.FallDuration;
            _spawnDuration = config.SpawnDuration;
            _blastDuration = config.BlastDuration;
            _shuffleDuration = config.ShuffleDuration;

            // Initialize active blocks array
            _activeBlocks = new BlockVisual[_rows * _columns];

            // Initialize pool if needed
            if (_blockPool != null && !_blockPool.IsInitialized)
            {
                _blockPool.Initialize(config.MaxPoolSize);
            }

            // Create board container if not assigned
            if (_boardContainer == null)
            {
                GameObject container = new GameObject("BoardContainer");
                container.transform.SetParent(transform);
                _boardContainer = container.transform;
            }

            // Calculate board origin (top-left corner)
            float boardWidth = _columns * _cellSize;
            float boardHeight = _rows * _cellSize;
            _boardOrigin = new Vector3(-boardWidth / 2f + _cellSize / 2f, boardHeight / 2f - _cellSize / 2f, 0f);
        }

        /// <summary>
        /// Render initial board state
        /// </summary>
        public void RenderBoard(BlockData[] boardData)
        {
            ClearBoard();

            for (int i = 0; i < boardData.Length; i++)
            {
                if (!boardData[i].IsEmpty)
                {
                    CreateBlockVisual(i, boardData[i]);
                }
            }
        }

        /// <summary>
        /// Update icon states for all blocks
        /// </summary>
        public void UpdateIconStates(BlockData[] boardData)
        {
            for (int i = 0; i < _activeBlocks.Length; i++)
            {
                if (_activeBlocks[i] != null && i < boardData.Length)
                {
                    _activeBlocks[i].SetIconState(boardData[i].IconState);
                }
            }
        }

        /// <summary>
        /// Render block destruction with animation
        /// </summary>
        public void RenderBlast(int[] destroyedIndices, int count)
        {
            _isAnimating = true;
            _pendingAnimations += count;

            for (int i = 0; i < count; i++)
            {
                int index = destroyedIndices[i];
                BlockVisual block = _activeBlocks[index];

                if (block != null)
                {
                    block.OnAnimationComplete += OnBlastComplete;
                    block.StartBlastAnimation(_blastDuration);
                    _animatingBlocks.Add(block);
                    _activeBlocks[index] = null;
                }
                else
                {
                    _pendingAnimations--;
                }
            }

            CheckAnimationsComplete();
        }

        /// <summary>
        /// Render falling blocks with animation
        /// </summary>
        public void RenderFalls(FallData[] fallData, int count)
        {
            _isAnimating = true;
            _pendingAnimations += count;

            for (int i = 0; i < count; i++)
            {
                int fromIndex = fallData[i].FromIndex;
                int toIndex = fallData[i].ToIndex;

                BlockVisual block = _activeBlocks[fromIndex];
                if (block != null)
                {
                    Vector3 fromPos = IndexToWorldPosition(fromIndex);
                    Vector3 toPos = IndexToWorldPosition(toIndex);

                    // Calculate duration based on fall distance
                    float duration = _fallDuration * (1f + fallData[i].FallDistance * 0.1f);

                    block.BoardIndex = toIndex;
                    block.OnAnimationComplete += OnFallComplete;
                    block.StartFallAnimation(fromPos, toPos, duration);
                    _animatingBlocks.Add(block);

                    // Update active blocks array
                    _activeBlocks[fromIndex] = null;
                    _activeBlocks[toIndex] = block;
                }
                else
                {
                    _pendingAnimations--;
                }
            }

            CheckAnimationsComplete();
        }

        /// <summary>
        /// Render spawning blocks with animation
        /// </summary>
        public void RenderSpawns(SpawnData[] spawnData, int count, BlockData[] boardData)
        {
            _isAnimating = true;
            _pendingAnimations += count;

            for (int i = 0; i < count; i++)
            {
                int targetIndex = spawnData[i].TargetIndex;

                BlockVisual block = _blockPool.Get();
                if (block != null)
                {
                    // Set color from board data
                    byte colorIndex = boardData[targetIndex].ColorIndex;
                    if (colorIndex < _config.ColorSprites.Length)
                    {
                        block.SetColor(colorIndex, _config.ColorSprites[colorIndex]);
                    }

                    block.BoardIndex = targetIndex;

                    // Calculate positions
                    Vector3 targetPos = IndexToWorldPosition(targetIndex);
                    Vector3 spawnPos = targetPos + Vector3.up * (-spawnData[i].SpawnRow * _cellSize);

                    // Calculate duration based on spawn row
                    float duration = _spawnDuration * (1f + (-spawnData[i].SpawnRow) * 0.1f);

                    block.transform.SetParent(_boardContainer);
                    block.OnAnimationComplete += OnSpawnComplete;
                    block.StartSpawnAnimation(spawnPos, targetPos, duration);
                    _animatingBlocks.Add(block);

                    _activeBlocks[targetIndex] = block;
                }
                else
                {
                    _pendingAnimations--;
                }
            }

            CheckAnimationsComplete();
        }

        /// <summary>
        /// Render shuffle animation for all blocks
        /// </summary>
        public void RenderShuffle(BlockData[] boardData)
        {
            // Update colors first
            for (int i = 0; i < _activeBlocks.Length; i++)
            {
                BlockVisual block = _activeBlocks[i];
                if (block != null && i < boardData.Length && !boardData[i].IsEmpty)
                {
                    byte colorIndex = boardData[i].ColorIndex;
                    if (colorIndex < _config.ColorSprites.Length)
                    {
                        block.SetColor(colorIndex, _config.ColorSprites[colorIndex]);
                    }
                }
            }

            // Start shuffle animation for all blocks
            _isAnimating = true;
            int activeCount = 0;

            for (int i = 0; i < _activeBlocks.Length; i++)
            {
                if (_activeBlocks[i] != null)
                {
                    activeCount++;
                }
            }

            _pendingAnimations += activeCount;

            for (int i = 0; i < _activeBlocks.Length; i++)
            {
                BlockVisual block = _activeBlocks[i];
                if (block != null)
                {
                    block.OnAnimationComplete += OnShuffleComplete;
                    block.StartShuffleAnimation(_shuffleDuration);
                    _animatingBlocks.Add(block);
                }
            }

            CheckAnimationsComplete();
        }

        /// <summary>
        /// Clear all blocks from the board
        /// </summary>
        public void ClearBoard()
        {
            if (_blockPool != null)
            {
                _blockPool.ReturnAll();
            }

            if (_activeBlocks != null)
            {
                Array.Clear(_activeBlocks, 0, _activeBlocks.Length);
            }

            _animatingBlocks.Clear();
            _pendingAnimations = 0;
            _isAnimating = false;
        }

        /// <summary>
        /// Handle click/touch input at world position
        /// </summary>
        public int GetBlockIndexAtPosition(Vector3 worldPosition)
        {
            Vector3 localPos = worldPosition - _boardOrigin;

            int col = Mathf.FloorToInt((localPos.x + _cellSize / 2f) / _cellSize);
            int row = Mathf.FloorToInt((-localPos.y + _cellSize / 2f) / _cellSize);

            if (row < 0 || row >= _rows || col < 0 || col >= _columns)
                return -1;

            return row * _columns + col;
        }

        #region Animation Callbacks

        private void OnBlastComplete(BlockVisual block)
        {
            block.OnAnimationComplete -= OnBlastComplete;
            _animatingBlocks.Remove(block);
            _blockPool.Return(block);
            _pendingAnimations--;
            CheckAnimationsComplete();
        }

        private void OnFallComplete(BlockVisual block)
        {
            block.OnAnimationComplete -= OnFallComplete;
            _animatingBlocks.Remove(block);
            _pendingAnimations--;
            CheckAnimationsComplete();
        }

        private void OnSpawnComplete(BlockVisual block)
        {
            block.OnAnimationComplete -= OnSpawnComplete;
            _animatingBlocks.Remove(block);
            _pendingAnimations--;
            CheckAnimationsComplete();
        }

        private void OnShuffleComplete(BlockVisual block)
        {
            block.OnAnimationComplete -= OnShuffleComplete;
            _animatingBlocks.Remove(block);
            _pendingAnimations--;
            CheckAnimationsComplete();
        }

        private void CheckAnimationsComplete()
        {
            if (_pendingAnimations <= 0)
            {
                _isAnimating = false;
                _pendingAnimations = 0;
                OnAllAnimationsComplete?.Invoke();
            }
        }

        #endregion

        #region Helper Methods

        private void CreateBlockVisual(int boardIndex, BlockData blockData)
        {
            BlockVisual block = _blockPool.Get();
            if (block == null)
                return;

            byte colorIndex = blockData.ColorIndex;
            if (colorIndex < _config.ColorSprites.Length)
            {
                block.SetColor(colorIndex, _config.ColorSprites[colorIndex]);
                block.SetIconState(blockData.IconState);
            }

            block.BoardIndex = boardIndex;
            block.transform.SetParent(_boardContainer);
            block.SetPosition(IndexToWorldPosition(boardIndex));

            _activeBlocks[boardIndex] = block;
        }

        private Vector3 IndexToWorldPosition(int index)
        {
            int row = index / _columns;
            int col = index % _columns;

            return new Vector3(
                _boardOrigin.x + col * _cellSize,
                _boardOrigin.y - row * _cellSize,
                0f
            );
        }

        #endregion

        #region Accessors

        public Vector3 BoardOrigin => _boardOrigin;
        public float CellSize => _cellSize;
        public int Rows => _rows;
        public int Columns => _columns;

        #endregion
    }
}
