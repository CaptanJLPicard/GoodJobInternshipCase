using UnityEngine;
using GoodJobInternshipCase.Config;

namespace GoodJobInternshipCase.Core
{
    /// <summary>
    /// Automatically adjusts camera size to fit the game board on any screen aspect ratio.
    /// Essential for mobile devices with varying screen sizes.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFit : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameConfig _config;

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
            if (Application.isPlaying && _camera != null)
            {
                FitCamera();
            }
        }
#endif
    }
}
