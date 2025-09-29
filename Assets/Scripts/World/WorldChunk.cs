using System;
codex/review-agents.md-and-checklist.md-files
using System.Collections.Generic;
main
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
codex/review-agents.md-and-checklist.md-files
using RobbieCraft.Blocks;

main

namespace RobbieCraft.World
{
    /// <summary>
    /// Runtime component that owns the mesh and lifecycle of a single chunk instance.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class WorldChunk : MonoBehaviour
    {
        [SerializeField]
        private MeshCollider meshCollider;

        private Mesh _mesh;
        private ChunkData _chunkData;
        private JobHandle _generationHandle;
        private JobHandle _meshHandle;
        private bool _generationScheduled;
        private bool _meshScheduled;
        private bool _meshRequested;
        private ChunkMeshData _meshData;
        private int _currentLodLevel = -1;
        codex/review-agents.md-and-checklist.md-files
        private NativeArray<int> _maskBuffer;
        private NativeArray<BlockUvData> _blockUvData;
        private NativeArray<float4> _blockColors;
        private NativeArray<byte> _blockTintMask;
        private NativeArray<float4> _tintPalette;
        private byte _groundBlockId;

        private NativeArray<byte> _maskBuffer;
        main

        /// <summary>
        /// Coordinate of this chunk on the world grid.
        /// </summary>
        public ChunkCoordinate Coordinate { get; private set; }

        /// <summary>
        /// True when the chunk is waiting for background job completion.
        /// </summary>
        public bool IsBusy => _generationScheduled || _meshScheduled;

        /// <summary>
        /// Current level-of-detail that is displayed by this chunk mesh.
        /// </summary>
        public int CurrentLodLevel => _currentLodLevel;

        /// <summary>
        /// Schedules the asynchronous generation job for this chunk.
        /// </summary>
        public void ScheduleGeneration(ChunkCoordinate coordinate, ChunkGenerationJob job)
        {
            Coordinate = coordinate;
            _chunkData ??= new ChunkData(Allocator.Persistent);
            if (!_maskBuffer.IsCreated)
            {
 codex/review-agents.md-and-checklist.md-files
                _maskBuffer = new NativeArray<int>(ChunkConfig.ChunkSizeX * ChunkConfig.ChunkSizeY, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            }
            NativeArray<byte> rawData = _chunkData.RawData;
            job.Blocks = rawData;
            job.TintIndices = _chunkData.TintData;

                _maskBuffer = new NativeArray<byte>(ChunkConfig.ChunkSizeX * ChunkConfig.ChunkSizeY, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            }
            NativeArray<byte> rawData = _chunkData.RawData;
            job.Blocks = rawData;
 main
            _generationHandle = job.Schedule(rawData.Length, ChunkConfig.ChunkSizeX);
            _generationScheduled = true;
            _meshRequested = false;
            _currentLodLevel = -1;
            _meshScheduled = false;
codex/review-agents.md-and-checklist.md-files
            _groundBlockId = job.GroundBlockId;

main
        }

        /// <summary>
        /// Marks the chunk to build a mesh as soon as generation completes.
        /// </summary>
        public void ScheduleMeshBuild(byte airBlockId, int lodLevel)
        {
            if (_currentLodLevel == lodLevel && !_meshRequested && !_meshScheduled)
            {
                return;
            }

            _pendingAirBlock = airBlockId;
            _pendingLodLevel = lodLevel;

            if (_generationScheduled)
            {
                _meshRequested = true;
            }
            else if (!_meshScheduled)
            {
                ScheduleMeshConstruction();
            }
        }

        private void LateUpdate()
        {
            if (_meshRequested && _generationHandle.IsCompleted && !_meshScheduled)
            {
                FinalizeGeneration();
            }

            if (!_meshScheduled || !_meshHandle.IsCompleted)
            {
                return;
            }

            _meshHandle.Complete();
            ApplyMesh();
            _meshScheduled = false;
            if (_meshData.Vertices.IsCreated)
            {
                _meshData.Dispose();
            }
            _currentLodLevel = _pendingLodLevel;
        }

        private byte _pendingAirBlock;
        private int _pendingLodLevel;

        private void FinalizeGeneration()
        {
            _generationHandle.Complete();
            _generationScheduled = false;
            _meshRequested = false;

            ScheduleMeshConstruction();
        }

        private void ScheduleMeshConstruction()
        {
            _meshRequested = false;

            if (_pendingLodLevel == 0)
            {
                _meshData = new ChunkMeshData(Allocator.TempJob);
                var mesher = new ChunkGreedyMesher
                {
                    Blocks = _chunkData.RawData,
codex/review-agents.md-and-checklist.md-files
                    TintIndices = _chunkData.TintData,
                    BlockUvs = _blockUvData,
                    BlockColors = _blockColors,
                    BlockTintMask = _blockTintMask,
                    TintPalette = _tintPalette,

 main
                    AirBlockId = _pendingAirBlock,
                    Vertices = _meshData.Vertices,
                    Triangles = _meshData.Triangles,
                    Normals = _meshData.Normals,
                    Uv = _meshData.Uv,
codex/review-agents.md-and-checklist.md-files
                    Colors = _meshData.Colors,

 main
                    MaskBuffer = _maskBuffer
                };

                _meshHandle = mesher.Schedule();
                _meshScheduled = true;
            }
            else
            {
                BuildLowDetailMesh(_pendingLodLevel);
            }
        }

        private void BuildLowDetailMesh(int lodLevel)
        {
            int step = 1 << lodLevel;
 codex/review-agents.md-and-checklist.md-files
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            var colors = new List<Color>();

            var vertices = new System.Collections.Generic.List<Vector3>();
            var normals = new System.Collections.Generic.List<Vector3>();
            var uvs = new System.Collections.Generic.List<Vector2>();
            var triangles = new System.Collections.Generic.List<int>();
 main

            NativeArray<byte> blocks = _chunkData.RawData;

            for (int x = 0; x < ChunkConfig.ChunkSizeX; x += step)
            {
                for (int z = 0; z < ChunkConfig.ChunkSizeZ; z += step)
                {
                    int maxHeight = 0;
                    for (int localX = 0; localX < step && x + localX < ChunkConfig.ChunkSizeX; localX++)
                    {
                        for (int localZ = 0; localZ < step && z + localZ < ChunkConfig.ChunkSizeZ; localZ++)
                        {
                            for (int y = ChunkConfig.ChunkSizeY - 1; y >= 0; y--)
                            {
                                if (blocks[ChunkConfig.ToIndex(x + localX, y, z + localZ)] != _pendingAirBlock)
                                {
                                    maxHeight = math.max(maxHeight, y + 1);
                                    break;
                                }
                            }
                        }
                    }

                    float height = maxHeight;
                    Vector3 basePos = new Vector3(x, height, z);
                    int vertStart = vertices.Count;
                    vertices.Add(basePos);
                    vertices.Add(basePos + new Vector3(step, 0f, 0f));
                    vertices.Add(basePos + new Vector3(0f, 0f, step));
                    vertices.Add(basePos + new Vector3(step, 0f, step));

                    triangles.Add(vertStart + 0);
                    triangles.Add(vertStart + 2);
                    triangles.Add(vertStart + 1);
                    triangles.Add(vertStart + 2);
                    triangles.Add(vertStart + 3);
                    triangles.Add(vertStart + 1);

                    Vector3 normal = Vector3.up;
                    normals.Add(normal);
                    normals.Add(normal);
                    normals.Add(normal);
                    normals.Add(normal);

                    uvs.Add(new Vector2(0f, 0f));
                    uvs.Add(new Vector2(1f, 0f));
                    uvs.Add(new Vector2(0f, 1f));
                    uvs.Add(new Vector2(1f, 1f));
codex/review-agents.md-and-checklist.md-files

                    Color color = ConvertColor(BlockTintingSystem.EvaluateTintedColor(_groundBlockId, 0, _blockColors, _blockTintMask, _tintPalette));
                    colors.Add(color);
                    colors.Add(color);
                    colors.Add(color);
                    colors.Add(color);
                }
            }

            ApplyMesh(vertices, normals, uvs, triangles, colors);

                }
            }

            ApplyMesh(vertices, normals, uvs, triangles);
 main
            _meshScheduled = false;
            _currentLodLevel = lodLevel;
        }

codex/review-agents.md-and-checklist.md-files
        private void ApplyMesh(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> triangles, List<Color> colors)

