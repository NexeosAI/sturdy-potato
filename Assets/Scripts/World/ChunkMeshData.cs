using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace RobbieCraft.World
{
    /// <summary>
    /// Helper container that owns the native lists produced by the greedy mesher.
    /// </summary>
    public struct ChunkMeshData
    {
        public NativeList<float3> Vertices;
        public NativeList<int> Triangles;
        public NativeList<float3> Normals;
        public NativeList<float2> Uv;
        public NativeList<Color32> Colors;

        public ChunkMeshData(Allocator allocator)
        {
            Vertices = new NativeList<float3>(allocator);
            Triangles = new NativeList<int>(allocator);
            Normals = new NativeList<float3>(allocator);
            Uv = new NativeList<float2>(allocator);
            Colors = new NativeList<Color32>(allocator);
        }

        public void Dispose()
        {
            if (Vertices.IsCreated) Vertices.Dispose();
            if (Triangles.IsCreated) Triangles.Dispose();
            if (Normals.IsCreated) Normals.Dispose();
            if (Uv.IsCreated) Uv.Dispose();
            if (Colors.IsCreated) Colors.Dispose();
        }
    }
}
