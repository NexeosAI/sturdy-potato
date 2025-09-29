using System;
using Unity.Collections;

namespace RobbieCraft.World
{
    /// <summary>
    /// Stores block data for a single chunk in a native-friendly structure.
    /// </summary>
    public sealed class ChunkData : IDisposable
    {
        private NativeArray<byte> _blocks;
 codex/review-agents.md-and-checklist.md-files
        private NativeArray<byte> _tintIndices;

main

        /// <summary>
        /// Creates a new chunk data container with native storage.
        /// </summary>
        public ChunkData(Allocator allocator)
        {
            _blocks = new NativeArray<byte>(ChunkConfig.BlocksPerChunk, allocator, NativeArrayOptions.ClearMemory);
 codex/review-agents.md-and-checklist.md-files
            _tintIndices = new NativeArray<byte>(ChunkConfig.BlocksPerChunk, allocator, NativeArrayOptions.ClearMemory);

main
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
 codex/review-agents.md-and-checklist.md-files
        /// Gets or sets the tint palette index for a block at the supplied coordinate.
        /// </summary>
        public byte GetTintIndex(int x, int y, int z) => _tintIndices[ChunkConfig.ToIndex(x, y, z)];

        /// <summary>
        /// Assigns a tint palette index for a block at the supplied coordinate.
        /// </summary>
        public void SetTintIndex(int x, int y, int z, byte tintIndex) => _tintIndices[ChunkConfig.ToIndex(x, y, z)] = tintIndex;

        /// <summary>

 main
        /// Provides raw access to the internal block array.
        /// </summary>
        public NativeArray<byte> RawData => _blocks;

        /// <summary>
 codex/review-agents.md-and-checklist.md-files
        /// Provides raw access to the tint palette indices for each block in the chunk.
        /// </summary>
        public NativeArray<byte> TintData => _tintIndices;

        /// <summary>

> main
        /// Fills the entire chunk with a single block type.
        /// </summary>
        public void Fill(byte blockId)
        {
            for (int i = 0; i < _blocks.Length; i++)
            {
                _blocks[i] = blockId;
 codex/review-agents.md-and-checklist.md-files
                _tintIndices[i] = 0;
            }
        }

        /// <summary>
        /// Resets all tint indices within the chunk to the provided value.
        /// </summary>
        public void ClearTint(byte tintIndex = 0)
        {
            for (int i = 0; i < _tintIndices.Length; i++)
            {
                _tintIndices[i] = tintIndex;

 main
            }
        }

        public void Dispose()
        {
            if (_blocks.IsCreated)
            {
                _blocks.Dispose();
            }
 codex/review-agents.md-and-checklist.md-files
            if (_tintIndices.IsCreated)
            {
                _tintIndices.Dispose();
            }

 main
        }
    }
}
