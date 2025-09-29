using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace RobbieCraft.Blocks
{
    /// <summary>
    /// Palette of tint colors used by the Rainbow Painter and other recoloring tools.
    /// </summary>
    [CreateAssetMenu(menuName = "RobbieCraft/Blocks/Tint Palette", fileName = "BlockTintPalette")]
    public sealed class BlockTintPalette : ScriptableObject
    {
        [Tooltip("List of tint colors. Index 0 should remain white (no tint).")]
        [SerializeField]
        private List<Color> colors = new List<Color> { Color.white, new Color(1f, 0.4f, 0.7f), new Color(0.4f, 0.8f, 1f), new Color(0.6f, 1f, 0.6f), new Color(1f, 0.9f, 0.4f) };

        /// <summary>
        /// Number of tint colors defined in the palette.
        /// </summary>
        public int Count => colors.Count;

        /// <summary>
        /// Gets a tint color by index in linear space.
        /// </summary>
        public float4 GetLinearColor(int index)
        {
            if (index < 0 || index >= colors.Count)
            {
                return new float4(1f, 1f, 1f, 1f);
            }

            Color c = colors[index];
            Color linear = c.linear;
            return new float4(linear.r, linear.g, linear.b, linear.a);
        }

        /// <summary>
        /// Builds a native array containing the palette colors.
        /// </summary>
        public NativeArray<float4> CreateNativePalette(Allocator allocator)
        {
            var array = new NativeArray<float4>(math.max(1, colors.Count), allocator, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = GetLinearColor(i);
            }

            return array;
        }
    }
}
