using Unity.Collections;
using Unity.Mathematics;

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
 codex/review-agents.md-and-checklist.md-files
        public NativeList<float4> Colors;

      main

        public ChunkMeshData(Allocator allocator)
        {
            Vertices = new NativeList<float3>(allocator);
            Triangles = new NativeList<int>(allocator);
            Normals = new NativeList<float3>(allocator);
            Uv = new NativeList<float2>(allocator);
 codex/review-agents.md-and-checklist.md-files
            Colors = new NativeList<float4>(allocator);

 main
        }

        public void Dispose()
        {
            if (Vertices.IsCreated) Vertices.Dispose();
            if (Triangles.IsCreated) Triangles.Dispose();
            if (Normals.IsCreated) Normals.Dispose();
            if (Uv.IsCreated) Uv.Dispose();
codex/review-agents.md-and-checklist.md-files
            if (Colors.IsCreated) Colors.Dispose();

 main
        }
    }
}
