using RobbieCraft.World;
using UnityEngine;

namespace RobbieCraft.Blocks
{
    /// <summary>
    /// Utility helpers for applying and clearing per-voxel tint colors.
    /// Designed for use by the Rainbow Painter tool and similar systems.
    /// </summary>
    public static class BlockTintSystem
    {
        /// <summary>
        /// Applies a tint to a specific voxel inside a chunk.
        /// </summary>
        /// <param name="chunk">Target chunk.</param>
        /// <param name="x">Local block X coordinate.</param>
        /// <param name="y">Local block Y coordinate.</param>
        /// <param name="z">Local block Z coordinate.</param>
        /// <param name="color">Desired tint color.</param>
        /// <returns>True if the tint was applied, false if the block does not support tinting.</returns>
        public static bool ApplyTint(WorldChunk chunk, int x, int y, int z, Color color)
        {
            if (chunk == null)
            {
                return false;
            }

            if (!chunk.TryGetBlock(x, y, z, out byte blockId))
            {
                return false;
            }

            if (!chunk.SupportsTint(blockId))
            {
                return false;
            }

            chunk.SetBlockTint(x, y, z, color);
            return true;
        }

        /// <summary>
        /// Clears any tint previously applied to the voxel at the given position.
        /// </summary>
        public static void ClearTint(WorldChunk chunk, int x, int y, int z)
        {
            if (chunk == null)
            {
                return;
            }

            chunk.ClearBlockTint(x, y, z);
        }
    }
}
