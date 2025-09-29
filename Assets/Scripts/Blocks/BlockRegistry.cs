using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace RobbieCraft.Blocks
{
    /// <summary>
    /// Central registry that exposes every available block type and builds runtime lookup tables.
    /// </summary>
    [CreateAssetMenu(menuName = "RobbieCraft/Blocks/Block Registry", fileName = "BlockRegistry")]
    public sealed class BlockRegistry : ScriptableObject
    {
        [SerializeField, Tooltip("Shared material atlas providing texture coordinates for blocks.")]
        private BlockMaterialAtlas materialAtlas;

        [SerializeField, Tooltip("Palette used for tintable blocks like the Rainbow Painter.")]
        private BlockTintPalette tintPalette;

        [SerializeField, Tooltip("List of all block types, including Air, Candy, Robot, and Ice.")]
        private List<BlockType> blocks = new();

        /// <summary>
        /// Accessor for the tint palette.
        /// </summary>
        public BlockTintPalette TintPalette => tintPalette;

        /// <summary>
        /// Returns the atlas associated with this registry.
        /// </summary>
        public BlockMaterialAtlas MaterialAtlas => materialAtlas;

        /// <summary>
        /// Total number of registered block types.
        /// </summary>
        public int Count => blocks.Count;

        /// <summary>
        /// Builds runtime friendly arrays that can be used inside Burst jobs.
        /// </summary>
        public BlockRuntimeData CreateRuntimeData(Allocator allocator)
        {
            if (blocks.Count == 0)
            {
                throw new InvalidOperationException("Block registry has no block types configured.");
            }

            int maxId = 0;
            foreach (BlockType block in blocks)
            {
                if (block == null)
                {
                    continue;
                }

                maxId = math.max(maxId, block.Id);
            }

            var runtime = new BlockRuntimeData(maxId + 1, allocator);
            if (tintPalette != null)
            {
                runtime.TintPalette = tintPalette.CreateNativePalette(allocator);
            }
            else
            {
                runtime.TintPalette = new NativeArray<float4>(1, allocator, NativeArrayOptions.ClearMemory);
                runtime.TintPalette[0] = new float4(1f, 1f, 1f, 1f);
            }

            foreach (BlockType block in blocks)
            {
                if (block == null)
                {
                    continue;
                }

                byte id = block.Id;
                runtime.BaseColors[id] = block.BaseColor.linear;
                runtime.Durability[id] = math.max(0f, block.Durability);
                runtime.BreakTimes[id] = math.max(0f, block.BreakTime);
                runtime.TintMask[id] = (byte)(block.SupportsTinting ? 1 : 0);

                runtime.UvData[id] = BuildUvData(block);
            }

            return runtime;
        }

        /// <summary>
        /// Returns the block definition matching the given identifier, or null when it is not registered.
        /// </summary>
        public BlockType GetBlock(byte id)
        {
            foreach (BlockType block in blocks)
            {
                if (block != null && block.Id == id)
                {
                    return block;
                }
            }

            return null;
        }

        private BlockUvData BuildUvData(BlockType block)
        {
            float4 F(int tileIndex) => materialAtlas != null ? materialAtlas.GetUvRect(tileIndex) : new float4(0f, 0f, 1f, 1f);

            return new BlockUvData
            {
                PositiveX = F(block.GetTileIndex(BlockFace.PositiveX)),
                NegativeX = F(block.GetTileIndex(BlockFace.NegativeX)),
                PositiveY = F(block.GetTileIndex(BlockFace.PositiveY)),
                NegativeY = F(block.GetTileIndex(BlockFace.NegativeY)),
                PositiveZ = F(block.GetTileIndex(BlockFace.PositiveZ)),
                NegativeZ = F(block.GetTileIndex(BlockFace.NegativeZ))
            };
        }

        private void OnValidate()
        {
            RemoveNullEntries();
            EnsureUniqueIds();
            EnsureSpecialBlocks();
        }

        private void RemoveNullEntries()
        {
            blocks.RemoveAll(block => block == null);
        }

        private void EnsureUniqueIds()
        {
            HashSet<byte> ids = new HashSet<byte>();
            foreach (BlockType block in blocks)
            {
                if (!ids.Add(block.Id))
                {
                    Debug.LogWarning($"Duplicate block id detected in registry: {block.DisplayName} ({block.Id}).");
                }
            }
        }

        private void EnsureSpecialBlocks()
        {
            bool hasCandy = false;
            bool hasRobot = false;
            bool hasIce = false;

            foreach (BlockType block in blocks)
            {
                if (block == null)
                {
                    continue;
                }

                switch (block.Category)
                {
                    case BlockCategory.Candy:
                        hasCandy = true;
                        break;
                    case BlockCategory.Robot:
                        hasRobot = true;
                        break;
                    case BlockCategory.Ice:
                        hasIce = true;
                        break;
                }
            }

            if (!hasCandy)
            {
                Debug.LogWarning("Block registry is missing a BlockType marked as Candy. Please assign one to unlock the Candy Kingdom biome.");
            }

            if (!hasRobot)
            {
                Debug.LogWarning("Block registry is missing a BlockType marked as Robot. Please assign one to unlock the Robot Factory biome.");
            }

            if (!hasIce)
            {
                Debug.LogWarning("Block registry is missing a BlockType marked as Ice. Please assign one to unlock the Snow and Ice experiences.");
            }
        }
    }
}
