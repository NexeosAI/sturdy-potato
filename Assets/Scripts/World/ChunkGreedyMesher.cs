codex/review-agents.md-and-checklist.md-files
using RobbieCraft.Blocks;

main
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RobbieCraft.World
{
    /// <summary>
    /// Burst-compiled job that performs greedy meshing on chunk voxel data to minimize triangle count.
    /// </summary>
    [BurstCompile]
    public struct ChunkGreedyMesher : IJob
    {
        [ReadOnly]
        public NativeArray<byte> Blocks;

codex/review-agents.md-and-checklist.md-files
        [ReadOnly]
        public NativeArray<byte> TintIndices;

        [ReadOnly]
        public NativeArray<BlockUvData> BlockUvs;

        [ReadOnly]
        public NativeArray<float4> BlockColors;

        [ReadOnly]
        public NativeArray<byte> BlockTintMask;

        [ReadOnly]
        public NativeArray<float4> TintPalette;


 main
        /// <summary>
        /// Block id that represents "air" and should not be rendered.
        /// </summary>
        public byte AirBlockId;

        public NativeList<float3> Vertices;
        public NativeList<int> Triangles;
        public NativeList<float3> Normals;
        public NativeList<float2> Uv;
codex/review-agents.md-and-checklist.md-files
        public NativeList<float4> Colors;
        public NativeArray<int> MaskBuffer;

        public NativeArray<byte> MaskBuffer;
 main

        private static readonly int3[] Directions =
        {
            new int3( 0,  0,  1),
            new int3( 0,  0, -1),
            new int3( 0,  1,  0),
            new int3( 0, -1,  0),
            new int3( 1,  0,  0),
            new int3(-1,  0,  0)
        };

        /// <summary>
        /// Executes the greedy meshing algorithm.
        /// </summary>
        public void Execute()
        {
            // For each direction we compute a 2D mask and greedy merge rectangles.
            for (int dirIndex = 0; dirIndex < Directions.Length; dirIndex++)
            {
                ProcessDirection(dirIndex);
            }
        }

        private void ProcessDirection(int dirIndex)
        {
            int3 dir = Directions[dirIndex];

            int a1, a2, b1, b2, c1, c2;

            if (dirIndex < 2) // +/-Z
            {
                a1 = 0; a2 = ChunkConfig.ChunkSizeX;
                b1 = 0; b2 = ChunkConfig.ChunkSizeY;
                c1 = dirIndex == 0 ? ChunkConfig.ChunkSizeZ - 1 : 0;
                c2 = c1 + (dirIndex == 0 ? 1 : -1);
            }
            else if (dirIndex < 4) // +/-Y
            {
                a1 = 0; a2 = ChunkConfig.ChunkSizeX;
                b1 = 0; b2 = ChunkConfig.ChunkSizeZ;
                c1 = dirIndex == 2 ? ChunkConfig.ChunkSizeY - 1 : 0;
                c2 = c1 + (dirIndex == 2 ? 1 : -1);
            }
            else // +/-X
            {
                a1 = 0; a2 = ChunkConfig.ChunkSizeZ;
                b1 = 0; b2 = ChunkConfig.ChunkSizeY;
                c1 = dirIndex == 4 ? ChunkConfig.ChunkSizeX - 1 : 0;
                c2 = c1 + (dirIndex == 4 ? 1 : -1);
            }

            int uSize = a2 - a1;
            int vSize = b2 - b1;
            int maskLength = uSize * vSize;
            if (MaskBuffer.Length < maskLength)
            {
                throw new System.InvalidOperationException("Mask buffer is too small for greedy meshing.");
            }

            for (int slice = 0; slice < ChunkConfig.ChunkSizeX + ChunkConfig.ChunkSizeY + ChunkConfig.ChunkSizeZ; slice++)
            {
                int c = c1 + slice * (c2 - c1);
                if (c < 0 || c >= (dirIndex < 2 ? ChunkConfig.ChunkSizeZ : dirIndex < 4 ? ChunkConfig.ChunkSizeY : ChunkConfig.ChunkSizeX))
                {
                    break;
                }

                for (int i = 0; i < maskLength; i++)
                {
                    MaskBuffer[i] = 0;
                }

                for (int v = b1; v < b2; v++)
                {
                    for (int u = a1; u < a2; u++)
                    {
                        int3 voxel = dirIndex switch
                        {
                            0 => new int3(u, v, c),
                            1 => new int3(u, v, c),
                            2 => new int3(u, c, v),
                            3 => new int3(u, c, v),
                            4 => new int3(c, v, u),
                            _ => new int3(c, v, u)
                        };

                        int3 neighbor = voxel + dir;
                        bool currentSolid = IsSolid(voxel.x, voxel.y, voxel.z);
                        bool neighborSolid = IsSolid(neighbor.x, neighbor.y, neighbor.z);

                        int maskIndex = (v - b1) * uSize + (u - a1);
codex/review-agents.md-and-checklist.md-files
                        MaskBuffer[maskIndex] = (currentSolid && !neighborSolid) ? EncodeFaceKey(voxel.x, voxel.y, voxel.z) : 0;

                        MaskBuffer[maskIndex] = (byte)((currentSolid && !neighborSolid) ? GetBlock(voxel.x, voxel.y, voxel.z) : (byte)0);
main
                    }
                }

                // Greedy merge rectangles within mask
                for (int v = 0; v < vSize; v++)
                {
                    for (int u = 0; u < uSize; )
                    {
 codex/review-agents.md-and-checklist.md-files
                        int faceKey = MaskBuffer[v * uSize + u];
                        if (faceKey == 0)

                        byte block = MaskBuffer[v * uSize + u];
                        if (block == 0)
 main
                        {
                            u++;
                            continue;
                        }

                        int width = 1;
 codex/review-agents.md-and-checklist.md-files
                        while (u + width < uSize && MaskBuffer[v * uSize + (u + width)] == faceKey)

                        while (u + width < uSize && MaskBuffer[v * uSize + (u + width)] == block)
 main
                        {
                            width++;
                        }

                        int height = 1;
                        bool done = false;
                        while (v + height < vSize && !done)
                        {
                            for (int k = 0; k < width; k++)
                            {
 codex/review-agents.md-and-checklist.md-files
                                if (MaskBuffer[(v + height) * uSize + (u + k)] != faceKey)

                                if (MaskBuffer[(v + height) * uSize + (u + k)] != block)
 main
                                {
                                    done = true;
                                    break;
                                }
                            }

                            if (!done)
                            {
                                height++;
                            }
                        }

                        // Add face
 codex/review-agents.md-and-checklist.md-files
                        AddFace(dirIndex, u + a1, v + b1, c, width, height, faceKey);

                        AddFace(dirIndex, u + a1, v + b1, c, width, height, block);
 main

                        // Clear mask region
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                MaskBuffer[(v + y) * uSize + (u + x)] = 0;
                            }
                        }

                        u += width;
                    }
                }
            }
        }

        private bool IsSolid(int x, int y, int z)
        {
            if (!InsideChunk(x, y, z))
            {
                return false;
            }

            return GetBlock(x, y, z) != AirBlockId;
        }

        private static bool InsideChunk(int x, int y, int z)
        {
            return x >= 0 && x < ChunkConfig.ChunkSizeX &&
                   y >= 0 && y < ChunkConfig.ChunkSizeY &&
                   z >= 0 && z < ChunkConfig.ChunkSizeZ;
        }

        private byte GetBlock(int x, int y, int z) => Blocks[ChunkConfig.ToIndex(x, y, z)];

 codex/review-agents.md-and-checklist.md-files
        private void AddFace(int dirIndex, int u, int v, int c, int width, int height, int faceKey)
        {
            byte blockId = (byte)(faceKey & 0xFF);
            byte tintIndex = (byte)((faceKey >> 8) & 0xFF);

        private void AddFace(int dirIndex, int u, int v, int c, int width, int height, byte blockId)
        {
 main
            float3 normal = Directions[dirIndex];
            int vertexStartIndex = Vertices.Length;

            float3 basePosition;
            float3 uAxis;
            float3 vAxis;

            switch (dirIndex)
            {
                case 0: // +Z
                    basePosition = new float3(u, v, c + 1);
                    uAxis = new float3(width, 0, 0);
                    vAxis = new float3(0, height, 0);
                    break;
                case 1: // -Z
                    basePosition = new float3(u + width, v, c);
                    uAxis = new float3(-width, 0, 0);
                    vAxis = new float3(0, height, 0);
                    break;
                case 2: // +Y
                    basePosition = new float3(u, c + 1, v);
                    uAxis = new float3(width, 0, 0);
                    vAxis = new float3(0, 0, height);
                    break;
                case 3: // -Y
                    basePosition = new float3(u, c, v + height);
                    uAxis = new float3(width, 0, 0);
                    vAxis = new float3(0, 0, -height);
                    break;
                case 4: // +X
                    basePosition = new float3(c + 1, v, u);
                    uAxis = new float3(0, 0, width);
                    vAxis = new float3(0, height, 0);
                    break;
                default: // -X
                    basePosition = new float3(c, v, u + width);
                    uAxis = new float3(0, 0, -width);
                    vAxis = new float3(0, height, 0);
                    break;
            }

            Vertices.Add(basePosition);
            Vertices.Add(basePosition + uAxis);
            Vertices.Add(basePosition + vAxis);
            Vertices.Add(basePosition + uAxis + vAxis);

            Triangles.Add(vertexStartIndex + 0);
            Triangles.Add(vertexStartIndex + 2);
            Triangles.Add(vertexStartIndex + 1);
            Triangles.Add(vertexStartIndex + 2);
            Triangles.Add(vertexStartIndex + 3);
            Triangles.Add(vertexStartIndex + 1);

            Normals.Add(normal);
            Normals.Add(normal);
            Normals.Add(normal);
            Normals.Add(normal);

codex/review-agents.md-and-checklist.md-files
            float4 uvRect = GetFaceUv(blockId, dirIndex);
            float2 uvMin = new float2(uvRect.x, uvRect.y);
            float2 uvMax = new float2(uvRect.z, uvRect.w);
            float2 uvSize = uvMax - uvMin;

            Uv.Add(uvMin);
            Uv.Add(uvMin + new float2(uvSize.x * width, 0f));
            Uv.Add(uvMin + new float2(0f, uvSize.y * height));
            Uv.Add(uvMin + new float2(uvSize.x * width, uvSize.y * height));

            float4 color = BlockTintingSystem.EvaluateTintedColor(blockId, tintIndex, BlockColors, BlockTintMask, TintPalette);
            Colors.Add(color);
            Colors.Add(color);
            Colors.Add(color);
            Colors.Add(color);
        }

        private int EncodeFaceKey(int x, int y, int z)
        {
            byte block = GetBlock(x, y, z);
            if (block == AirBlockId)
            {
                return 0;
            }

            byte tint = TintIndices.IsCreated ? TintIndices[ChunkConfig.ToIndex(x, y, z)] : (byte)0;
            return block | (tint << 8);
        }

        private float4 GetFaceUv(byte blockId, int dirIndex)
        {
            if (!BlockUvs.IsCreated || blockId >= BlockUvs.Length)
            {
                return new float4(0f, 0f, 1f, 1f);
            }

            BlockFace face = dirIndex switch
            {
                0 => BlockFace.PositiveZ,
                1 => BlockFace.NegativeZ,
                2 => BlockFace.PositiveY,
                3 => BlockFace.NegativeY,
                4 => BlockFace.PositiveX,
                _ => BlockFace.NegativeX
            };

            return BlockUvs[blockId].GetFace(face);

            Uv.Add(new float2(0, 0));
            Uv.Add(new float2(width, 0));
            Uv.Add(new float2(0, height));
            Uv.Add(new float2(width, height));
 main
        }
    }
}
