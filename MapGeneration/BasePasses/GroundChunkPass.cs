namespace Structure2D.MapGeneration.BasePasses
{
    /// <summary>
    /// This pass generates the ground chunks.
    /// </summary>
    internal class GroundChunkPass : MapGenerationPass
    {
        private const int groundChunkBlock = 2;
    
        public override void Apply(MapGenerator mapGenerator)
        {
            int desiredGroundHeight = mapGenerator.BaseChunkCellHeight + mapGenerator.GroundCellHeight;
        
            for (int x = 0; x < mapGenerator.MapWidth; ++x)
            {
                for (int y = mapGenerator.BaseChunkCellHeight; y < desiredGroundHeight; ++y)
                {
                    ++mapGenerator.UsableDefaultBlocks;
                    var cellData = CellMap.GetCell(x, y);
                    cellData.Block = groundChunkBlock;
                    cellData.Background = 1;
                }
            }
        }
    }
}