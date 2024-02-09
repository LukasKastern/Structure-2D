namespace Structure2D.MapGeneration.BasePasses
{
    /// <summary>
    /// This pass generates Air chunks which are used as a buffer on top of the generated map to let the player build upwards
    /// </summary>
    internal class AirChunkPass : MapGenerationPass
    {
        public override void Apply(MapGenerator mapGenerator)
        {
            int startHeight = mapGenerator.GroundCellHeight + mapGenerator.BaseChunkCellHeight + mapGenerator.TerrainCellHeight;

            for (int x = 0; x < mapGenerator.MapWidth; ++x)
            {
                for (int y = startHeight; y < mapGenerator.AirChunkCellHeight + startHeight; ++y)
                {
                    var cellData = CellMap.GetCell(x, y);
                    cellData.Block = 0;
                    cellData.Background = 0;
                }
            }

        }
    }
}