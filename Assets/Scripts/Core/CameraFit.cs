// ============================================================================
// Made By Hakan Emre ÖZKAN
// For more follow my itch.io account (Heodev) - Begin to begin
// ============================================================================

using UnityEngine;
using GoodJobInternshipCase.Config;

namespace GoodJobInternshipCase.Core
{
    /// <summary>
    /// Automatically adjusts camera size to fit the game board on any screen aspect ratio.
    /// Also scales background to always cover the entire screen.
    /// Essential for mobile devices with varying screen sizes.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFit : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameConfig _config;

        [Header("Background")]
        [Tooltip("Background sprite transform to scale with camera")]
        [SerializeField] private Transform _background;

        [Tooltip("Original size of background sprite in world units (width, height)")]
        [SerializeField] private Vector2 _backgroundOriginalSize = new Vector2(10f, 10f);

        [Header("Settings")]
        [Tooltip("Extra padding around the board (in world units)")]
        [SerializeField] private float _padding = 1f;

        [Tooltip("Minimum padding at top for UI elements")]
        [SerializeField] private float _topPadding = 2f;

        [Tooltip("Minimum padding at bottom")]
        [SerializeField] private float _bottomPadding = 0.5f;

        private Camera _camera;
        private int _lastScreenWidth;
        private int _lastScreenHeight;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Start()
        {
            FitCamera();
        }

        private void Update()
        {
            // Check for screen size changes (orientation change on mobile)
            if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
            {
                FitCamera();
            }
        }

        /// <summary>
        /// Fit camera to show the entire board
        /// </summary>
        public void FitCamera()
        {
            if (_config == null || _camera == null)
                return;

            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;

            // Calculate board dimensions
            float boardWidth = _config.Columns * _config.TotalCellSize;
            float boardHeight = _config.Rows * _config.TotalCellSize;

            // Add padding
            float targetWidth = boardWidth + _padding * 2f;
            float targetHeight = boardHeight + _topPadding + _bottomPadding;

            // Get screen aspect ratio
            float screenAspect = (float)Screen.width / Screen.height;

            // Calculate required orthographic size
            float sizeForWidth = targetWidth / (2f * screenAspect);
            float sizeForHeight = targetHeight / 2f;

            // Use the larger size to ensure everything fits
            _camera.orthographicSize = Mathf.Max(sizeForWidth, sizeForHeight);

            // Center camera on board (slightly offset up for UI space)
            float yOffset = (_topPadding - _bottomPadding) / 2f;
            _camera.transform.position = new Vector3(0f, -yOffset, _camera.transform.position.z);

            // Scale background to cover entire screen
            ScaleBackground();
        }

        /// <summary>
        /// Scale background to always cover the entire camera view
        /// </summary>
        private void ScaleBackground()
        {
            if (_background == null || _camera == null)
                return;

            // Calculate camera view dimensions in world units
            float cameraHeight = _camera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * _camera.aspect;

            // Add extra margin to ensure no gaps at edges (10% extra)
            float marginMultiplier = 1.1f;
            cameraWidth *= marginMultiplier;
            cameraHeight *= marginMultiplier;

            // Calculate scale needed to cover the screen
            float scaleX = cameraWidth / _backgroundOriginalSize.x;
            float scaleY = cameraHeight / _backgroundOriginalSize.y;

            // Use the larger scale to ensure full coverage (cover mode, not fit mode)
            float uniformScale = Mathf.Max(scaleX, scaleY);

            // Apply scale
            _background.localScale = new Vector3(uniformScale, uniformScale, 1f);

            // Position background at camera center
            Vector3 camPos = _camera.transform.position;
            _background.position = new Vector3(camPos.x, camPos.y, _background.position.z);
        }

        /// <summary>
        /// Set config reference at runtime
        /// </summary>
        public void SetConfig(GameConfig config)
        {
            _config = config;
            FitCamera();
        }

        /// <summary>
        /// Set background reference at runtime
        /// </summary>
        /// <param name="background">Background transform to scale</param>
        /// <param name="originalSize">Original sprite size in world units (before any scaling)</param>
        public void SetBackground(Transform background, Vector2 originalSize)
        {
            _background = background;
            _backgroundOriginalSize = originalSize;
            ScaleBackground();
        }

        /// <summary>
        /// Auto-detect background size from SpriteRenderer
        /// </summary>
        public void SetBackground(Transform background)
        {
            _background = background;

            // Try to auto-detect size from SpriteRenderer
            if (background != null)
            {
                SpriteRenderer sr = background.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    // Get sprite bounds (unscaled size)
                    _backgroundOriginalSize = sr.sprite.bounds.size;
                }
            }

            ScaleBackground();
        }

        /// <summary>
        /// Force recalculate camera fit
        /// </summary>
        public void Recalculate()
        {
            _lastScreenWidth = 0;
            _lastScreenHeight = 0;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Delayed call to avoid issues during serialization
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null && _camera != null)
                {
                    if (_camera == null)
                        _camera = GetComponent<Camera>();

                    FitCamera();
                }
            };
        }
#endif
    }
}
