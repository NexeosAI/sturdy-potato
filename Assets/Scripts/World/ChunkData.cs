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

        /// <summary>
        /// Creates a new chunk data container with native storage.
        /// </summary>
        public ChunkData(Allocator allocator)
        {
            _blocks = new NativeArray<byte>(ChunkConfig.BlocksPerChunk, allocator, NativeArrayOptions.ClearMemory);
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
        /// Fills the entire chunk with a single block type.
        /// </summary>
        public void Fill(byte blockId)
        {
            for (int i = 0; i < _blocks.Length; i++)
            {
                _blocks[i] = blockId;
            }
        }

        public void Dispose()
        {
            if (_blocks.IsCreated)
            {
                _blocks.Dispose();
            }
        }
    }
}
