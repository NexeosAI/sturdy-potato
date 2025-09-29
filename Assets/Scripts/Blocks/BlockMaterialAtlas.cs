using System;
using Unity.Mathematics;
using UnityEngine;

namespace RobbieCraft.Blocks
{
    /// <summary>
    /// Describes the shared 2048×2048 material atlas used by RobbieCraft blocks.
    /// </summary>
    [CreateAssetMenu(menuName = "RobbieCraft/Blocks/Material Atlas", fileName = "BlockMaterialAtlas")]
    public sealed class BlockMaterialAtlas : ScriptableObject
    {
        [Tooltip("Atlas texture containing every block tile (defaults to 2048×2048).")]
        [SerializeField]
        private Texture2D texture;

        [Tooltip("Pixel size of each square tile inside the atlas.")]
        [SerializeField]
        private int tileSize = 128;

        [Tooltip("Optional padding in pixels that will be trimmed from each edge when generating UVs.")]
        [SerializeField]
        private int tilePadding = 2;

        /// <summary>
        /// Gets the atlas texture assigned in the editor.
        /// </summary>
        public Texture2D Texture => texture;

        /// <summary>
        /// Returns the pixel size of each tile.
        /// </summary>
        public int TileSize => math.max(1, tileSize);

        /// <summary>
        /// Computes the UV rectangle for the supplied atlas tile index.
        /// </summary>
        public float4 GetUvRect(int tileIndex)
        {
            if (tileIndex < 0)
            {
                return new float4(0f, 0f, 1f, 1f);
            }

            int atlasWidth = texture != null ? texture.width : 2048;
            int atlasHeight = texture != null ? texture.height : 2048;
            int tile = math.max(1, tileSize);
            int tilesPerRow = math.max(1, atlasWidth / tile);

            int x = tileIndex % tilesPerRow;
            int y = tileIndex / tilesPerRow;

            float pixelX = x * tile + tilePadding * 0.5f;
            float pixelY = y * tile + tilePadding * 0.5f;
            float effectiveTileSize = tile - tilePadding;
            effectiveTileSize = math.max(1f, effectiveTileSize);

            float minX = pixelX / atlasWidth;
            float minY = pixelY / atlasHeight;
            float maxX = (pixelX + effectiveTileSize) / atlasWidth;
            float maxY = (pixelY + effectiveTileSize) / atlasHeight;

            return new float4(minX, minY, maxX, maxY);
        }
    }
}
