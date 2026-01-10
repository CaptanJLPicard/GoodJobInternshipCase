using UnityEngine;
using MoreMountains.Feedbacks;

namespace BlastPuzzle.Feedback
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

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _blastSound;
        [SerializeField] private AudioClip _landSound;
        [SerializeField] private AudioClip _shuffleSound;

        [Header("Settings")]
        [Tooltip("Group size threshold for big group feedback")]
        [SerializeField] private int _bigGroupThreshold = 5;

        [Tooltip("Volume for sound effects")]
        [Range(0f, 1f)]
        [SerializeField] private float _sfxVolume = 0.7f;

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

            // Big group bonus feedback
            if (groupSize > _bigGroupThreshold && _bigGroupFeedback != null)
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
    }
}
