// ============================================================================
// Made By Hakan Emre ÖZKAN
// For more follow my itch.io account (Heodev) - To begin, begin
// ============================================================================

using UnityEngine;
using UnityEngine.Rendering;

namespace GoodJobInternshipCase.Core
{
    /// <summary>
    /// Aggressive mobile optimizations for low-end devices like Huawei P9 Lite.
    /// Targets 60 FPS on Mali-T830 / Kirin 650 class devices.
    /// </summary>
    public class MobileOptimizer : MonoBehaviour
    {
        [Header("Performance Profile")]
        [Tooltip("Select device performance tier")]
        [SerializeField] private PerformanceTier _performanceTier = PerformanceTier.Low;

        [Header("Frame Rate")]
        [Tooltip("Target frame rate")]
        [SerializeField] private int _targetFrameRate = 60;

        [Header("Rendering")]
        [Tooltip("Reduce render resolution for performance")]
        [Range(0.5f, 1f)]
        [SerializeField] private float _renderScale = 0.85f;

        [Tooltip("Disable anti-aliasing for performance")]
        [SerializeField] private bool _disableAntiAliasing = true;

        [Tooltip("Reduce texture quality (0=full, 1=half, 2=quarter)")]
        [Range(0, 2)]
        [SerializeField] private int _textureMipmapLimit = 1;

        [Header("Quality")]
        [Tooltip("Use simplified shaders")]
        [SerializeField] private bool _useSimplifiedShaders = true;

        [Tooltip("Disable shadows completely")]
        [SerializeField] private bool _disableShadows = true;

        [Tooltip("Disable soft particles")]
        [SerializeField] private bool _disableSoftParticles = true;

        [Header("Memory")]
        [Tooltip("Run GC every N seconds (0 to disable)")]
        [SerializeField] private float _gcInterval = 30f;

        [Tooltip("Prevent screen sleep")]
        [SerializeField] private bool _preventScreenSleep = true;

        // Internals
        private float _gcTimer;
        private static MobileOptimizer _instance;

        public static MobileOptimizer Instance => _instance;

        public enum PerformanceTier
        {
            Low,      // Huawei P9 Lite, old devices (aggressive optimization)
            Medium,   // Mid-range devices
            High      // Flagship devices
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            ApplyAllOptimizations();
        }

        private void Update()
        {
            // Periodic GC for low-end devices
            if (_gcInterval > 0f && _performanceTier == PerformanceTier.Low)
            {
                _gcTimer += Time.unscaledDeltaTime;
                if (_gcTimer >= _gcInterval)
                {
                    _gcTimer = 0f;
                    PerformLightweightGC();
                }
            }
        }

        private void ApplyAllOptimizations()
        {
            ApplyFrameRateSettings();
            ApplyRenderingOptimizations();
            ApplyQualitySettings();
            ApplyMemoryOptimizations();
            ApplyPlatformSpecificSettings();

            Debug.Log($"[MobileOptimizer] Applied {_performanceTier} tier optimizations - Target: {_targetFrameRate} FPS");
        }

        private void ApplyFrameRateSettings()
        {
            // Target frame rate
            Application.targetFrameRate = _targetFrameRate;

            // Disable VSync for consistent frame rate control
            QualitySettings.vSyncCount = 0;

            // Optimize fixed timestep (no physics needed in this game)
            Time.fixedDeltaTime = 1f / 30f;
            Time.maximumDeltaTime = 1f / 10f;
        }

        private void ApplyRenderingOptimizations()
        {
            // Anti-aliasing
            if (_disableAntiAliasing)
            {
                QualitySettings.antiAliasing = 0;
            }

            // Texture quality
            QualitySettings.globalTextureMipmapLimit = _textureMipmapLimit;

            // Anisotropic filtering - disable for performance
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;

            // Shadows
            if (_disableShadows)
            {
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.shadowResolution = ShadowResolution.Low;
                QualitySettings.shadowDistance = 0f;
            }

            // Soft particles
            QualitySettings.softParticles = !_disableSoftParticles;

            // LOD bias - favor lower LODs
            QualitySettings.lodBias = 0.5f;

            // Reduce pixel light count
            QualitySettings.pixelLightCount = 0;

            // Disable realtime reflection probes
            QualitySettings.realtimeReflectionProbes = false;

            // Billboards - use lower quality
            QualitySettings.billboardsFaceCameraPosition = false;

            // Particle raycast budget
            QualitySettings.particleRaycastBudget = 16;

            // Async upload (reduce CPU spikes)
            QualitySettings.asyncUploadTimeSlice = 2;
            QualitySettings.asyncUploadBufferSize = 4;
        }

