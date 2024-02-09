using System.Collections.Generic;

namespace Structure2D.Utility
{
    internal class ChunkPool
    {
        private static Stack<Chunk> _pool = new Stack<Chunk>();
    
        internal static void Add(Chunk chunk)
        {
            chunk.IsVisible = false;
            chunk.ClearColliders();
            _pool.Push(chunk);
        }

        internal static Chunk Get()
        {
            //Pool is empty so we create a new chunk
            if (_pool.Count == 0)
            {
                return CreateChunk();
            }
        
            return _pool.Pop();
        }
    
        private static Chunk CreateChunk()
        {
            var newChunk = new Chunk();
        
            newChunk.Initialize();


            return newChunk;
        }

    }
}