using System;
using UnityEngine;
using GoodJobInternshipCase.Board;
using GoodJobInternshipCase.Config;
using GoodJobInternshipCase.Core;
using GoodJobInternshipCase.Rendering;
using GoodJobInternshipCase.Feedback;

/// <summary>
/// Main game controller that orchestrates all game systems.
/// Manages game state, coordinates between logic and visuals.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private GameConfig _config;

    [Header("References")]
    [SerializeField] private BoardRenderer _boardRenderer;
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private FeedbackManager _feedbackManager;
    [SerializeField] private Camera _mainCamera;

    [Header("UI References")]
    [SerializeField] private TMPro.TextMeshProUGUI _scoreText;

    [Header("Runtime Info (Read Only)")]
    [SerializeField] private GameState _currentState;
    [SerializeField] private int _score;

    // Core systems
    private BoardManager _boardManager;

    // State
    private bool _isProcessingCascade;

    // Events
    public event Action<GameState> OnStateChanged;
    public event Action<int> OnScoreChanged;
    public event Action OnGameReady;

    public GameState CurrentState => _currentState;
    public int Score => _score;
    public GameConfig Config => _config;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Initialize camera reference
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        // Create board manager
        _boardManager = new BoardManager();
    }

    private void Start()
    {
        InitializeGame();
    }

    private void OnDestroy()
    {
        // Cleanup event subscriptions
        if (_boardManager != null)
        {
            _boardManager.OnBlocksDestroyed -= HandleBlocksDestroyed;
            _boardManager.OnBlocksFalling -= HandleBlocksFalling;
            _boardManager.OnBlocksSpawned -= HandleBlocksSpawned;
            _boardManager.OnGroupsUpdated -= HandleGroupsUpdated;
            _boardManager.OnDeadlockDetected -= HandleDeadlockDetected;
            _boardManager.OnShuffleComplete -= HandleShuffleComplete;
        }

        if (_inputHandler != null)
        {
            _inputHandler.OnClick -= HandleClick;
        }

        if (_boardRenderer != null)
        {
            _boardRenderer.OnAllAnimationsComplete -= HandleAnimationsComplete;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Initialize all game systems
    /// </summary>
    private void InitializeGame()
    {
        if (_config == null)
        {
            Debug.LogError("GameConfig is not assigned!");
            return;
        }

        // Initialize board manager
        _boardManager.Initialize(_config);

        // Subscribe to board events
        _boardManager.OnBlocksDestroyed += HandleBlocksDestroyed;
        _boardManager.OnBlocksFalling += HandleBlocksFalling;
        _boardManager.OnBlocksSpawned += HandleBlocksSpawned;
        _boardManager.OnGroupsUpdated += HandleGroupsUpdated;
        _boardManager.OnDeadlockDetected += HandleDeadlockDetected;
        _boardManager.OnShuffleComplete += HandleShuffleComplete;
        _boardManager.OnBoardReady += HandleBoardReady;

        // Initialize renderer
        if (_boardRenderer != null)
        {
            _boardRenderer.Initialize(_config);
            _boardRenderer.OnAllAnimationsComplete += HandleAnimationsComplete;
        }

        // Initialize input
        if (_inputHandler != null)
        {
            _inputHandler.SetCamera(_mainCamera);
            _inputHandler.OnClick += HandleClick;
        }

        // Reset score
        _score = 0;
        UpdateScoreUI();

        // Generate initial board
        _boardManager.GenerateBoard();
    }

    /// <summary>
    /// Restart the game
    /// </summary>
    public void RestartGame()
    {
        _score = 0;
        UpdateScoreUI();

        if (_boardRenderer != null)
        {
            _boardRenderer.ClearBoard();
        }

        _boardManager.Reset();
        SetState(GameState.Playing);
    }

    /// <summary>
    /// Change game state
    /// </summary>
    private void SetState(GameState newState)
    {
        if (_currentState == newState)
            return;

        _currentState = newState;
        OnStateChanged?.Invoke(newState);

        // Handle state-specific logic
        switch (newState)
        {
            case GameState.Playing:
                if (_inputHandler != null)
                    _inputHandler.Enable();
                break;

            case GameState.Animating:
            case GameState.Shuffling:
                if (_inputHandler != null)
                    _inputHandler.Disable();
                break;
        }
    }

    #region Input Handling

    private void HandleClick(Vector3 worldPosition)
    {
        if (_currentState != GameState.Playing)
            return;

        if (_boardRenderer == null)
            return;

        int blockIndex = _boardRenderer.GetBlockIndexAtPosition(worldPosition);

        if (blockIndex >= 0)
        {
            TryBlastAt(blockIndex);
        }
    }

    private void TryBlastAt(int boardIndex)
    {
        if (_boardManager.TryBlastAt(boardIndex))
        {
            _isProcessingCascade = true;
            SetState(GameState.Animating);
        }
    }

    #endregion

    #region Board Event Handlers

    private void HandleBlocksDestroyed(int[] indices, int count)
    {
        // Add score based on destroyed blocks
        int points = CalculateScore(count);
        AddScore(points);

        // Play feedback
        if (_feedbackManager != null && count > 0)
        {
            Vector3 center = CalculateGroupCenter(indices, count);
            _feedbackManager.PlayBlastFeedback(center, count);
        }

        // Render destruction
        if (_boardRenderer != null)
        {
            _boardRenderer.RenderBlast(indices, count);
        }
    }

    private void HandleBlocksFalling(FallData[] fallData, int count)
    {
        if (_boardRenderer != null)
        {
            _boardRenderer.RenderFalls(fallData, count);
        }
    }

    private void HandleBlocksSpawned(SpawnData[] spawnData, int count)
    {
        if (_boardRenderer != null)
        {
            _boardRenderer.RenderSpawns(spawnData, count, _boardManager.BoardData);
        }
    }

    private void HandleGroupsUpdated()
    {
        if (_boardRenderer != null)
        {
            _boardRenderer.UpdateIconStates(_boardManager.BoardData);
        }
    }

    private void HandleDeadlockDetected()
    {
        SetState(GameState.Shuffling);

        if (_feedbackManager != null)
        {
            _feedbackManager.PlayDeadlockWarning();
        }

        // Perform shuffle
        _boardManager.PerformShuffle();
    }

    private void HandleShuffleComplete()
    {
        if (_boardRenderer != null)
        {
            _boardRenderer.RenderShuffle(_boardManager.BoardData);
        }

        if (_feedbackManager != null)
        {
            _feedbackManager.PlayShuffleFeedback();
        }
    }

    private void HandleBoardReady()
    {
        if (_boardRenderer != null)
        {
            _boardRenderer.RenderBoard(_boardManager.BoardData);
        }

        SetState(GameState.Playing);
        OnGameReady?.Invoke();
    }

    #endregion

    #region Animation Handling

    private void HandleAnimationsComplete()
    {
        if (!_isProcessingCascade)
        {
            SetState(GameState.Playing);
            return;
        }

        // Continue cascade: falls -> spawns -> check deadlock
        int fallCount = _boardManager.ProcessFalls();

        if (fallCount > 0)
        {
            // Wait for fall animations
            return;
        }

        int spawnCount = _boardManager.SpawnBlocks();

        if (spawnCount > 0)
        {
            // Wait for spawn animations
            return;
        }

        // Cascade complete, update groups and check deadlock
        _boardManager.UpdateGroupsIfDirty();

        // Play land feedback
        if (_feedbackManager != null)
        {
            _feedbackManager.PlayLandFeedback();
        }

        if (_boardManager.CheckDeadlock())
        {
            // Deadlock detected, shuffle will be triggered
            return;
        }

        // All done, return to playing state
        _isProcessingCascade = false;
        SetState(GameState.Playing);
    }

    #endregion

    #region Score System

    private int CalculateScore(int blocksDestroyed)
    {
        // Base score + bonus for larger groups
        int baseScore = blocksDestroyed * 10;
        int bonus = 0;

        if (blocksDestroyed > _config.GetThresholdC())
        {
            bonus = blocksDestroyed * 5;
        }
        else if (blocksDestroyed > _config.GetThresholdB())
        {
            bonus = blocksDestroyed * 3;
        }
        else if (blocksDestroyed > _config.GetThresholdA())
        {
            bonus = blocksDestroyed * 2;
        }

        return baseScore + bonus;
    }

    private void AddScore(int points)
    {
        _score += points;
        UpdateScoreUI();
        OnScoreChanged?.Invoke(_score);
    }

    private void UpdateScoreUI()
    {
        if (_scoreText != null)
        {
            _scoreText.text = $"Score: {_score}";
        }
    }

    #endregion

    #region Utility Methods

    private Vector3 CalculateGroupCenter(int[] indices, int count)
    {
        if (_boardRenderer == null || count == 0)
            return Vector3.zero;

        Vector3 sum = Vector3.zero;
        int cols = _boardManager.Columns;

        for (int i = 0; i < count; i++)
        {
            int idx = indices[i];
            int row = idx / cols;
            int col = idx % cols;

            sum += new Vector3(
                _boardRenderer.BoardOrigin.x + col * _boardRenderer.CellSize,
                _boardRenderer.BoardOrigin.y - row * _boardRenderer.CellSize,
                0f
            );
        }

        return sum / count;
    }

    #endregion

    #region Public API

    /// <summary>
    /// Get current board state (for debugging/UI)
    /// </summary>
    public BoardManager GetBoardManager() => _boardManager;

    /// <summary>
    /// Check if game is ready to receive input
    /// </summary>
    public bool IsReady => _currentState == GameState.Playing;

    #endregion
}

/// <summary>
/// Game state enumeration
/// </summary>
public enum GameState
{
    Initializing,
    Playing,
    Animating,
    Shuffling,
    Paused,
    GameOver
}
