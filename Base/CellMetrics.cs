using UnityEngine;

namespace Structure2D
{
    /// <summary>
    /// Class which holds constant data about the Cells and Chunks.
    /// </summary>
    public static class CellMetrics
    {
        /// <summary>
        /// The size that every Cell should be big.
        /// </summary>
        public const float CellSize = 0.25f;
        
        /// <summary>
        /// The amount of Cells that a chunk can store in each dimensions.
        /// </summary>
        public const int ChunkSize = 10;
    }
}