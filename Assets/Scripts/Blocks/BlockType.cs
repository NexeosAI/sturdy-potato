using UnityEngine;

namespace RobbieCraft.Blocks
{
    /// <summary>
    /// ScriptableObject describing an individual block type.
    /// </summary>
    [CreateAssetMenu(menuName = "RobbieCraft/Blocks/Block Type", fileName = "BlockType")]
    public sealed class BlockType : ScriptableObject
    {
        [SerializeField, Tooltip("Unique numeric identifier used in chunk data (0-255)."), Range(0, 255)]
        private byte id;

        [SerializeField, Tooltip("Kid-friendly display name shown in UI.")]
        private string displayName = "Block";

        [SerializeField, Tooltip("Optional description used in tips and codex entries."), TextArea]
        private string description;

        [SerializeField, Tooltip("Category that determines biome and special handling.")]
        private BlockCategory category = BlockCategory.Standard;

        [SerializeField, Tooltip("Base colour of the block in linear space (before tinting).")]
        private Color baseColor = Color.white;

        [SerializeField, Tooltip("Durability value used by mining tools to determine depletion.")]
        private float durability = 1f;

        [SerializeField, Tooltip("Time in seconds required to break the block with a standard tool.")]
        private float breakTime = 0.75f;

        [SerializeField, Tooltip("Controls which atlas tiles should be sampled for each face.")]
        private BlockTextureSet textures = BlockTextureSet.Single(0);

        [SerializeField, Tooltip("If true the Rainbow Painter may recolor this block.")]
        private bool supportsTinting;

        [SerializeField, Tooltip("Optional emission intensity for glowing robot blocks.")]
        private float emissionStrength;

        /// <summary>
        /// Numeric identifier used inside chunk voxel data.
        /// </summary>
        public byte Id => id;

        /// <summary>
        /// Display name shown to the player.
        /// </summary>
        public string DisplayName => displayName;

        /// <summary>
        /// Localised description for UI entries.
        /// </summary>
        public string Description => description;

        /// <summary>
        /// High level category used for biome placement and special effects.
        /// </summary>
        public BlockCategory Category => category;

        /// <summary>
        /// Base colour used when rendering this block.
        /// </summary>
        public Color BaseColor => baseColor;

        /// <summary>
        /// Durability used to calculate mining progress.
        /// </summary>
        public float Durability => Mathf.Max(0f, durability);

        /// <summary>
        /// Number of seconds required to break this block.
        /// </summary>
        public float BreakTime => Mathf.Max(0f, breakTime);

        /// <summary>
        /// Returns the atlas tile index for the requested face.
        /// </summary>
        public int GetTileIndex(BlockFace face) => textures.GetTileIndex(face);

        /// <summary>
        /// Whether the Rainbow Painter may apply tint colours to this block.
        /// </summary>
        public bool SupportsTinting => supportsTinting;

        /// <summary>
        /// Emission intensity used by stylised glowing materials.
        /// </summary>
        public float EmissionStrength => Mathf.Max(0f, emissionStrength);
    }
}
