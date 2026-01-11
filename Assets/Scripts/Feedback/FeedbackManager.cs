// ============================================================================
// Made By Hakan Emre ÖZKAN
// For more follow my itch.io account (Heodev) - Begin to begin
// ============================================================================

using System;
using UnityEngine;
using MoreMountains.Feedbacks;
using GoodJobInternshipCase.Config;

namespace GoodJobInternshipCase.Feedback
{
    /// <summary>
    /// Manages game feel and feedback effects using the Feel framework.
    /// All references are null-safe - the game works without any feedbacks assigned.
    /// </summary>
    public class FeedbackManager : MonoBehaviour
    {
        [Header("Blast Feedbacks")]
        [Tooltip("Feedback for block destruction (scale punch + particles)")]
        [SerializeField] private MMF_Player _blastFeedback;

        [Tooltip("Feedback for big group destruction")]
        [SerializeField] private MMF_Player _bigGroupFeedback;

        [Header("Movement Feedbacks")]
        [Tooltip("Feedback when blocks land after falling")]
        [SerializeField] private MMF_Player _landFeedback;

        [Tooltip("Feedback during shuffle")]
        [SerializeField] private MMF_Player _shuffleFeedback;

        [Header("Special Feedbacks")]
        [Tooltip("Warning feedback when deadlock is detected")]
        [SerializeField] private MMF_Player _deadlockFeedback;

        [Header("Score Popup")]
        [Tooltip("Pool for score popup instances")]
        [SerializeField] private ScorePopupPool _scorePopupPool;

        [Tooltip("Camera for world-to-screen conversion")]
        [SerializeField] private Camera _mainCamera;

        [Tooltip("Color for normal score popup")]
        [SerializeField] private Color _normalScoreColor = Color.white;

        [Tooltip("Color for big group score popup (ThresholdB+)")]
        [SerializeField] private Color _bigScoreColor = Color.yellow;

        [Tooltip("Color for huge group score popup (ThresholdC+)")]
        [SerializeField] private Color _hugeScoreColor = new Color(1f, 0.5f, 0f); // Orange

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _blastSound;
        [SerializeField] private AudioClip _landSound;
        [SerializeField] private AudioClip _shuffleSound;

        [Header("Settings")]
        [Tooltip("Volume for sound effects")]
        [Range(0f, 1f)]
        [SerializeField] private float _sfxVolume = 0.7f;

        // Reference to game config for threshold values
        private GameConfig _config;

        // Event for delayed score addition
        public event Action<int> OnScorePopupComplete;

        private void Awake()
        {
            // Create audio source if not assigned
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
                if (_audioSource == null)
                {
                    _audioSource = gameObject.AddComponent<AudioSource>();
                    _audioSource.playOnAwake = false;
                    _audioSource.spatialBlend = 0f; // 2D sound
                }
            }

            // Get main camera if not assigned
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
        }

        /// <summary>
        /// Initialize feedback manager with game config.
        /// Big group threshold syncs with GameConfig.ThresholdB.
        /// </summary>
        public void Initialize(GameConfig config)
        {
            _config = config;

            // Also set audio clips from config if available
            if (config != null)
            {
                SetAudioClips(config.BlastSound, config.LandSound, config.ShuffleSound);
            }

            // Initialize score popup pool if available
            if (_scorePopupPool != null && !_scorePopupPool.IsInitialized)
            {
                _scorePopupPool.Initialize();
            }
        }

        /// <summary>
        /// Play feedback for block destruction
        /// </summary>
        public void PlayBlastFeedback(Vector3 position, int groupSize)
        {
            // Move feedback to position and play
            if (_blastFeedback != null)
            {
                _blastFeedback.transform.position = position;
                _blastFeedback.PlayFeedbacks();
            }

            // Big group bonus feedback - uses ThresholdB from GameConfig
            int bigGroupThreshold = _config != null ? _config.ThresholdB : 5;
            if (groupSize > bigGroupThreshold && _bigGroupFeedback != null)
            {
                _bigGroupFeedback.PlayFeedbacks();
            }

            // Play sound
            PlaySound(_blastSound);
        }

