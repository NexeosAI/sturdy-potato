using RobbieCraft.Blocks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RobbieCraft.World
{
    /// <summary>
    /// Represents an entry in the greedy meshing mask.
    /// </summary>
    public struct GreedyMaskEntry
    {
        public byte BlockId;
        public Color32 Color;

        public bool IsEmpty => BlockId == 0;

        public static bool Matches(in GreedyMaskEntry a, in GreedyMaskEntry b)
        {
            return a.BlockId == b.BlockId &&
                   a.Color.r == b.Color.r &&
                   a.Color.g == b.Color.g &&
                   a.Color.b == b.Color.b;
        }
    }

    /// <summary>
    /// Burst-compiled job that performs greedy meshing on chunk voxel data to minimize triangle count.
    /// </summary>
    [BurstCompile]
    public struct ChunkGreedyMesher : IJob
    {
        [ReadOnly]
        public NativeArray<byte> Blocks;

        /// <summary>
        /// Block id that represents "air" and should not be rendered.
        /// </summary>
        public byte AirBlockId;

        [ReadOnly]
        public NativeArray<BlockVisualInfo> BlockVisuals;

        [ReadOnly]
        public NativeArray<Color32> TintOverrides;

        public NativeList<float3> Vertices;
        public NativeList<int> Triangles;
        public NativeList<float3> Normals;
        public NativeList<float2> Uv;
        public NativeList<Color32> Colors;
        public NativeArray<GreedyMaskEntry> MaskBuffer;

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
                    MaskBuffer[i] = default;
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
                        if (currentSolid && !neighborSolid)
                        {
                            byte blockId = GetBlock(voxel.x, voxel.y, voxel.z);
                            MaskBuffer[maskIndex] = new GreedyMaskEntry
                            {
                                BlockId = blockId,
                                Color = ResolveColor(voxel.x, voxel.y, voxel.z, blockId)
                            };
                        }
                        else
                        {
                            MaskBuffer[maskIndex] = default;
                        }
                    }
                }

                // Greedy merge rectangles within mask
                for (int v = 0; v < vSize; v++)
                {
                    for (int u = 0; u < uSize; )
                    {
                        GreedyMaskEntry entry = MaskBuffer[v * uSize + u];
                        if (entry.IsEmpty)
                        {
                            u++;
                            continue;
                        }

                        int width = 1;
                        while (u + width < uSize && GreedyMaskEntry.Matches(MaskBuffer[v * uSize + (u + width)], entry))
                        {
                            width++;
                        }

                        int height = 1;
                        bool done = false;
                        while (v + height < vSize && !done)
                        {
                            for (int k = 0; k < width; k++)
                            {
                                if (!GreedyMaskEntry.Matches(MaskBuffer[(v + height) * uSize + (u + k)], entry))
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
                        AddFace(dirIndex, u + a1, v + b1, c, width, height, entry);

                        // Clear mask region
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                MaskBuffer[(v + y) * uSize + (u + x)] = default;
                            }
                        }

                        u += width;
                    }
                }
            }
        }

        private Color32 ResolveColor(int x, int y, int z, byte blockId)
        {
            Color32 baseColor = default;
            if (blockId < BlockVisuals.Length)
            {
                baseColor = BlockVisuals[blockId].BaseColor;
            }

            if (TintOverrides.IsCreated && TintOverrides.Length == Blocks.Length)
            {
                Color32 tint = TintOverrides[ChunkConfig.ToIndex(x, y, z)];
                if (tint.a > 0 && blockId < BlockVisuals.Length && (BlockVisuals[blockId].Flags & BlockVisualFlags.SupportsTint) != 0)
                {
                    tint.a = baseColor.a == 0 ? (byte)255 : baseColor.a;
                    return tint;
                }
            }

            if (baseColor.a == 0)
            {
                baseColor.a = 255;
            }

            return baseColor;
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

        private void AddFace(int dirIndex, int u, int v, int c, int width, int height, GreedyMaskEntry entry)
        {
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

            BlockVisualInfo visual = entry.BlockId < BlockVisuals.Length ? BlockVisuals[entry.BlockId] : default;
            float2 uvMin = visual.UvMin;
            float2 uvSize = visual.UvSize;
            if (uvSize.x <= 0f || uvSize.y <= 0f)
            {
                uvSize = new float2(1f, 1f);
            }

            Uv.Add(uvMin);
            Uv.Add(uvMin + new float2(uvSize.x * width, 0f));
            Uv.Add(uvMin + new float2(0f, uvSize.y * height));
            Uv.Add(uvMin + new float2(uvSize.x * width, uvSize.y * height));

            Color32 faceColor = entry.Color;
            if (faceColor.a == 0)
            {
                faceColor = visual.BaseColor;
                if (faceColor.a == 0)
                {
                    faceColor.a = 255;
                }
            }

            Colors.Add(faceColor);
            Colors.Add(faceColor);
            Colors.Add(faceColor);
            Colors.Add(faceColor);
        }
    }
}
