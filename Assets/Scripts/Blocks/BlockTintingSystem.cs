using RobbieCraft.World;
using Unity.Collections;
using Unity.Mathematics;

namespace RobbieCraft.Blocks
{
    /// <summary>
    /// Utility helpers that allow the Rainbow Painter to recolor chunk voxels.
    /// </summary>
    public static class BlockTintingSystem
    {
        /// <summary>
        /// Applies a tint index to a block inside a chunk.
        /// </summary>
        public static void ApplyTint(ChunkData chunk, int x, int y, int z, byte tintIndex)
        {
            if (chunk == null)
            {
                return;
            }

            if (!InsideChunk(x, y, z))
            {
                return;
            }

            chunk.SetTintIndex(x, y, z, tintIndex);
        }

        /// <summary>
        /// Looks up the final colour for a block by combining its base colour with a palette tint.
        /// </summary>
        public static float4 EvaluateTintedColor(byte blockId, byte tintIndex, NativeArray<float4> baseColors, NativeArray<byte> tintMask, NativeArray<float4> palette)
        {
            if (!baseColors.IsCreated || blockId >= baseColors.Length)
            {
                return new float4(1f, 1f, 1f, 1f);
            }

            float4 baseColor = baseColors[blockId];
            if (!tintMask.IsCreated || tintIndex == 0 || blockId >= tintMask.Length || tintMask[blockId] == 0)
            {
                return baseColor;
            }

            if (!palette.IsCreated || tintIndex >= palette.Length)
            {
                return baseColor;
            }

            float4 tint = palette[tintIndex];
            return new float4(baseColor.x * tint.x, baseColor.y * tint.y, baseColor.z * tint.z, baseColor.w);
        }

        private static bool InsideChunk(int x, int y, int z)
        {
            return x >= 0 && x < ChunkConfig.ChunkSizeX &&
                   y >= 0 && y < ChunkConfig.ChunkSizeY &&
                   z >= 0 && z < ChunkConfig.ChunkSizeZ;
        }
    }
}