        /// <summary>
        /// Play feedback when blocks land after falling
        /// </summary>
        public void PlayLandFeedback()
        {
            if (_landFeedback != null)
            {
                _landFeedback.PlayFeedbacks();
            }

            PlaySound(_landSound, 0.5f); // Quieter land sound
        }

        /// <summary>
        /// Play feedback during shuffle
        /// </summary>
        public void PlayShuffleFeedback()
        {
            if (_shuffleFeedback != null)
            {
                _shuffleFeedback.PlayFeedbacks();
            }

            PlaySound(_shuffleSound);
        }

        /// <summary>
        /// Play warning feedback when deadlock is detected
        /// </summary>
        public void PlayDeadlockWarning()
        {
            if (_deadlockFeedback != null)
            {
                _deadlockFeedback.PlayFeedbacks();
            }
        }

        /// <summary>
        /// Stop all active feedbacks
        /// </summary>
        public void StopAllFeedbacks()
        {
            if (_blastFeedback != null) _blastFeedback.StopFeedbacks();
            if (_bigGroupFeedback != null) _bigGroupFeedback.StopFeedbacks();
            if (_landFeedback != null) _landFeedback.StopFeedbacks();
            if (_shuffleFeedback != null) _shuffleFeedback.StopFeedbacks();
            if (_deadlockFeedback != null) _deadlockFeedback.StopFeedbacks();
        }

        /// <summary>
        /// Play audio clip with null safety
        /// </summary>
        private void PlaySound(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (clip != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(clip, _sfxVolume * volumeMultiplier);
            }
        }

        /// <summary>
        /// Set SFX volume at runtime
        /// </summary>
        public void SetVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }

        /// <summary>
        /// Set audio clips from GameConfig
        /// </summary>
        public void SetAudioClips(AudioClip blast, AudioClip land, AudioClip shuffle)
        {
            if (blast != null) _blastSound = blast;
            if (land != null) _landSound = land;
            if (shuffle != null) _shuffleSound = shuffle;
        }

        #region Score Popup

        /// <summary>
        /// Show animated score popup at world position.
        /// Call this before adding score to show "+X" animation.
        /// </summary>
        /// <param name="score">Score amount to display</param>
        /// <param name="worldPosition">World position to spawn popup</param>
        /// <param name="groupSize">Group size for color selection</param>
        public void ShowScorePopup(int score, Vector3 worldPosition, int groupSize)
        {
            if (_scorePopupPool == null || _mainCamera == null)
                return;

            ScorePopup popup = _scorePopupPool.Get();
            if (popup == null)
                return;

            // Determine color based on group size thresholds
            Color popupColor = GetScoreColor(groupSize);

            // Show popup
            popup.Show(score, worldPosition, _mainCamera, popupColor);
        }

        /// <summary>
        /// Show animated score popup at screen position.
        /// </summary>
        public void ShowScorePopup(int score, Vector2 screenPosition, int groupSize)
        {
            if (_scorePopupPool == null)
                return;

            ScorePopup popup = _scorePopupPool.Get();
            if (popup == null)
                return;

            Color popupColor = GetScoreColor(groupSize);
            popup.Show(score, screenPosition, popupColor);
        }

        /// <summary>
        /// Get popup color based on group size and thresholds
        /// </summary>
        private Color GetScoreColor(int groupSize)
        {
            if (_config == null)
                return _normalScoreColor;

            if (groupSize > _config.ThresholdC)
                return _hugeScoreColor;
            else if (groupSize > _config.ThresholdB)
                return _bigScoreColor;
            else
                return _normalScoreColor;
        }

        /// <summary>
        /// Set score popup pool at runtime
        /// </summary>
        public void SetScorePopupPool(ScorePopupPool pool)
        {
            _scorePopupPool = pool;
        }

        /// <summary>
        /// Set camera reference for world-to-screen conversion
        /// </summary>
        public void SetCamera(Camera camera)
        {
            _mainCamera = camera;
        }

        #endregion
    }
}
