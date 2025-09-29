using System;
using Unity.Mathematics;

namespace RobbieCraft.World
{
    /// <summary>
    /// Value type representing the grid position of a chunk in the infinite world.
    /// </summary>
    [Serializable]
    public struct ChunkCoordinate : IEquatable<ChunkCoordinate>
    {
        /// <summary>
        /// X grid index of the chunk.
        /// </summary>
        public int X;

        /// <summary>
        /// Z grid index of the chunk.
        /// </summary>
        public int Z;

        public ChunkCoordinate(int x, int z)
        {
            X = x;
            Z = z;
        }

        /// <summary>
        /// Converts the chunk coordinate into world-space position of the chunk origin.
        /// </summary>
        public float3 ToWorldPosition(float heightOffset = 0f)
        {
            return new float3(X * ChunkConfig.ChunkSizeX, heightOffset, Z * ChunkConfig.ChunkSizeZ);
        }

        public bool Equals(ChunkCoordinate other) => X == other.X && Z == other.Z;

        public override bool Equals(object obj) => obj is ChunkCoordinate other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Z);

        public static bool operator ==(ChunkCoordinate left, ChunkCoordinate right) => left.Equals(right);

        public static bool operator !=(ChunkCoordinate left, ChunkCoordinate right) => !left.Equals(right);

        public override string ToString() => $"Chunk({X}, {Z})";
    }
}
