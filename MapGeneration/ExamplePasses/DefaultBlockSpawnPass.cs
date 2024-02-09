using System.Security.Cryptography;
using Structure2D.Utility.MapGeneration;
using Structure2D.Utility;
using UnityEngine;

namespace Structure2D.MapGeneration.BasePasses
{
    /// <summary>
    /// This pass generates veins for the given Block.
    /// </summary>
    internal class DefaultBlockSpawnPass : MapGenerationPass
    {
        private static PriorityQueue _priorityQueue;
        private BlockSpawnData _block;

        private static int usedBlocksOnMap = 0;
        internal DefaultBlockSpawnPass(BlockSpawnData blockToCreateVeinsFor)
        {
            _block = blockToCreateVeinsFor;
        }
        
        public override void Apply(MapGenerator mapGenerator)
        {
            //If the block ID is -1 it is inside the Block Data therefor we can't create a vein for it
            if (_block == null || _block.ID == -1)
            {
                Debug.LogWarning("Couldn't apply Block Spawn Pass, block was null or not added to the CellData ");
                return;
            }
            
            
            int desiredBlocks = _block.GetDesiredBlocks(mapGenerator.UsableDefaultBlocks);

            var leftOverBlocks = mapGenerator.UsableDefaultBlocks - usedBlocksOnMap;
            
            //Calculate how much blocks we can spawn
            int actualBlocks = desiredBlocks > leftOverBlocks ? leftOverBlocks : desiredBlocks;

            int usedBlocks = 0;

            int leftOverBlocksToSpawn = actualBlocks - usedBlocks;

            while (usedBlocks < actualBlocks && leftOverBlocksToSpawn > _block.MinVeinSize)
            {
                leftOverBlocksToSpawn = actualBlocks - usedBlocks;

                int veinSize = mapGenerator.MapGenRandom.Next(_block.MinVeinSize, _block.MaxVeinSize);

                if (veinSize > leftOverBlocksToSpawn)
                    veinSize = leftOverBlocksToSpawn;

                var spawnedBlocks = CreateVein(veinSize, _block.BlockSpawnConditionChecker, _block.ID, mapGenerator);

                if (spawnedBlocks == -1)
                    return;
                

                usedBlocks += spawnedBlocks;
            }

            usedBlocksOnMap += usedBlocks;
        }
        
        private int CreateVein(int veinSize, BlockSpawnConditionChecker condition, int blockId, MapGenerator mapGenerator)
        {
            if(_priorityQueue == null)
                _priorityQueue = new PriorityQueue();
            
            var notUsableBaseChunkRange = mapGenerator.BaseChunkCellHeight;

            var firstCell = mapGenerator.GetRandomSolidCell();

            //There are no more cells than we can use
            if (firstCell == null)
                return -1;

            if (!condition.IsCellUsable(firstCell, mapGenerator))
                return 0;
            
            var queueData = new PriorityQueue.QueueData();

            queueData.Cell = firstCell;
            queueData.Distance = 0;
            queueData.SearchHeuristic = 0;
            
            _priorityQueue.Enqueue(queueData);

            var center = firstCell.Coordinate;

            int count = 0;

            while (_priorityQueue.Count > 0 && count < veinSize)
            {
                var currentCell = _priorityQueue.Dequeue(mapGenerator);
                
                if (currentCell.Block != (int)BaseBlockTypes.EmptyBlock)
                    currentCell.Block = blockId;

                ++count;

                for (Direction direction = Direction.Up; direction < Direction.UpLeft; ++direction)
                {
                    var neighbor = currentCell.GetNeighbor(direction);

                    if (neighbor == null || !_priorityQueue.IsCellQueueAble(neighbor) ||
                        neighbor.Coordinate.y < notUsableBaseChunkRange ||
                        !condition.IsCellUsable(neighbor, mapGenerator)) continue;
                    
                    queueData.Cell = neighbor;
                    queueData.Distance = neighbor.Coordinate.DistanceTo(center);
                    queueData.SearchHeuristic = (byte)(mapGenerator.MapGenRandom.NextDouble() < 0.5f ? 1 : 0);
                    
                    _priorityQueue.Enqueue(queueData);
                }
            }
            
            _priorityQueue.Clear();

            return count;
        }
    }
}