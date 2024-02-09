
using UnityEngine;

namespace Structure2D.MapGeneration.BasePasses
{
    /// <summary>
    /// This is the pass that uses the Noise Map to generate a terrain on top of the ground chunks
    /// </summary>
    internal class TerrainChunkPass : MapGenerationPass
    {
        private const int defaultTerrainChunkBlock = (int)BaseBlockTypes.BaseBlock;
    
        public override void Apply(MapGenerator mapGenerator)
        {
            int xIndex = 0;
            int yIndex = 0;

            int heightStart = mapGenerator.GroundCellHeight + mapGenerator.BaseChunkCellHeight;
            
            foreach (var block in NoiseMap.GetTerrain(mapGenerator.MapWidth, mapGenerator.TerrainCellHeight, mapGenerator.ActiveSeed))
            {
                if (yIndex >= mapGenerator.TerrainCellHeight)
                {
                    ++xIndex;
                    yIndex = 0;
                }

                bool isSolid = block <= 0.5;

                if (isSolid)
                    ++mapGenerator.UsableDefaultBlocks;

                var cellData = CellMap.GetCell(xIndex, yIndex + heightStart);
                cellData.Block = (ushort)(isSolid ?  (int)BaseBlockTypes.BaseBlock : 0);
                cellData.Background = (ushort)(isSolid ? 1 : (int)BaseBlockTypes.EmptyBlock);
                ++yIndex;
            }
        }

        public override int GetWeight()
        {
            return 5;
        }
    }
}