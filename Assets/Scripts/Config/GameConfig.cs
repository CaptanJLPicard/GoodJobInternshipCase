// ============================================================================
// Made By Hakan Emre ÖZKAN
// For more follow my itch.io account (Heodev) - Begin to begin
// ============================================================================

using UnityEngine;

namespace GoodJobInternshipCase.Config
{
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
        [Tooltip("Group size threshold for A icon")]
        [Range(2, 20)]
        public int ThresholdA = 3;

        [Tooltip("Group size threshold for B icon")]
        [Range(2, 30)]
        public int ThresholdB = 5;

        [Tooltip("Group size threshold for C icon")]
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
        /// Get threshold A value
        /// </summary>
        public int GetThresholdA() => ThresholdA;

        /// <summary>
        /// Get threshold B value
        /// </summary>
        public int GetThresholdB() => ThresholdB;

        /// <summary>
        /// Get threshold C value
        /// </summary>
        public int GetThresholdC() => ThresholdC;

        /// <summary>
        /// Calculate icon state based on group size
        /// </summary>
        public byte CalculateIconState(int groupSize)
        {
            if (groupSize > ThresholdC) return 3; // C icon
            if (groupSize > ThresholdB) return 2; // B icon
            if (groupSize > ThresholdA) return 1; // A icon
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
            // Ensure thresholds are in ascending order
            ThresholdB = Mathf.Max(ThresholdB, ThresholdA + 1);
            ThresholdC = Mathf.Max(ThresholdC, ThresholdB + 1);

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
