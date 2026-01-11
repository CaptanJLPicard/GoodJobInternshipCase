// ============================================================================
// Made By Hakan Emre ÖZKAN
// For more follow my itch.io account (Heodev) - To begin, begin
// ============================================================================

using System;
using System.Text;
using UnityEngine;
using TMPro;

namespace GoodJobInternshipCase.Feedback
{
    /// <summary>
    /// Animated score popup that shows points gained.
    /// Uses Update-based animation for optimal performance (no coroutines).
    /// Optimized for low-end mobile devices.
    /// </summary>
    public class ScorePopup : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float _duration = 1f;
        [SerializeField] private float _moveDistance = 100f;
        [SerializeField] private float _startScale = 0.5f;
        [SerializeField] private float _peakScale = 1.2f;
        [SerializeField] private float _endScale = 0.8f;

        [Header("Screen Bounds")]
        [Tooltip("Minimum margin from screen edges")]
        [SerializeField] private float _screenMargin = 80f;

        // Animation state
        private RectTransform _rectTransform;
        private Vector2 _startPosition;
        private float _animationTime;
        private bool _isAnimating;
        private int _poolIndex;

        // Cached values for performance
        private static readonly StringBuilder s_stringBuilder = new StringBuilder(16);
        private static readonly Vector3 s_one = Vector3.one;

        // Events
        public event Action<ScorePopup> OnAnimationComplete;
        public int PoolIndex { get => _poolIndex; set => _poolIndex = value; }
        public bool IsAnimating => _isAnimating;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();

            // Auto-find components if not assigned
            if (_text == null)
                _text = GetComponentInChildren<TextMeshProUGUI>();

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        /// <summary>
        /// Show score popup with animation
        /// </summary>
        public void Show(int score, Vector2 screenPosition, Color color)
        {
            // Set text using StringBuilder to avoid allocation
            s_stringBuilder.Clear();
            s_stringBuilder.Append('+');
            s_stringBuilder.Append(score);
            _text.SetText(s_stringBuilder);
            _text.color = color;

            // Clamp position to screen bounds
            Vector2 clampedPos = ClampToScreenBounds(screenPosition);

            // Set position
            _startPosition = clampedPos;
            _rectTransform.anchoredPosition = clampedPos;

            // Reset state
            _animationTime = 0f;
            _isAnimating = true;
            _canvasGroup.alpha = 1f;
            _rectTransform.localScale = s_one * _startScale;

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Show with world position (converted to screen)
        /// </summary>
        public void Show(int score, Vector3 worldPosition, Camera camera, Color color)
        {
            Vector2 screenPos = camera.WorldToScreenPoint(worldPosition);
            Show(score, screenPos, color);
        }

        /// <summary>
        /// Clamp screen position to stay within visible screen bounds
        /// </summary>
        private Vector2 ClampToScreenBounds(Vector2 screenPosition)
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // Clamp X position (left and right edges)
            float clampedX = Mathf.Clamp(
                screenPosition.x,
                _screenMargin,
                screenWidth - _screenMargin
            );

            // Clamp Y position (bottom and top edges)
            // Also account for upward movement during animation
            float clampedY = Mathf.Clamp(
                screenPosition.y,
                _screenMargin,
                screenHeight - _screenMargin - _moveDistance
            );

            return new Vector2(clampedX, clampedY);
        }

        private void Update()
        {
            if (!_isAnimating)
                return;

            _animationTime += Time.deltaTime;
            float t = Mathf.Clamp01(_animationTime / _duration);

            // Position - ease out quad movement upward
            float moveT = EaseOutQuad(t);
            Vector2 pos = _startPosition + Vector2.up * (_moveDistance * moveT);
            _rectTransform.anchoredPosition = pos;

            // Scale - punch effect
            float scale;
            if (t < 0.2f)
            {
                // Scale up to peak (0 -> 0.2)
                float scaleT = t / 0.2f;
                scale = Mathf.Lerp(_startScale, _peakScale, EaseOutBack(scaleT));
            }
            else if (t < 0.4f)
            {
                // Scale down from peak to normal (0.2 -> 0.4)
                float scaleT = (t - 0.2f) / 0.2f;
                scale = Mathf.Lerp(_peakScale, 1f, EaseOutQuad(scaleT));
            }
            else
            {
                // Slowly scale down (0.4 -> 1.0)
                float scaleT = (t - 0.4f) / 0.6f;
                scale = Mathf.Lerp(1f, _endScale, scaleT);
            }
            _rectTransform.localScale = Vector3.one * scale;

            // Alpha - fade out in last 40%
            if (t > 0.6f)
            {
                float alphaT = (t - 0.6f) / 0.4f;
                _canvasGroup.alpha = 1f - EaseInQuad(alphaT);
            }

            // Complete
            if (t >= 1f)
            {
                _isAnimating = false;
                gameObject.SetActive(false);
                OnAnimationComplete?.Invoke(this);
            }
        }

        /// <summary>
        /// Reset for pooling
        /// </summary>
        public void ResetPopup()
        {
            _isAnimating = false;
            _animationTime = 0f;
            _canvasGroup.alpha = 1f;
            _rectTransform.localScale = s_one;
            gameObject.SetActive(false);
        }

        #region Easing Functions

        private static float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        private static float EaseInQuad(float t)
        {
            return t * t;
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        #endregion
    }
}