        private void ApplyMesh(System.Collections.Generic.List<Vector3> vertices, System.Collections.Generic.List<Vector3> normals, System.Collections.Generic.List<Vector2> uvs, System.Collections.Generic.List<int> triangles)
 main
        {
            _mesh ??= new Mesh { name = $"Chunk_{Coordinate.X}_{Coordinate.Z}" };
            _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            _mesh.SetVertices(vertices);
            _mesh.SetNormals(normals);
            _mesh.SetUVs(0, uvs);
            _mesh.SetTriangles(triangles, 0);
codex/review-agents.md-and-checklist.md-files
            _mesh.SetColors(colors);

 main
            _mesh.RecalculateBounds();

            GetComponent<MeshFilter>().sharedMesh = _mesh;
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = _mesh;
            }
        }

        private void ApplyMesh()
        {
            _mesh ??= new Mesh { name = $"Chunk_{Coordinate.X}_{Coordinate.Z}" };
            _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            _mesh.SetVertices(_meshData.Vertices.AsArray());
            _mesh.SetNormals(_meshData.Normals.AsArray());
            _mesh.SetUVs(0, _meshData.Uv.AsArray());
            _mesh.SetTriangles(_meshData.Triangles.AsArray(), 0);
 codex/review-agents.md-and-checklist.md-files
            ApplyColors(_meshData.Colors);

 main
            _mesh.RecalculateBounds();

            GetComponent<MeshFilter>().sharedMesh = _mesh;
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = _mesh;
            }
        }

