using UnityEngine;

namespace GoodJobInternshipCase.Config
{
    /// <summary>
    /// Threshold preset options for group icon states
    /// </summary>
    public enum ThresholdPreset
    {
        Easy,      // A=3, B=5, C=8
        Medium,    // A=4, B=7, C=10
        Hard,      // A=5, B=9, C=14
        Custom     // User-defined values
    }

    /// <summary>
    /// ScriptableObject containing all game configuration parameters.
    /// Create via Assets > Create > GoodJobInternshipCase > Game Config
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "GoodJobInternshipCase/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Board Settings")]
        [Tooltip("Number of rows (M)")]
        [Range(2, 10)]
        public int Rows = 8;

        [Tooltip("Number of columns (N)")]
        [Range(2, 10)]
        public int Columns = 8;

        [Tooltip("Number of different colors (K)")]
        [Range(1, 6)]
        public int ColorCount = 4;

        [Header("Threshold Settings")]
        [Tooltip("Select a preset or choose Custom for manual values")]
        public ThresholdPreset ThresholdPreset = ThresholdPreset.Easy;

        [Tooltip("Group size threshold for A icon (only used when preset is Custom)")]
        [Range(2, 20)]
        public int ThresholdA = 3;

        [Tooltip("Group size threshold for B icon (only used when preset is Custom)")]
        [Range(2, 30)]
        public int ThresholdB = 5;

        [Tooltip("Group size threshold for C icon (only used when preset is Custom)")]
        [Range(2, 50)]
        public int ThresholdC = 8;

        [Header("Animation Timings")]
        [Tooltip("Duration of block falling animation")]
        public float FallDuration = 0.3f;

        [Tooltip("Duration of new block spawn animation")]
        public float SpawnDuration = 0.25f;

        [Tooltip("Duration of block blast animation")]
        public float BlastDuration = 0.2f;

        [Tooltip("Duration of shuffle animation")]
        public float ShuffleDuration = 0.5f;

        [Header("Visual Settings")]
        [Tooltip("Size of each cell in world units")]
        public float CellSize = 1f;

        [Tooltip("Spacing between cells")]
        public float CellSpacing = 0.1f;

        [Header("Sprites - Assign 6 colors, each with 4 states (Default, A, B, C)")]
        public BlockSpriteSet[] ColorSprites = new BlockSpriteSet[6];

        [Header("Audio (Optional - null-safe)")]
        public AudioClip BlastSound;
        public AudioClip LandSound;
        public AudioClip ShuffleSound;

        /// <summary>
        /// Get the effective threshold A value based on preset
        /// </summary>
        public int GetThresholdA()
        {
            return ThresholdPreset switch
            {
                ThresholdPreset.Easy => 3,
                ThresholdPreset.Medium => 4,
                ThresholdPreset.Hard => 5,
                _ => ThresholdA
            };
        }

        /// <summary>
        /// Get the effective threshold B value based on preset
        /// </summary>
        public int GetThresholdB()
        {
            return ThresholdPreset switch
            {
                ThresholdPreset.Easy => 5,
                ThresholdPreset.Medium => 7,
                ThresholdPreset.Hard => 9,
                _ => ThresholdB
            };
        }

        /// <summary>
        /// Get the effective threshold C value based on preset
        /// </summary>
        public int GetThresholdC()
        {
            return ThresholdPreset switch
            {
                ThresholdPreset.Easy => 8,
                ThresholdPreset.Medium => 10,
                ThresholdPreset.Hard => 14,
                _ => ThresholdC
            };
        }

        /// <summary>
        /// Calculate icon state based on group size
        /// </summary>
        public byte CalculateIconState(int groupSize)
        {
            if (groupSize > GetThresholdC()) return 3; // C icon
            if (groupSize > GetThresholdB()) return 2; // B icon
            if (groupSize > GetThresholdA()) return 1; // A icon
            return 0; // Default icon
        }

        /// <summary>
        /// Get total cell size including spacing
        /// </summary>
        public float TotalCellSize => CellSize + CellSpacing;

        /// <summary>
        /// Get board dimensions in world units
        /// </summary>
        public Vector2 BoardWorldSize => new Vector2(
            Columns * TotalCellSize - CellSpacing,
            Rows * TotalCellSize - CellSpacing
        );

        /// <summary>
        /// Get maximum possible pool size (for pre-allocation)
        /// </summary>
        public int MaxPoolSize => (int)(Rows * Columns * 1.5f);

        private void OnValidate()
        {
            // Ensure thresholds are in ascending order for custom mode
            if (ThresholdPreset == ThresholdPreset.Custom)
            {
                ThresholdB = Mathf.Max(ThresholdB, ThresholdA + 1);
                ThresholdC = Mathf.Max(ThresholdC, ThresholdB + 1);
            }

            // Ensure at least 2 colors for gameplay
            ColorCount = Mathf.Max(2, ColorCount);
        }
    }

    /// <summary>
    /// Sprite set for a single color with all icon states
    /// </summary>
    [System.Serializable]
    public struct BlockSpriteSet
    {
        [Tooltip("Color name for reference")]
        public string ColorName;

        [Tooltip("Default state sprite")]
        public Sprite DefaultSprite;

        [Tooltip("A state sprite (medium group)")]
        public Sprite SpriteA;

        [Tooltip("B state sprite (large group)")]
        public Sprite SpriteB;

        [Tooltip("C state sprite (very large group)")]
        public Sprite SpriteC;

        /// <summary>
        /// Get sprite for given icon state
        /// </summary>
        public readonly Sprite GetSprite(int iconState)
        {
            return iconState switch
            {
                1 => SpriteA ?? DefaultSprite,
                2 => SpriteB ?? DefaultSprite,
                3 => SpriteC ?? DefaultSprite,
                _ => DefaultSprite
            };
        }

        /// <summary>
        /// Check if this sprite set is properly configured
        /// </summary>
        public readonly bool IsValid => DefaultSprite != null;
    }
}
