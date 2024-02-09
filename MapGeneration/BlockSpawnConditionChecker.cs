using Structure2D.Utility;
using UnityEngine;

namespace Structure2D.MapGeneration
{
    /// <summary>
    /// Default spawn condition checker for Blocks.
    /// </summary>
    [System.Serializable]
    public class BlockSpawnConditionChecker
    {
        [SerializeField] 
        private bool _canOverrideOtherBlocks;
    
        [SerializeField] 
        private bool _canTouchSurface;

        [Tooltip("Lowest block in % where this block can spawn")]
        [Range(0, 1)]
        [SerializeField] 
        public float _minSpawnHeight;

        [Tooltip("Highest block in % from the given block")]
        [Range(0.1f, 1)]
        [SerializeField] 
        public float _maxSpawnHeight;

        /// <summary>
        /// Override this to create your own custom block spawn logic
        /// By default this returns false if the block is solid and doesn't to any additional checking
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public virtual bool IsCellUsable(Cell cell, MapGenerator mapGenerator)
        {
            if (!mapGenerator.IsBlockInDesiredHeight(cell.Coordinate.y, _minSpawnHeight, _maxSpawnHeight))
                return false;

            if (!_canTouchSurface && DoesCellTouchSurface(cell, mapGenerator))
                return false;
            
            if (_canOverrideOtherBlocks)
                return cell.Block != (int) BaseBlockTypes.EmptyBlock;

            else
            {
                return cell.Block == (int)BaseBlockTypes.BaseBlock;
            }
        }

        private static bool DoesCellTouchSurface(Cell cell, MapGenerator generator)
        {
            for (Direction direction = Direction.Up; direction < Direction.UpLeft; ++direction)
            {
                var neighbor = cell.GetNeighbor(direction);
                
                if(neighbor == null)
                    continue;

                if (neighbor.Block == 0)
                    return true;
            }

            return false;
        }
    }
}