codex/review-agents.md-and-checklist.md-files
        private void ApplyColors(NativeList<float4> colors)
        {
            if (!colors.IsCreated)
            {
                return;
            }

            var colorArray = colors.AsArray();
            var finalColors = new List<Color>(colorArray.Length);
            for (int i = 0; i < colorArray.Length; i++)
            {
                finalColors.Add(ConvertColor(colorArray[i]));
            }

            _mesh.SetColors(finalColors);
        }

        private static Color ConvertColor(float4 linear)
        {
            return new Color(math.clamp(linear.x, 0f, 1f), math.clamp(linear.y, 0f, 1f), math.clamp(linear.z, 0f, 1f), math.clamp(linear.w, 0f, 1f));
        }

        /// <summary>
        /// Provides block visual data used during mesh construction.
        /// </summary>
        public void ConfigureVisuals(NativeArray<BlockUvData> blockUvs, NativeArray<float4> blockColors, NativeArray<byte> blockTintMask, NativeArray<float4> tintPalette)
        {
            _blockUvData = blockUvs;
            _blockColors = blockColors;
            _blockTintMask = blockTintMask;
            _tintPalette = tintPalette;
        }


 main
        private void OnDestroy()
        {
            if (_generationScheduled)
            {
                _generationHandle.Complete();
            }

            if (_meshScheduled)
            {
                _meshHandle.Complete();
            }

            _chunkData?.Dispose();
            if (_meshData.Vertices.IsCreated)
            {
                _meshData.Dispose();
            }
            if (_maskBuffer.IsCreated)
            {
                _maskBuffer.Dispose();
            }
        }
    }
}
