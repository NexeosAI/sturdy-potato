using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RobbieCraft.World
{
    /// <summary>
    /// Generates voxel data for a chunk using layered Perlin noise.
    /// </summary>
    [BurstCompile]
    public struct ChunkGenerationJob : IJobParallelFor
    {
        public ChunkCoordinate Coordinate;
        public NativeArray<byte> Blocks;
        public float BaseHeight;
        public float NoiseScale;
        public float HeightAmplitude;
        public byte GroundBlockId;
        public byte AirBlockId;

        public void Execute(int index)
        {
            int3 localPos = ChunkConfig.ToCoordinate(index);
            int worldX = Coordinate.X * ChunkConfig.ChunkSizeX + localPos.x;
            int worldZ = Coordinate.Z * ChunkConfig.ChunkSizeZ + localPos.z;

            float height = BaseHeight + noise.snoise(new float2(worldX, worldZ) * NoiseScale) * HeightAmplitude;
            int terrainHeight = math.clamp((int)math.round(height), 0, ChunkConfig.ChunkSizeY - 1);

            Blocks[index] = localPos.y <= terrainHeight ? GroundBlockId : AirBlockId;
        }
    }
}
