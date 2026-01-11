// ============================================================================
// Made By Hakan Emre ÖZKAN
// For more follow my itch.io account (Heodev) - To begin, begin
// ============================================================================

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GoodJobInternshipCase.Core
{
    /// <summary>
    /// Runtime URP optimization for low-end mobile devices.
    /// Disables expensive rendering features that aren't needed for 2D puzzle games.
    /// </summary>
    [DefaultExecutionOrder(-100)] // Run before other scripts
    public class URPOptimizer : MonoBehaviour
    {
        [Header("URP Settings")]
        [Tooltip("The URP asset to optimize")]
        [SerializeField] private UniversalRenderPipelineAsset _urpAsset;

        [Header("Optimization Settings")]
        [Tooltip("Keep HDR enabled if using glow/bloom effects")]
        [SerializeField] private bool _disableHDR = false;
        [SerializeField] private bool _disableMSAA = true;
        [SerializeField] private bool _disableShadows = true;
        [SerializeField] private bool _disableDepthTexture = true;
        [SerializeField] private bool _disableOpaqueTexture = true;
        [SerializeField] private bool _reduceShadowQuality = true;

        [Tooltip("Render scale (0.5-1.0) - lower = better performance")]
        [Range(0.5f, 1f)]
        [SerializeField] private float _renderScale = 0.85f;

        private void Awake()
        {
            OptimizeURP();
        }

        private void OptimizeURP()
        {
            // Try to get URP asset from graphics settings if not assigned
            if (_urpAsset == null)
            {
                _urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            }

            if (_urpAsset == null)
            {
                Debug.LogWarning("[URPOptimizer] No URP asset found!");
                return;
            }

            // HDR - not needed for 2D puzzle games
            if (_disableHDR)
            {
                _urpAsset.supportsHDR = false;
            }

            // MSAA - expensive, disable for performance
            if (_disableMSAA)
            {
                _urpAsset.msaaSampleCount = 1; // 1 = disabled
            }

            // Render Scale - reduce resolution for performance
            _urpAsset.renderScale = _renderScale;

            // Shadows - disable via QualitySettings (URP properties are read-only in Unity 6)
            if (_disableShadows)
            {
                QualitySettings.shadows = UnityEngine.ShadowQuality.Disable;
            }

            // Shadow quality (if shadows are still enabled somewhere)
            if (_reduceShadowQuality)
            {
                QualitySettings.shadowDistance = 10f;
                QualitySettings.shadowResolution = UnityEngine.ShadowResolution.Low;
            }

            // Depth and opaque textures - not needed for simple 2D
            if (_disableDepthTexture)
            {
                _urpAsset.supportsCameraDepthTexture = false;
            }

            if (_disableOpaqueTexture)
            {
                _urpAsset.supportsCameraOpaqueTexture = false;
            }

            // Additional optimizations
            _urpAsset.useSRPBatcher = true; // SRP Batcher is good for performance

            Debug.Log($"[URPOptimizer] Applied URP optimizations - RenderScale: {_renderScale}, HDR: {!_disableHDR}, MSAA: {!_disableMSAA}");
        }

        /// <summary>
        /// Apply low-end device preset
        /// </summary>
        [ContextMenu("Apply Low-End Preset")]
        public void ApplyLowEndPreset()
        {
            _disableHDR = false; // Keep HDR for glow effects
            _disableMSAA = true;
            _disableShadows = true;
            _disableDepthTexture = true;
            _disableOpaqueTexture = true;
            _reduceShadowQuality = true;
            _renderScale = 0.75f;

            if (Application.isPlaying)
                OptimizeURP();
        }

        /// <summary>
        /// Apply high-end device preset
        /// </summary>
        [ContextMenu("Apply High-End Preset")]
        public void ApplyHighEndPreset()
        {
            _disableHDR = false;
            _disableMSAA = false;
            _disableShadows = true; // Still disable for 2D
            _disableDepthTexture = true; // Still disable for 2D
            _disableOpaqueTexture = true; // Still disable for 2D
            _reduceShadowQuality = true;
            _renderScale = 1f;

            if (Application.isPlaying)
                OptimizeURP();
        }

        /// <summary>
        /// Set render scale at runtime
        /// </summary>
        public void SetRenderScale(float scale)
        {
            _renderScale = Mathf.Clamp(scale, 0.5f, 1f);
            if (_urpAsset != null)
            {
                _urpAsset.renderScale = _renderScale;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying && _urpAsset != null)
            {
                OptimizeURP();
            }
        }
#endif
    }
}
