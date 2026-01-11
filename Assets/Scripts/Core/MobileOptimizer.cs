// ============================================================================
// Made By Hakan Emre ÖZKAN
// For more follow my itch.io account (Heodev) - Begin to begin
// ============================================================================

using UnityEngine;

namespace GoodJobInternshipCase.Core
{
    /// <summary>
    /// Applies mobile-specific optimizations at runtime.
    /// Attach to a GameObject in the scene for automatic optimization.
    /// </summary>
    public class MobileOptimizer : MonoBehaviour
    {
        [Header("Frame Rate")]
        [Tooltip("Target frame rate for mobile (30 or 60 recommended)")]
        [SerializeField] private int _targetFrameRate = 60;

        [Header("Quality Settings")]
        [Tooltip("Enable VSync on mobile")]
        [SerializeField] private bool _enableVSync = false;

        [Tooltip("Disable screen dimming during gameplay")]
        [SerializeField] private bool _preventScreenSleep = true;

        [Header("Memory")]
        [Tooltip("Enable incremental GC for smoother performance")]
        [SerializeField] private bool _useIncrementalGC = true;

        [Header("Multi-touch")]
        [Tooltip("Enable multi-touch input")]
        [SerializeField] private bool _enableMultiTouch = false;

        private void Awake()
        {
            ApplyOptimizations();
        }

        private void ApplyOptimizations()
        {
            // Frame rate
            Application.targetFrameRate = _targetFrameRate;

            // VSync
            QualitySettings.vSyncCount = _enableVSync ? 1 : 0;

            // Prevent screen from dimming/sleeping during gameplay
            if (_preventScreenSleep)
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            // Multi-touch setting
            Input.multiTouchEnabled = _enableMultiTouch;

            // Mobile-specific optimizations
            if (Application.isMobilePlatform)
            {
                ApplyMobileSpecificSettings();
            }
        }

        private void ApplyMobileSpecificSettings()
        {
            // Reduce texture quality on low-end devices if needed
            // QualitySettings.globalTextureMipmapLimit = 1;

            // Enable GPU skinning for better performance
            // QualitySettings.skinWeights = SkinWeights.TwoBones;

            // Optimize for battery
            // Application.lowMemoryCallback += OnLowMemory;

            // Set fixed timestep for consistent physics (if using physics)
            // Time.fixedDeltaTime = 1f / 30f;

            Debug.Log($"[MobileOptimizer] Applied mobile optimizations - Target FPS: {_targetFrameRate}");
        }

        /// <summary>
        /// Handle low memory warning
        /// </summary>
        private void OnLowMemory()
        {
            Debug.LogWarning("[MobileOptimizer] Low memory warning - clearing unused assets");
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        private void OnDestroy()
        {
            // Restore screen sleep when leaving game
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }

        /// <summary>
        /// Set target frame rate at runtime
        /// </summary>
        public void SetTargetFrameRate(int fps)
        {
            _targetFrameRate = fps;
            Application.targetFrameRate = fps;
        }

        /// <summary>
        /// Enable/disable multi-touch at runtime
        /// </summary>
        public void SetMultiTouchEnabled(bool enabled)
        {
            _enableMultiTouch = enabled;
            Input.multiTouchEnabled = enabled;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                ApplyOptimizations();
            }
        }
#endif
    }
}
