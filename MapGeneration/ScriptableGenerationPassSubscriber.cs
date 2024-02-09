using System;
using UnityEngine;

namespace Structure2D.MapGeneration
{
    /// <summary>
    /// Subscriber which you can add to the Map Generator to run your own MapGenPasses.
    /// </summary>
    public class ScriptableGenerationPassSubscriber : ScriptableObject, IGenerationPassSubscriber
    {
        public virtual MapGenerationPass[] GetPasses()
        {
            throw new NotImplementedException("You have to override GetPasses of your subscriber");
        }

        public virtual int FetchPassOrder()
        {
            return 100;
        }

        public int PastProgressionWeight()
        {
            return 1;
        }
    }
}