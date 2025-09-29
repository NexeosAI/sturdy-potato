using System;
using UnityEngine;

namespace RobbieCraft.Blocks
{
    /// <summary>
    /// Defines which atlas tile is used for each face of a block.
    /// </summary>
    [Serializable]
    public struct BlockTextureSet
    {
        [Tooltip("Atlas tile index for the face pointing along +X.")]
        [SerializeField]
        private int positiveX;

        [Tooltip("Atlas tile index for the face pointing along -X.")]
        [SerializeField]
        private int negativeX;

        [Tooltip("Atlas tile index for the face pointing along +Y (top).")]
        [SerializeField]
        private int positiveY;

        [Tooltip("Atlas tile index for the face pointing along -Y (bottom).")]
        [SerializeField]
        private int negativeY;

        [Tooltip("Atlas tile index for the face pointing along +Z.")]
        [SerializeField]
        private int positiveZ;

        [Tooltip("Atlas tile index for the face pointing along -Z.")]
        [SerializeField]
        private int negativeZ;

        /// <summary>
        /// Creates a texture set that uses the same atlas tile for every face.
        /// </summary>
        public static BlockTextureSet Single(int tileIndex)
        {
            return new BlockTextureSet
            {
                positiveX = tileIndex,
                negativeX = tileIndex,
                positiveY = tileIndex,
                negativeY = tileIndex,
                positiveZ = tileIndex,
                negativeZ = tileIndex
            };
        }

        /// <summary>
        /// Returns the atlas tile index used for the given face.
        /// </summary>
        public int GetTileIndex(BlockFace face)
        {
            return face switch
            {
                BlockFace.PositiveX => positiveX,
                BlockFace.NegativeX => negativeX,
                BlockFace.PositiveY => positiveY,
                BlockFace.NegativeY => negativeY,
                BlockFace.PositiveZ => positiveZ,
                BlockFace.NegativeZ => negativeZ,
                _ => positiveX
            };
        }

        /// <summary>
        /// Sets every face to use the provided atlas tile index.
        /// </summary>
        public void SetAll(int tileIndex)
        {
            positiveX = tileIndex;
            negativeX = tileIndex;
            positiveY = tileIndex;
            negativeY = tileIndex;
            positiveZ = tileIndex;
            negativeZ = tileIndex;
        }
    }
}
