using System;

namespace Structure2D
{
    /// <summary>
    /// This class handles the initialization of block meta data.
    /// </summary>
    internal class MetaDataManager
    {
        /// <summary>
        /// This gets called for every block that gets initialized
        /// </summary>
        internal  static Action<Block> BlockInitializationEnumerator;

        /// <summary>
        /// This gets called before we iterate over the blocks to initialize them.
        /// The integer is the amount of Blocks that we will iterate over
        /// </summary>
        internal static Action<int> PrepareBlockInitialization;

        /// <summary>
        /// This gets called for every background that gets initialized.
        /// </summary>
        internal static Action<Background> BackgroundInitializationEnumerator;

        /// <summary>
        /// This gets called before we iterate over the background to initialize them.
        /// The integer is the amount of backgrounds that we will iterate over.
        /// </summary>
        internal static Action<int> PrepareBackgroundInitialization;

        internal static void InitializeBlocks(Block[] blocks)
        {
            var blockSize = blocks.Length;
        
            PrepareBlockInitialization?.Invoke(blockSize);

            for (int i = 0; i < blockSize; ++i)
            {
                BlockInitializationEnumerator?.Invoke(blocks[i]);
            }
        }

        internal static void InitializeBackgrounds(Background[] backgrounds)
        {
            var backgroundsSize = backgrounds.Length;
        
            PrepareBackgroundInitialization?.Invoke(backgroundsSize);

            for (int i = 0; i < backgroundsSize; ++i)
            {
                BackgroundInitializationEnumerator?.Invoke(backgrounds[i]);
            }
        }
    }
}