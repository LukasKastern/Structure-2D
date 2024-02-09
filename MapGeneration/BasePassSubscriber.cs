using Structure2D.MapGeneration.BasePasses;

namespace Structure2D.MapGeneration
{
    /// <summary>
    /// Pass subscriber which has the base passes.
    /// </summary>
    public struct BasePassSubscriber : IGenerationPassSubscriber
    {
        public MapGenerationPass[] GetPasses()
        {
            return new MapGenerationPass[]
            {
                new BaseChunkPass(),
                new GroundChunkPass(),
                new TerrainChunkPass(),
                new AirChunkPass(),
            };
        }

        public int FetchPassOrder()
        {
            return 0;
        }

        public int PastProgressionWeight()
        {
            return 1;
        }
    }
}