using System.Collections.Generic;
using System;
using RobbieCraft.Blocks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace RobbieCraft.World
{
    /// <summary>
    /// Manages chunk lifecycle, streaming, and visibility around the active player.
    /// </summary>
    public sealed class WorldManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform player;

        [SerializeField]
        private WorldChunk chunkPrefab;

        [Header("Generation Settings")]
        [SerializeField, Range(1, 8)]
        private int viewDistance = 4;

        [SerializeField]
        private float baseHeight = 32f;

        [SerializeField]
        private float noiseScale = 0.05f;

        [SerializeField]
        private float heightAmplitude = 12f;

        [Header("Blocks")]
        [SerializeField]
        private BlockRegistry blockRegistry;

        [SerializeField]
        private BlockType groundBlock;

        [SerializeField]
        private BlockType airBlock;

        private readonly Dictionary<ChunkCoordinate, WorldChunk> _loadedChunks = new();
        private readonly Queue<WorldChunk> _chunkPool = new();
        private Camera _mainCamera;
        private Plane[] _frustumPlanes = new Plane[6];
        private BlockRuntimeData _blockRuntimeData;
        private bool _runtimeInitialized;
        private byte _resolvedGroundBlockId = 1;
        private byte _resolvedAirBlockId;

        private void Awake()
        {
            _mainCamera = Camera.main;
            InitializeRuntimeData();
        }

        private void Start()
        {
            EnsureSpawnArea();
        }

        private void Update()
        {
            if (player == null || chunkPrefab == null)
            {
                return;
            }

            ChunkCoordinate center = WorldToChunk(player.position);
            UpdateVisibleChunks(center);
            UpdateChunkLods(center);
            UpdateFrustumCulling();
        }

        private void EnsureSpawnArea()
        {
            // Preload a flat spawn area centered at origin
            for (int x = -ChunkConfig.SpawnAreaSize; x < ChunkConfig.SpawnAreaSize; x++)
            {
                for (int z = -ChunkConfig.SpawnAreaSize; z < ChunkConfig.SpawnAreaSize; z++)
                {
                    ChunkCoordinate coord = new ChunkCoordinate(x, z);
                    if (_loadedChunks.ContainsKey(coord))
                    {
                        continue;
                    }

                    var chunk = InstantiateChunk(coord);
                    ScheduleChunkBuild(chunk, coord, flat: true, lodLevel: 0);
                }
            }
        }

        private void UpdateVisibleChunks(ChunkCoordinate center)
        {
            List<ChunkCoordinate> needed = new();

            for (int x = -viewDistance; x <= viewDistance; x++)
            {
                for (int z = -viewDistance; z <= viewDistance; z++)
                {
                    ChunkCoordinate coord = new ChunkCoordinate(center.X + x, center.Z + z);
                    needed.Add(coord);
                    if (!_loadedChunks.ContainsKey(coord))
                    {
                        var chunk = InstantiateChunk(coord);
                        bool flat = math.abs(coord.X) < ChunkConfig.SpawnAreaSize && math.abs(coord.Z) < ChunkConfig.SpawnAreaSize;
                        int lod = DetermineLodLevel(center, coord);
                        ScheduleChunkBuild(chunk, coord, flat, lod);
                    }
                }
            }

            // Despawn chunks that are no longer needed.
            List<ChunkCoordinate> toRemove = new();
            foreach (var kvp in _loadedChunks)
            {
                if (!needed.Contains(kvp.Key) && !kvp.Value.IsBusy)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (ChunkCoordinate coord in toRemove)
            {
                var chunk = _loadedChunks[coord];
                _loadedChunks.Remove(coord);
                chunk.gameObject.SetActive(false);
                _chunkPool.Enqueue(chunk);
            }
        }

        private void UpdateChunkLods(ChunkCoordinate center)
        {
            foreach (var kvp in _loadedChunks)
            {
                int desiredLod = DetermineLodLevel(center, kvp.Key);
                if (kvp.Value.CurrentLodLevel != desiredLod && !kvp.Value.IsBusy)
                {
                    kvp.Value.ScheduleMeshBuild(_resolvedAirBlockId, desiredLod);
                }
            }
        }

        private void UpdateFrustumCulling()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null)
                {
                    return;
                }
            }

            GeometryUtility.CalculateFrustumPlanes(_mainCamera, _frustumPlanes);

            foreach (WorldChunk chunk in _loadedChunks.Values)
            {
                Bounds chunkBounds = new Bounds(chunk.transform.position + new Vector3(ChunkConfig.ChunkSizeX, ChunkConfig.ChunkSizeY / 2f, ChunkConfig.ChunkSizeZ) * 0.5f,
                    new Vector3(ChunkConfig.ChunkSizeX, ChunkConfig.ChunkSizeY, ChunkConfig.ChunkSizeZ));
                bool visible = GeometryUtility.TestPlanesAABB(_frustumPlanes, chunkBounds);
                chunk.gameObject.SetActive(visible);
            }
        }

        private WorldChunk InstantiateChunk(ChunkCoordinate coord)
        {
            WorldChunk chunk = _chunkPool.Count > 0 ? _chunkPool.Dequeue() : Instantiate(chunkPrefab);
            chunk.gameObject.name = $"Chunk_{coord.X}_{coord.Z}";
            chunk.transform.SetParent(transform, false);
            chunk.transform.position = new Vector3(coord.X * ChunkConfig.ChunkSizeX, 0f, coord.Z * ChunkConfig.ChunkSizeZ);
            chunk.gameObject.SetActive(true);
            if (_runtimeInitialized)
            {
                chunk.ConfigureVisuals(_blockRuntimeData.UvData, _blockRuntimeData.BaseColors, _blockRuntimeData.TintMask, _blockRuntimeData.TintPalette);
            }
            _loadedChunks[coord] = chunk;
            return chunk;
        }

        private void ScheduleChunkBuild(WorldChunk chunk, ChunkCoordinate coord, bool flat, int lodLevel)
        {
            var job = new ChunkGenerationJob
            {
                Coordinate = coord,
                BaseHeight = flat ? baseHeight : baseHeight,
                NoiseScale = flat ? 0f : noiseScale,
                HeightAmplitude = flat ? 0f : heightAmplitude,
                GroundBlockId = _resolvedGroundBlockId,
                AirBlockId = _resolvedAirBlockId
            };

            chunk.ScheduleGeneration(coord, job);
            chunk.ScheduleMeshBuild(_resolvedAirBlockId, lodLevel);
        }

        private static ChunkCoordinate WorldToChunk(Vector3 worldPosition)
        {
            int x = Mathf.FloorToInt(worldPosition.x / ChunkConfig.ChunkSizeX);
            int z = Mathf.FloorToInt(worldPosition.z / ChunkConfig.ChunkSizeZ);
            return new ChunkCoordinate(x, z);
        }

        private int DetermineLodLevel(ChunkCoordinate center, ChunkCoordinate target)
        {
            int distance = Mathf.Max(Mathf.Abs(center.X - target.X), Mathf.Abs(center.Z - target.Z));
            if (distance <= 1)
            {
                return 0;
            }

            if (distance <= 3)
            {
                return 1;
            }

            return 2;
        }

        private void InitializeRuntimeData()
        {
            _resolvedGroundBlockId = groundBlock != null ? groundBlock.Id : (byte)1;
            _resolvedAirBlockId = airBlock != null ? airBlock.Id : (byte)0;

            if (blockRegistry == null)
            {
                Debug.LogWarning("WorldManager is missing a BlockRegistry reference. Falling back to default block ids.");
                return;
            }

            try
            {
                _blockRuntimeData = blockRegistry.CreateRuntimeData(Allocator.Persistent);
                _runtimeInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to build block runtime data: {ex.Message}");
                _runtimeInitialized = false;
            }
        }

        private void OnDestroy()
        {
            if (_runtimeInitialized)
            {
                _blockRuntimeData.Dispose();
                _runtimeInitialized = false;
            }
        }
    }
}
