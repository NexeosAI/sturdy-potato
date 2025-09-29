using System;
using Unity.Collections;
using UnityEngine;

namespace RobbieCraft.World
{
    /// <summary>
    /// Stores block data for a single chunk in a native-friendly structure.
    /// </summary>
    public sealed class ChunkData : IDisposable
    {
        private NativeArray<byte> _blocks;
        private NativeArray<Color32> _tints;

        /// <summary>
        /// Creates a new chunk data container with native storage.
        /// </summary>
        public ChunkData(Allocator allocator)
        {
            _blocks = new NativeArray<byte>(ChunkConfig.BlocksPerChunk, allocator, NativeArrayOptions.ClearMemory);
            _tints = new NativeArray<Color32>(ChunkConfig.BlocksPerChunk, allocator, NativeArrayOptions.ClearMemory);
        }

        /// <summary>
        /// Gets or sets the block id for a given block coordinate in the chunk.
        /// </summary>
        public byte this[int x, int y, int z]
        {
            get => _blocks[ChunkConfig.ToIndex(x, y, z)];
            set => _blocks[ChunkConfig.ToIndex(x, y, z)] = value;
        }

        /// <summary>
        /// Provides raw access to the internal block array.
        /// </summary>
        public NativeArray<byte> RawData => _blocks;

        /// <summary>
        /// Provides raw access to tint override data. Alpha of zero indicates no tint.
        /// </summary>
        public NativeArray<Color32> RawTints => _tints;

        /// <summary>
        /// Fills the entire chunk with a single block type.
        /// </summary>
        public void Fill(byte blockId)
        {
            for (int i = 0; i < _blocks.Length; i++)
            {
                _blocks[i] = blockId;
                _tints[i] = default;
            }
        }

        /// <summary>
        /// Applies a tint override to a block position.
        /// </summary>
        public void SetTint(int x, int y, int z, Color32 color)
        {
            int index = ChunkConfig.ToIndex(x, y, z);
            color.a = 255;
            _tints[index] = color;
        }

        /// <summary>
        /// Removes a tint override from a block position.
        /// </summary>
        public void ClearTint(int x, int y, int z)
        {
            int index = ChunkConfig.ToIndex(x, y, z);
            _tints[index] = default;
        }

        /// <summary>
        /// Clears all tint overrides.
        /// </summary>
        public void ClearAllTints()
        {
            for (int i = 0; i < _tints.Length; i++)
            {
                _tints[i] = default;
            }
        }

        public void Dispose()
        {
            if (_blocks.IsCreated)
            {
                _blocks.Dispose();
            }
            if (_tints.IsCreated)
            {
                _tints.Dispose();
            }
        }
    }
}
