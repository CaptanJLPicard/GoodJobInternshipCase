// ============================================================================
// Made By Hakan Emre ÖZKAN
// For more follow my itch.io account (Heodev) - Begin to begin
// ============================================================================

using System;
using UnityEngine;
using GoodJobInternshipCase.Config;

namespace GoodJobInternshipCase.Rendering
{
    /// <summary>
    /// Visual representation of a single block on the board.
    /// Handles sprite display and animations using Update-based approach (no coroutines).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BlockVisual : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private Transform _transform;

        // Pooling
        public int PoolIndex { get; set; }
        public int BoardIndex { get; set; }

        // Sprite data
        private BlockSpriteSet _currentSpriteSet;
        private byte _currentColorIndex;
        private byte _currentIconState;

        // Animation state
        private Vector3 _startPos;
        private Vector3 _targetPos;
        private Vector3 _startScale;
        private Vector3 _targetScale;
        private float _animationTime;
        private float _animationDuration;
        private AnimationType _currentAnimation;
        private Color _originalColor;

        public bool IsAnimating => _currentAnimation != AnimationType.None;
        public event Action<BlockVisual> OnAnimationComplete;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _transform = transform;
            _originalColor = Color.white;

            // Optimization: Disable unnecessary rendering features
            _spriteRenderer.receiveShadows = false;
            _spriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _spriteRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            _spriteRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        }

        /// <summary>
        /// Set block color and sprite set
        /// </summary>
        public void SetColor(byte colorIndex, BlockSpriteSet spriteSet)
        {
            _currentColorIndex = colorIndex;
            _currentSpriteSet = spriteSet;
            _currentIconState = 0;

            if (spriteSet.IsValid)
            {
                _spriteRenderer.sprite = spriteSet.DefaultSprite;
            }
        }

        /// <summary>
        /// Update icon state based on group size
        /// </summary>
        public void SetIconState(byte iconState)
        {
            if (_currentIconState == iconState)
                return;

            _currentIconState = iconState;
            _spriteRenderer.sprite = _currentSpriteSet.GetSprite(iconState);
        }

        /// <summary>
        /// Set world position immediately
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            _transform.position = position;
        }

        /// <summary>
        /// Reset visual state for pooling
        /// </summary>
        public void ResetVisual()
        {
            _currentAnimation = AnimationType.None;
            _transform.localScale = Vector3.one;
            _spriteRenderer.color = _originalColor;
            BoardIndex = -1;
        }

        #region Animations

        /// <summary>
        /// Start falling animation
        /// </summary>
        public void StartFallAnimation(Vector3 from, Vector3 to, float duration)
        {
            _startPos = from;
            _targetPos = to;
            _animationTime = 0f;
            _animationDuration = duration;
            _currentAnimation = AnimationType.Fall;
            _transform.position = from;
        }

        /// <summary>
        /// Start spawn animation (scale up + fall)
        /// </summary>
        public void StartSpawnAnimation(Vector3 spawnPos, Vector3 targetPos, float duration)
        {
            _startPos = spawnPos;
            _targetPos = targetPos;
            _startScale = Vector3.zero;
            _targetScale = Vector3.one;
            _animationTime = 0f;
            _animationDuration = duration;
            _currentAnimation = AnimationType.Spawn;
            _transform.position = spawnPos;
            _transform.localScale = Vector3.zero;
        }

        /// <summary>
        /// Start blast animation (scale down + fade)
        /// </summary>
        public void StartBlastAnimation(float duration)
        {
            _startScale = Vector3.one;
            _targetScale = Vector3.zero;
            _animationTime = 0f;
            _animationDuration = duration;
            _currentAnimation = AnimationType.Blast;
        }

        /// <summary>
        /// Start shuffle animation (quick scale bounce)
        /// </summary>
        public void StartShuffleAnimation(float duration)
        {
            _startScale = Vector3.one;
            _animationTime = 0f;
            _animationDuration = duration;
            _currentAnimation = AnimationType.Shuffle;
        }

        /// <summary>
        /// Update animation (call from BoardRenderer.Update)
        /// </summary>
        public void UpdateAnimation(float deltaTime)
        {
            if (_currentAnimation == AnimationType.None)
                return;

            _animationTime += deltaTime;
            float t = Mathf.Clamp01(_animationTime / _animationDuration);

            switch (_currentAnimation)
            {
                case AnimationType.Fall:
                    UpdateFallAnimation(t);
                    break;

                case AnimationType.Spawn:
                    UpdateSpawnAnimation(t);
                    break;

                case AnimationType.Blast:
                    UpdateBlastAnimation(t);
                    break;

                case AnimationType.Shuffle:
                    UpdateShuffleAnimation(t);
                    break;
            }

            if (t >= 1f)
            {
                CompleteAnimation();
            }
        }

        private void UpdateFallAnimation(float t)
        {
            float easedT = EaseOutBounce(t);
            _transform.position = Vector3.LerpUnclamped(_startPos, _targetPos, easedT);
        }

        private void UpdateSpawnAnimation(float t)
        {
            float scaleT = EaseOutBack(t);
            float posT = EaseOutQuad(t);

            _transform.localScale = Vector3.LerpUnclamped(_startScale, _targetScale, scaleT);
            _transform.position = Vector3.LerpUnclamped(_startPos, _targetPos, posT);
        }

        private void UpdateBlastAnimation(float t)
        {
            float inverseT = 1f - t;
            _transform.localScale = Vector3.one * inverseT;

            Color c = _originalColor;
            c.a = inverseT;
            _spriteRenderer.color = c;
        }

        private void UpdateShuffleAnimation(float t)
        {
            // Scale punch effect
            float scale;
            if (t < 0.5f)
            {
                // Scale down
                scale = Mathf.Lerp(1f, 0.7f, t * 2f);
            }
            else
            {
                // Scale up with overshoot
                scale = Mathf.Lerp(0.7f, 1f, (t - 0.5f) * 2f);
                scale *= 1f + Mathf.Sin((t - 0.5f) * Mathf.PI * 2f) * 0.1f;
            }
            _transform.localScale = Vector3.one * scale;
        }

        private void CompleteAnimation()
        {
            AnimationType completedType = _currentAnimation;
            _currentAnimation = AnimationType.None;

            // Reset state after animation
            switch (completedType)
            {
                case AnimationType.Fall:
                    _transform.position = _targetPos;
                    break;

                case AnimationType.Spawn:
                    _transform.localScale = Vector3.one;
                    _transform.position = _targetPos;
                    break;

                case AnimationType.Shuffle:
                    _transform.localScale = Vector3.one;
                    break;
            }

            OnAnimationComplete?.Invoke(this);
        }

        #endregion

        #region Easing Functions

        private static float EaseOutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1)
            {
                return n1 * t * t;
            }
            else if (t < 2f / d1)
            {
                t -= 1.5f / d1;
                return n1 * t * t + 0.75f;
            }
            else if (t < 2.5f / d1)
            {
                t -= 2.25f / d1;
                return n1 * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / d1;
                return n1 * t * t + 0.984375f;
            }
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private static float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        #endregion

        public enum AnimationType
        {
            None,
            Fall,
            Spawn,
            Blast,
            Shuffle
        }
    }
}
