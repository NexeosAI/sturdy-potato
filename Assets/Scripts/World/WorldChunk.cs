using System;
using RobbieCraft.Blocks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

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
        private NativeArray<GreedyMaskEntry> _maskBuffer;
        private NativeArray<BlockVisualInfo> _blockVisuals;
        private BlockTypeRegistry _blockRegistry;
        private byte _activeAirBlock;

        /// <summary>
        /// Coordinate of this chunk on the world grid.
        /// </summary>
        public ChunkCoordinate Coordinate { get; private set; }

        /// <summary>
        /// Configures shared registry data for this chunk instance.
        /// </summary>
        public void Initialize(BlockTypeRegistry registry, NativeArray<BlockVisualInfo> visuals)
        {
            _blockRegistry = registry;
            _blockVisuals = visuals;
        }

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
                _maskBuffer = new NativeArray<GreedyMaskEntry>(ChunkConfig.ChunkSizeX * ChunkConfig.ChunkSizeY, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            }
            NativeArray<byte> rawData = _chunkData.RawData;
            job.Blocks = rawData;
            _generationHandle = job.Schedule(rawData.Length, ChunkConfig.ChunkSizeX);
            _generationScheduled = true;
            _meshRequested = false;
            _currentLodLevel = -1;
            _meshScheduled = false;
            _chunkData.ClearAllTints();
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
            _activeAirBlock = airBlockId;

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
                    AirBlockId = _pendingAirBlock,
                    BlockVisuals = _blockVisuals,
                    TintOverrides = _chunkData.RawTints,
                    Vertices = _meshData.Vertices,
                    Triangles = _meshData.Triangles,
                    Normals = _meshData.Normals,
                    Uv = _meshData.Uv,
                    Colors = _meshData.Colors,
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
            var vertices = new System.Collections.Generic.List<Vector3>();
            var normals = new System.Collections.Generic.List<Vector3>();
            var uvs = new System.Collections.Generic.List<Vector2>();
            var colors = new System.Collections.Generic.List<Color32>();
            var triangles = new System.Collections.Generic.List<int>();

            NativeArray<byte> blocks = _chunkData.RawData;
            NativeArray<Color32> tints = _chunkData.RawTints;

            for (int x = 0; x < ChunkConfig.ChunkSizeX; x += step)
            {
                for (int z = 0; z < ChunkConfig.ChunkSizeZ; z += step)
                {
                    int maxHeight = 0;
                    byte topBlockId = _pendingAirBlock;
                    int topIndex = -1;

                    for (int localX = 0; localX < step && x + localX < ChunkConfig.ChunkSizeX; localX++)
                    {
                        for (int localZ = 0; localZ < step && z + localZ < ChunkConfig.ChunkSizeZ; localZ++)
                        {
                            for (int y = ChunkConfig.ChunkSizeY - 1; y >= 0; y--)
                            {
                                int index = ChunkConfig.ToIndex(x + localX, y, z + localZ);
                                byte blockId = blocks[index];
                                if (blockId != _pendingAirBlock && y + 1 > maxHeight)
                                {
                                    maxHeight = y + 1;
                                    topBlockId = blockId;
                                    topIndex = index;
                                    break;
                                }
                            }
                        }
                    }

                    if (maxHeight == 0)
                    {
                        continue;
                    }

                    BlockVisualInfo visual = default;
                    Color32 faceColor = new Color32(255, 255, 255, 255);
                    if (topBlockId < _blockVisuals.Length)
                    {
                        visual = _blockVisuals[topBlockId];
                        faceColor = visual.BaseColor;
                        if (tints.IsCreated && topIndex >= 0)
                        {
                            Color32 tint = tints[topIndex];
                            if (tint.a > 0 && (visual.Flags & BlockVisualFlags.SupportsTint) != 0)
                            {
                                faceColor = tint;
                            }
                        }
                        if (faceColor.a == 0)
                        {
                            faceColor.a = 255;
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

                    Vector2 uvMin = new Vector2(visual.UvMin.x, visual.UvMin.y);
                    Vector2 uvSize = new Vector2(visual.UvSize.x, visual.UvSize.y);
                    if (uvSize.x <= 0f || uvSize.y <= 0f)
                    {
                        uvSize = Vector2.one;
                    }
                    uvs.Add(uvMin);
                    uvs.Add(uvMin + new Vector2(uvSize.x, 0f));
                    uvs.Add(uvMin + new Vector2(0f, uvSize.y));
                    uvs.Add(uvMin + new Vector2(uvSize.x, uvSize.y));

                    colors.Add(faceColor);
                    colors.Add(faceColor);
                    colors.Add(faceColor);
                    colors.Add(faceColor);
                }
            }

            ApplyMesh(vertices, normals, uvs, colors, triangles);
            _meshScheduled = false;
            _currentLodLevel = lodLevel;
        }

        private void ApplyMesh(System.Collections.Generic.List<Vector3> vertices, System.Collections.Generic.List<Vector3> normals, System.Collections.Generic.List<Vector2> uvs, System.Collections.Generic.List<Color32> colors, System.Collections.Generic.List<int> triangles)
        {
            _mesh ??= new Mesh { name = $"Chunk_{Coordinate.X}_{Coordinate.Z}" };
            _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            _mesh.SetVertices(vertices);
            _mesh.SetNormals(normals);
            _mesh.SetUVs(0, uvs);
            _mesh.SetColors(colors);
            _mesh.SetTriangles(triangles, 0);
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
            _mesh.SetColors(_meshData.Colors.AsArray());
            _mesh.SetTriangles(_meshData.Triangles.AsArray(), 0);
            _mesh.RecalculateBounds();

            GetComponent<MeshFilter>().sharedMesh = _mesh;
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = _mesh;
            }
        }

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

        /// <summary>
        /// Attempts to read the block id at the provided local coordinate.
        /// </summary>
        public bool TryGetBlock(int x, int y, int z, out byte blockId)
        {
            if (!InsideChunk(x, y, z))
            {
                blockId = 0;
                return false;
            }

            if (_chunkData == null)
            {
                blockId = 0;
                return false;
            }

            blockId = _chunkData.RawData[ChunkConfig.ToIndex(x, y, z)];
            return blockId != _activeAirBlock;
        }

        /// <summary>
        /// Returns true when the block type supports tint overrides.
        /// </summary>
        public bool SupportsTint(byte blockId)
        {
            return _blockRegistry != null && _blockRegistry.SupportsTinting(blockId);
        }

        /// <summary>
        /// Applies a tint override to the given voxel.
        /// </summary>
        public void SetBlockTint(int x, int y, int z, Color color)
        {
            if (_chunkData == null)
            {
                return;
            }

            Color32 tintColor = color;
            tintColor.a = 255;
            _chunkData.SetTint(x, y, z, tintColor);
            ScheduleMeshBuild(_activeAirBlock, Mathf.Max(0, _currentLodLevel));
        }

        /// <summary>
        /// Clears any tint override from the given voxel.
        /// </summary>
        public void ClearBlockTint(int x, int y, int z)
        {
            if (_chunkData == null)
            {
                return;
            }

            _chunkData.ClearTint(x, y, z);
            ScheduleMeshBuild(_activeAirBlock, Mathf.Max(0, _currentLodLevel));
        }

        private static bool InsideChunk(int x, int y, int z)
        {
            return x >= 0 && x < ChunkConfig.ChunkSizeX &&
                   y >= 0 && y < ChunkConfig.ChunkSizeY &&
                   z >= 0 && z < ChunkConfig.ChunkSizeZ;
        }
    }
}
