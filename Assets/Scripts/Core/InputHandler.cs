using System;
using UnityEngine;

namespace BlastPuzzle.Core
{
    /// <summary>
    /// Handles player input using the old Input System.
    /// Detects clicks/touches and converts to world positions.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private LayerMask _boardLayer = -1;

        private bool _isEnabled = true;
        private bool _wasPressed;

        // Events
        public event Action<Vector3> OnClick;
        public event Action<Vector3> OnHold;
        public event Action<Vector3> OnRelease;

        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (!_isEnabled || _mainCamera == null)
                return;

            // Handle mouse/touch input
            bool isPressed = Input.GetMouseButton(0);

            // Only process if we have a valid press
            if (!isPressed && !_wasPressed)
                return;

            Vector3 inputPosition = Input.mousePosition;

            // Validate screen position is within screen bounds
            if (inputPosition.x < 0 || inputPosition.x > Screen.width ||
                inputPosition.y < 0 || inputPosition.y > Screen.height)
            {
                _wasPressed = isPressed;
                return;
            }

            // Convert to world position
            inputPosition.z = Mathf.Abs(_mainCamera.transform.position.z);
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(inputPosition);
            worldPos.z = 0f;

            if (isPressed && !_wasPressed)
            {
                // Just pressed
                OnClick?.Invoke(worldPos);
            }
            else if (isPressed && _wasPressed)
            {
                // Holding
                OnHold?.Invoke(worldPos);
            }
            else if (!isPressed && _wasPressed)
            {
                // Just released
                OnRelease?.Invoke(worldPos);
            }

            _wasPressed = isPressed;
        }

        /// <summary>
        /// Enable input handling
        /// </summary>
        public void Enable()
        {
            _isEnabled = true;
        }

        /// <summary>
        /// Disable input handling
        /// </summary>
        public void Disable()
        {
            _isEnabled = false;
            _wasPressed = false;
        }

        /// <summary>
        /// Check if input is currently enabled
        /// </summary>
        public bool IsEnabled => _isEnabled;

        /// <summary>
        /// Set camera reference
        /// </summary>
        public void SetCamera(Camera camera)
        {
            _mainCamera = camera;
        }
    }
}
