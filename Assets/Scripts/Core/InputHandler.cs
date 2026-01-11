// ============================================================================
// Made By Hakan Emre ÖZKAN
// For more follow my itch.io account (Heodev) - Begin to begin
// ============================================================================

using System;
using UnityEngine;

namespace GoodJobInternshipCase.Core
{
    /// <summary>
    /// Handles player input for both mobile (touch) and desktop (mouse).
    /// Optimized for mobile with proper touch handling.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Camera _mainCamera;

        [Header("Mobile Settings")]
        [Tooltip("Prevent multi-touch - only first finger is processed")]
        [SerializeField] private bool _singleTouchOnly = true;

        [Tooltip("Minimum drag distance to be considered a swipe (ignore small movements)")]
        [SerializeField] private float _tapThreshold = 10f;

        private bool _isEnabled = true;
        private bool _wasPressed;
        private int _activeTouchId = -1;
        private Vector2 _touchStartPos;
        private bool _isMobile;

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

            // Detect platform
            _isMobile = Application.isMobilePlatform;

            // Mobile optimization: Set target frame rate
            if (_isMobile)
            {
                Application.targetFrameRate = 60;
            }
        }

        private void Update()
        {
            if (!_isEnabled || _mainCamera == null)
                return;

            if (_isMobile || Input.touchCount > 0)
            {
                HandleTouchInput();
            }
            else
            {
                HandleMouseInput();
            }
        }

        /// <summary>
        /// Handle touch input for mobile devices
        /// </summary>
        private void HandleTouchInput()
        {
            if (Input.touchCount == 0)
            {
                if (_wasPressed)
                {
                    _wasPressed = false;
                    _activeTouchId = -1;
                }
                return;
            }

            Touch touch;

            // Single touch mode - only track one finger
            if (_singleTouchOnly)
            {
                if (_activeTouchId >= 0)
                {
                    // Find our tracked touch
                    bool found = false;
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        if (Input.GetTouch(i).fingerId == _activeTouchId)
                        {
                            touch = Input.GetTouch(i);
                            found = true;
                            ProcessTouch(touch);
                            break;
                        }
                    }

                    if (!found)
                    {
                        // Touch ended
                        _wasPressed = false;
                        _activeTouchId = -1;
                    }
                }
                else
                {
                    // Start tracking first touch
                    touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Began)
                    {
                        _activeTouchId = touch.fingerId;
                        _touchStartPos = touch.position;
                        ProcessTouch(touch);
                    }
                }
            }
            else
            {
                // Multi-touch mode - process first touch
                touch = Input.GetTouch(0);
                ProcessTouch(touch);
            }
        }

        /// <summary>
        /// Process a single touch event
        /// </summary>
        private void ProcessTouch(Touch touch)
        {
            Vector3 worldPos = ScreenToWorld(touch.position);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _wasPressed = true;
                    _touchStartPos = touch.position;
                    OnClick?.Invoke(worldPos);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (_wasPressed)
                    {
                        OnHold?.Invoke(worldPos);
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (_wasPressed)
                    {
                        // Check if it was a tap (not a drag)
                        float dragDistance = Vector2.Distance(touch.position, _touchStartPos);
                        if (dragDistance < _tapThreshold)
                        {
                            // It's a tap - trigger release at tap position
                            OnRelease?.Invoke(worldPos);
                        }
                        _wasPressed = false;
                        _activeTouchId = -1;
                    }
                    break;
            }
        }

        /// <summary>
        /// Handle mouse input for desktop/editor
        /// </summary>
        private void HandleMouseInput()
        {
            bool isPressed = Input.GetMouseButton(0);

            if (!isPressed && !_wasPressed)
                return;

            Vector3 inputPosition = Input.mousePosition;

            // Validate screen position
            if (inputPosition.x < 0 || inputPosition.x > Screen.width ||
                inputPosition.y < 0 || inputPosition.y > Screen.height)
            {
                _wasPressed = isPressed;
                return;
            }

            Vector3 worldPos = ScreenToWorld(inputPosition);

            if (isPressed && !_wasPressed)
            {
                OnClick?.Invoke(worldPos);
            }
            else if (isPressed && _wasPressed)
            {
                OnHold?.Invoke(worldPos);
            }
            else if (!isPressed && _wasPressed)
            {
                OnRelease?.Invoke(worldPos);
            }

            _wasPressed = isPressed;
        }

        /// <summary>
        /// Convert screen position to world position
        /// </summary>
        private Vector3 ScreenToWorld(Vector2 screenPos)
        {
            Vector3 pos = new Vector3(screenPos.x, screenPos.y, Mathf.Abs(_mainCamera.transform.position.z));
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(pos);
            worldPos.z = 0f;
            return worldPos;
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
            _activeTouchId = -1;
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
