namespace Structure2D.MapGeneration.BasePasses
{
    /// <summary>
    /// Pass which generates Chunks of the base block type.
    /// </summary>
    internal class BaseChunkPass : MapGenerationPass
    {
        private const int baseChunkBlock = 1;
    
        public override void Apply(MapGenerator mapGenerator)
        {
            for (int x = 0; x < mapGenerator.MapWidth; ++x)
            {
                for (int y = 0; y < mapGenerator.BaseChunkCellHeight; ++y)
                {
                    var cellData = CellMap.GetCell(x, y);
                    cellData.Block = baseChunkBlock;
                    cellData.Background = 1;
                }
            }
        }
    }
}