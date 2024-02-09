using AccidentalNoise;

namespace Structure2D.MapGeneration
{
    /// <summary>
    /// Base class for the passes of the Map Generator
    /// </summary>
    public abstract class MapGenerationPass
    {
        /// <summary>
        /// Override this to add your Pass logic.
        /// </summary>
        /// <param name="mapGenerator">This is the generator that calls the pass.</param>
        public virtual void Apply(MapGenerator mapGenerator) {}
        
        /// <summary>
        /// This is called before the generation gets started on the separate thread,
        /// you can use this to prepare things that are only possible on the main thread.
        /// </summary>
        public virtual void PrepareGeneration() {}

        /// <summary>
        /// This weight is used the calculate the current Map Generation progress.
        /// </summary>
        public virtual int GetWeight()
        {
            return 10;
        }
    }
}