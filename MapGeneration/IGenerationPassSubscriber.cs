namespace Structure2D.MapGeneration
{
    public interface IGenerationPassSubscriber
    {
        MapGenerationPass[] GetPasses();
        
        int FetchPassOrder();
    }
}