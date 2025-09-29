using Unity.Mathematics;

namespace RobbieCraft.World
{
    /// <summary>
    /// Central configuration and helper utilities for RobbieCraft's voxel world.
    /// </summary>
    public static class ChunkConfig
    {
        /// <summary>
        /// Width of a chunk in blocks.
        /// </summary>
        public const int ChunkSizeX = 16;

        /// <summary>
        /// Height of a chunk in blocks.
        /// </summary>
        public const int ChunkSizeY = 128;

        /// <summary>
        /// Depth of a chunk in blocks.
        /// </summary>
        public const int ChunkSizeZ = 16;

        /// <summary>
        /// Total number of blocks contained in a single chunk.
        /// </summary>
        public const int BlocksPerChunk = ChunkSizeX * ChunkSizeY * ChunkSizeZ;

        /// <summary>
        /// Size of the safe, flat spawn area in chunks.
        /// </summary>
        public const int SpawnAreaSize = 2; // 32x32 blocks when multiplied by chunk size.

        /// <summary>
        /// Index helper that flattens a 3D block position inside a chunk to a 1D array index.
        /// </summary>
        public static int ToIndex(int x, int y, int z)
        {
            return x + ChunkSizeX * (z + ChunkSizeZ * y);
        }

        /// <summary>
        /// Converts a block index back into a <see cref="int3"/> coordinate relative to the chunk.
        /// </summary>
        public static int3 ToCoordinate(int index)
        {
            int y = index / (ChunkSizeX * ChunkSizeZ);
            int remaining = index - (y * ChunkSizeX * ChunkSizeZ);
            int z = remaining / ChunkSizeX;
            int x = remaining - (z * ChunkSizeX);
            return new int3(x, y, z);
        }
    }
}
