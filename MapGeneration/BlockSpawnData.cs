using Structure2D.MapGeneration;
using UnityEngine;

namespace Structure2D.MapGeneration
{
    /// <summary>
    /// Data about how a given Block should be spawned.
    /// Used by adding a instance of this to a Block Spawner
    /// </summary>
    [CreateAssetMenu(menuName = "Structure2D/MapGeneration/Block Spawn Data", fileName = "New BlockSpawnData")]
    public class BlockSpawnData : ScriptableObject
    {
        public int MaxVeinSize
        {
            get => maxVeinSize;
            set => maxVeinSize = value;
        }

        public int MinVeinSize
        {
            get => minVeinSize;
            set => minVeinSize = value;
        }
        
        [Range(0, 1)]
        [SerializeField]
        [Tooltip("The desired amount in percent that should be covered by this block")]
        public float mapBudget;

        [SerializeField] 
        private Block _blockToSpawn;
        
        public int ID => _blockToSpawn.ID;
        
        [SerializeField]
        private int minVeinSize;
    
        [SerializeField]
        private int maxVeinSize;

        /// <summary>
        /// Returns the amount of blocks that should be covered by this one
        /// This gets called with the amount of blocks that are solid
        /// </summary>
        /// <param name="mapSize"></param>
        /// <returns></returns>
        internal int GetDesiredBlocks(int mapSize)
        {
            return Mathf.CeilToInt(mapSize * mapBudget);
        }
        
        public BlockSpawnConditionChecker BlockSpawnConditionChecker = new BlockSpawnConditionChecker();

    }
}