namespace Structure2D
{
    /// <summary>
    /// Base class for implementing your own Meta Data.
    /// </summary>
    public class MetaDataBaseClass
    {
        
        /// <summary>
        /// This will be called before we iterate over the blocks.
        /// </summary>
        /// <param name="size">The amount of blocks we iterate over.</param>
        public virtual void RegisterForBlockInitialization(int size) { }
 
        /// <summary>
        /// This is called for every Block that we iterate over.
        /// </summary>
        /// <param name="block">The current Block.</param>
        public virtual void EnumerateBlockInitialization(Block block) { }
 
        /// <summary>
        /// This is called for every background we iterate over.
        /// </summary>
        /// <param name="size">Amount of background that we iterate over.</param>
        public virtual void RegisterForBackgroundInitialization(int size) { }
 
        /// <summary>
        /// This is called for every Background we iterate over.
        /// </summary>
        /// <param name="background">The current Background.</param>
        public virtual void EnumerateBackgroundInitialization(Background background) { }
    }
}