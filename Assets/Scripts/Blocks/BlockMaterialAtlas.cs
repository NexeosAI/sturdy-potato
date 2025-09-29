using UnityEngine;

namespace RobbieCraft.Blocks
{
    /// <summary>
    /// Describes the shared texture atlas used by voxel blocks.
    /// Stores UV layout information so runtime systems can map block tiles.
    /// </summary>
    [CreateAssetMenu(fileName = "BlockMaterialAtlas", menuName = "RobbieCraft/Blocks/Material Atlas")]
    public sealed class BlockMaterialAtlas : ScriptableObject
    {
        [SerializeField]
        private Texture2D atlasTexture;

        [SerializeField]
        private Vector2Int atlasSize = new Vector2Int(2048, 2048);

        [SerializeField]
        private Vector2Int tileSize = new Vector2Int(128, 128);

        [SerializeField, Tooltip("Optional padding between tiles in pixels to avoid bleeding when sampling mipmaps.")]
        private int tilePadding = 4;

        /// <summary>
        /// Underlying texture for the atlas. Can be null in editor builds while content is authored.
        /// </summary>
        public Texture2D AtlasTexture => atlasTexture;

        /// <summary>
        /// Pixel dimensions of the atlas. Falls back to the assigned texture size if available.
        /// </summary>
        public Vector2Int AtlasSize => atlasTexture != null ? new Vector2Int(atlasTexture.width, atlasTexture.height) : atlasSize;

        /// <summary>
        /// Pixel size for each tile entry in the atlas.
        /// </summary>
        public Vector2Int TileSize => tileSize;

        /// <summary>
        /// Pixel padding between atlas tiles.
        /// </summary>
        public int TilePadding => Mathf.Max(0, tilePadding);

        /// <summary>
        /// Calculates the UV rect for a tile coordinate within the atlas.
        /// </summary>
        /// <param name="tileIndex">Zero-based tile coordinate (x = column, y = row).</param>
        /// <returns>UV rect in normalized 0-1 space.</returns>
        public Rect GetTileUv(Vector2Int tileIndex)
        {
            Vector2Int size = AtlasSize;
            if (tileSize.x <= 0 || tileSize.y <= 0 || size.x <= 0 || size.y <= 0)
            {
                return new Rect(0f, 0f, 1f, 1f);
            }

            Vector2 tileSizeWithPadding = new Vector2(tileSize.x + TilePadding, tileSize.y + TilePadding);
            Vector2 uvScale = new Vector2(tileSize.x / (float)size.x, tileSize.y / (float)size.y);
            Vector2 uvPadding = new Vector2(TilePadding / (float)size.x, TilePadding / (float)size.y);

            Vector2 uvMin = new Vector2(
                tileIndex.x * tileSizeWithPadding.x / size.x,
                tileIndex.y * tileSizeWithPadding.y / size.y);

            return new Rect(uvMin.x + uvPadding.x * 0.5f, uvMin.y + uvPadding.y * 0.5f, uvScale.x - uvPadding.x, uvScale.y - uvPadding.y);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            atlasSize.x = Mathf.Max(1, atlasSize.x);
            atlasSize.y = Mathf.Max(1, atlasSize.y);
            tileSize.x = Mathf.Max(1, tileSize.x);
            tileSize.y = Mathf.Max(1, tileSize.y);

            if (atlasTexture != null && (atlasTexture.width != 2048 || atlasTexture.height != 2048))
            {
                Debug.LogWarning($"BlockMaterialAtlas '{name}' expects a 2048x2048 texture for optimal sampling.");
            }
        }
#endif
    }
}
