using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace RobbieCraft.Blocks
{
    /// <summary>
    /// Serializable description of a single block type.
    /// </summary>
    [Serializable]
    public sealed class BlockTypeDefinition
    {
        [SerializeField]
        protected internal byte id;

        [SerializeField]
        protected internal string displayName = "New Block";

        [SerializeField]
        protected internal bool isSolid = true;

        [SerializeField, Tooltip("Maximum durability (hit points) before the block breaks.")]
        protected internal float durability = 10f;

        [SerializeField, Tooltip("Default time (seconds) it takes to break the block with standard tools.")]
        protected internal float breakTime = 1f;

        [SerializeField, Tooltip("Base tint color displayed on the voxel faces.")]
        protected internal Color baseTint = Color.white;

        [SerializeField, Tooltip("True if the block can be recolored by tinting tools (Rainbow Painter).")]
        protected internal bool supportsTinting = false;

        [SerializeField, Tooltip("Tile coordinate within the shared block material atlas.")]
        protected internal Vector2Int atlasTile = Vector2Int.zero;

        public byte Id => id;
        public string DisplayName => displayName;
        public bool IsSolid => isSolid;
        public float Durability => math.max(0f, durability);
        public float BreakTime => math.max(0.01f, breakTime);
        public Color BaseTint => baseTint;
        public bool SupportsTinting => supportsTinting;
        public Vector2Int AtlasTile => atlasTile;

        /// <summary>
        /// Converts the definition into the runtime visual info used by Burst jobs.
        /// </summary>
        internal BlockVisualInfo ToVisualInfo(BlockMaterialAtlas atlas)
        {
            Rect uv = atlas != null ? atlas.GetTileUv(atlasTile) : new Rect(0f, 0f, 1f, 1f);
            var info = new BlockVisualInfo
            {
                BaseColor = baseTint,
                UvMin = new float2(uv.xMin, uv.yMin),
                UvSize = new float2(uv.width, uv.height),
                Flags = supportsTinting ? BlockVisualFlags.SupportsTint : BlockVisualFlags.None
            };
            return info;
        }
    }

    /// <summary>
    /// Runtime data passed to Burst jobs for rendering information.
    /// </summary>
    public struct BlockVisualInfo
    {
        public Color32 BaseColor;
        public float2 UvMin;
        public float2 UvSize;
        public BlockVisualFlags Flags;
        private byte _padding0;
        private byte _padding1;
        private byte _padding2;
    }

    [Flags]
    public enum BlockVisualFlags : byte
    {
        None = 0,
        SupportsTint = 1 << 0
    }

    /// <summary>
    /// Scriptable object registry containing all block definitions for the game.
    /// </summary>
    [CreateAssetMenu(fileName = "BlockTypeRegistry", menuName = "RobbieCraft/Blocks/Registry")]
    public sealed class BlockTypeRegistry : ScriptableObject
    {
        [SerializeField]
        private BlockMaterialAtlas materialAtlas;

        [SerializeField]
        private List<BlockTypeDefinition> blockTypes = new();

        private readonly Dictionary<byte, BlockTypeDefinition> _lookup = new();

        /// <summary>
        /// Shared atlas information used by voxel materials.
        /// </summary>
        public BlockMaterialAtlas MaterialAtlas => materialAtlas;

        /// <summary>
        /// Provides read-only access to the block definitions.
        /// </summary>
        public IReadOnlyList<BlockTypeDefinition> BlockTypes => blockTypes;

        private void OnEnable()
        {
            BuildLookup();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            BuildLookup();
        }
#endif

        /// <summary>
        /// Retrieves a block definition by id.
        /// </summary>
        public bool TryGetDefinition(byte blockId, out BlockTypeDefinition definition)
        {
            return _lookup.TryGetValue(blockId, out definition);
        }

        /// <summary>
        /// Gets the durability for a given block id.
        /// </summary>
        public float GetDurability(byte blockId)
        {
            return _lookup.TryGetValue(blockId, out BlockTypeDefinition def) ? def.Durability : 0f;
        }

        /// <summary>
        /// Gets the base break time for a given block id.
        /// </summary>
        public float GetBreakTime(byte blockId)
        {
            return _lookup.TryGetValue(blockId, out BlockTypeDefinition def) ? def.BreakTime : 0f;
        }

        /// <summary>
        /// Returns whether a block id supports rainbow tinting.
        /// </summary>
        public bool SupportsTinting(byte blockId)
        {
            return _lookup.TryGetValue(blockId, out BlockTypeDefinition def) && def.SupportsTinting;
        }

        /// <summary>
        /// Builds the runtime visual information used by the greedy mesher.
        /// The caller is responsible for disposing the returned native array.
        /// </summary>
        public NativeArray<BlockVisualInfo> BuildVisualInfo(Allocator allocator)
        {
            int maxId = 0;
            foreach (BlockTypeDefinition def in blockTypes)
            {
                maxId = Mathf.Max(maxId, def.Id);
            }

            var array = new NativeArray<BlockVisualInfo>(maxId + 1, allocator, NativeArrayOptions.ClearMemory);
            foreach (BlockTypeDefinition def in blockTypes)
            {
                array[def.Id] = def.ToVisualInfo(materialAtlas);
            }

            return array;
        }

        /// <summary>
        /// Attempts to resolve a block id using its display name.
        /// </summary>
        public bool TryGetBlockId(string displayName, out byte blockId)
        {
            foreach (BlockTypeDefinition def in blockTypes)
            {
                if (string.Equals(def.DisplayName, displayName, StringComparison.OrdinalIgnoreCase))
                {
                    blockId = def.Id;
                    return true;
                }
            }

            blockId = 0;
            return false;
        }

        private void BuildLookup()
        {
            _lookup.Clear();
            if (blockTypes == null)
            {
                blockTypes = new List<BlockTypeDefinition>();
            }

            foreach (BlockTypeDefinition def in blockTypes)
            {
                if (_lookup.ContainsKey(def.Id))
                {
                    Debug.LogWarning($"Duplicate block id {def.Id} detected in {name}. Only the first entry will be used.");
                    continue;
                }

                _lookup.Add(def.Id, def);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Populate Default Blocks")]
        private void PopulateDefaults()
        {
            blockTypes = new List<BlockTypeDefinition>
            {
                CreateBlock(0, "Air", false, 0f, 0f, Color.clear, false, new Vector2Int(0, 0)),
                CreateBlock(1, "Grass", true, 12f, 0.8f, new Color32(116, 185, 84, 255), true, new Vector2Int(0, 1)),
                CreateBlock(2, "Dirt", true, 18f, 1.2f, new Color32(134, 96, 67, 255), false, new Vector2Int(1, 1)),
                CreateBlock(3, "Stone", true, 30f, 2.5f, new Color32(120, 120, 120, 255), false, new Vector2Int(2, 1)),
                CreateBlock(10, "Candy", true, 10f, 0.6f, new Color32(255, 116, 188, 255), true, new Vector2Int(0, 2)),
                CreateBlock(11, "Robot", true, 35f, 2.0f, new Color32(180, 196, 210, 255), false, new Vector2Int(1, 2)),
                CreateBlock(12, "Ice", true, 16f, 1.4f, new Color32(160, 220, 255, 200), true, new Vector2Int(2, 2))
            };
            BuildLookup();
        }

        private static BlockTypeDefinition CreateBlock(byte id, string name, bool solid, float durability, float breakTime, Color tint, bool tintable, Vector2Int tile)
        {
            return new BlockTypeDefinitionAccessor(id, name, solid, durability, breakTime, tint, tintable, tile);
        }

        /// <summary>
        /// Helper class used to instantiate definitions with constructor-style syntax inside the editor-only context menu.
        /// </summary>
        private sealed class BlockTypeDefinitionAccessor : BlockTypeDefinition
        {
            public BlockTypeDefinitionAccessor(byte id, string displayName, bool isSolid, float durability, float breakTime, Color baseTint, bool supportsTinting, Vector2Int atlasTile)
            {
                this.id = id;
                this.displayName = displayName;
                this.isSolid = isSolid;
                this.durability = durability;
                this.breakTime = breakTime;
                this.baseTint = baseTint;
                this.supportsTinting = supportsTinting;
                this.atlasTile = atlasTile;
            }
        }
#endif
    }
}
