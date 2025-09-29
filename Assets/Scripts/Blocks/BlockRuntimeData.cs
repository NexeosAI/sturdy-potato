using System;
using Unity.Collections;
using Unity.Mathematics;

namespace RobbieCraft.Blocks
{
    /// <summary>
    /// Runtime data generated from the block registry for fast lookup inside jobs.
    /// </summary>
    public struct BlockRuntimeData : IDisposable
    {
        public NativeArray<BlockUvData> UvData;
        public NativeArray<float4> BaseColors;
        public NativeArray<byte> TintMask;
        public NativeArray<float> Durability;
        public NativeArray<float> BreakTimes;
        public NativeArray<float4> TintPalette;
        public int Length;

        public BlockRuntimeData(int count, Allocator allocator)
        {
            Length = math.max(0, count);
            UvData = new NativeArray<BlockUvData>(Length, allocator, NativeArrayOptions.ClearMemory);
            BaseColors = new NativeArray<float4>(Length, allocator, NativeArrayOptions.ClearMemory);
            TintMask = new NativeArray<byte>(Length, allocator, NativeArrayOptions.ClearMemory);
            Durability = new NativeArray<float>(Length, allocator, NativeArrayOptions.ClearMemory);
            BreakTimes = new NativeArray<float>(Length, allocator, NativeArrayOptions.ClearMemory);
            TintPalette = default;
        }

        public void Dispose()
        {
            if (UvData.IsCreated) UvData.Dispose();
            if (BaseColors.IsCreated) BaseColors.Dispose();
            if (TintMask.IsCreated) TintMask.Dispose();
            if (Durability.IsCreated) Durability.Dispose();
            if (BreakTimes.IsCreated) BreakTimes.Dispose();
            if (TintPalette.IsCreated) TintPalette.Dispose();
        }
    }

    /// <summary>
    /// Stores per-face UV rectangles for a block in atlas space.
    /// </summary>
    public struct BlockUvData
    {
        public float4 PositiveX;
        public float4 NegativeX;
        public float4 PositiveY;
        public float4 NegativeY;
        public float4 PositiveZ;
        public float4 NegativeZ;

        public float4 GetFace(BlockFace face)
        {
            return face switch
            {
                BlockFace.PositiveX => PositiveX,
                BlockFace.NegativeX => NegativeX,
                BlockFace.PositiveY => PositiveY,
                BlockFace.NegativeY => NegativeY,
                BlockFace.PositiveZ => PositiveZ,
                BlockFace.NegativeZ => NegativeZ,
                _ => PositiveX
            };
        }
    }
}