        private void ApplyQualitySettings()
        {
            // Set quality level based on tier
            int qualityLevel = _performanceTier switch
            {
                PerformanceTier.Low => 0,
                PerformanceTier.Medium => 1,
                PerformanceTier.High => 2,
                _ => 0
            };

            // Don't change quality level if it would override our manual settings
            // QualitySettings.SetQualityLevel(qualityLevel, false);

            // Skin weights - minimal for 2D game
            QualitySettings.skinWeights = SkinWeights.OneBone;

            // Resolution scaling
            if (_performanceTier == PerformanceTier.Low && _renderScale < 1f)
            {
                // Note: This affects UI, so be careful
                // Screen.SetResolution((int)(Screen.width * _renderScale), (int)(Screen.height * _renderScale), Screen.fullScreen);
            }
        }

        private void ApplyMemoryOptimizations()
        {
            // Prevent screen sleep
            if (_preventScreenSleep)
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            // Multi-touch disabled (single touch game)
            Input.multiTouchEnabled = false;

            // Gyro/accelerometer disabled
            Input.gyro.enabled = false;
            Input.compensateSensors = false;

            // Register for low memory callback
            Application.lowMemory += OnLowMemory;

            // Initial cleanup
            if (_performanceTier == PerformanceTier.Low)
            {
                // Pre-warm and clean
                Resources.UnloadUnusedAssets();
            }
        }

        private void ApplyPlatformSpecificSettings()
        {
            if (!Application.isMobilePlatform)
                return;

            // Android-specific
#if UNITY_ANDROID
            // Disable multi-threaded rendering on very old devices
            // (Can cause issues on Mali-T830)
            // This is set in Player Settings, can't change at runtime

            // Optimize for battery when in background
            Application.runInBackground = false;
#endif

            // iOS-specific
#if UNITY_IOS
            // iOS handles most optimizations automatically
            Application.runInBackground = false;
#endif
        }

        private void PerformLightweightGC()
        {
            // Only collect generation 0 (young generation) - fast and non-blocking
            System.GC.Collect(0, System.GCCollectionMode.Optimized, false);
        }

        private void OnLowMemory()
        {
            Debug.LogWarning("[MobileOptimizer] LOW MEMORY - Emergency cleanup!");

            // Aggressive cleanup
            Resources.UnloadUnusedAssets();
            System.GC.Collect();

            // Force texture compression
            QualitySettings.globalTextureMipmapLimit = 2;
        }

        private void OnDestroy()
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
            Application.lowMemory -= OnLowMemory;

            if (_instance == this)
                _instance = null;
        }

        /// <summary>
        /// Apply low-end device preset
        /// </summary>
        [ContextMenu("Apply Low-End Preset")]
        public void ApplyLowEndPreset()
        {
            _performanceTier = PerformanceTier.Low;
            _targetFrameRate = 60;
            _renderScale = 0.75f;
            _disableAntiAliasing = true;
            _textureMipmapLimit = 1;
            _useSimplifiedShaders = true;
            _disableShadows = true;
            _disableSoftParticles = true;
            _gcInterval = 30f;

            if (Application.isPlaying)
                ApplyAllOptimizations();
        }

        /// <summary>
        /// Set performance tier at runtime
        /// </summary>
        public void SetPerformanceTier(PerformanceTier tier)
        {
            _performanceTier = tier;
            ApplyAllOptimizations();
        }

        /// <summary>
        /// Force garbage collection (use sparingly)
        /// </summary>
        public void ForceGC()
        {
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                ApplyAllOptimizations();
            }
        }

        [ContextMenu("Log Current Settings")]
        private void LogCurrentSettings()
        {
            Debug.Log($"=== Mobile Optimizer Settings ===\n" +
                $"Tier: {_performanceTier}\n" +
                $"Target FPS: {_targetFrameRate}\n" +
                $"VSync: {QualitySettings.vSyncCount}\n" +
                $"AA: {QualitySettings.antiAliasing}\n" +
                $"Shadows: {QualitySettings.shadows}\n" +
                $"Texture Limit: {QualitySettings.globalTextureMipmapLimit}\n" +
                $"Aniso: {QualitySettings.anisotropicFiltering}");
        }
#endif
    }
}
