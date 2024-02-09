using System;
using UnityEngine;

namespace Structure2D.MapGeneration
{
    /// <summary>
    /// This is the settings that you pass to the MapGenerator so he knows how to generate the Map.
    /// </summary>
    [CreateAssetMenu(menuName = "Assets/Create/Terrain2D/MapGenSettings")]
    public class MapGeneratorSettings : ScriptableObject
    {
        /// <summary>
        /// Used to scale the Noise Map for the terrain chunks
        /// </summary>
        public float NoiseMapXScale;
        public float NoiseMapYScale;
    
        /// <summary>
        /// This is the 
        /// </summary>
        public int MapWidth;
    
        public int MapHeight;

        public int AirChunks;
        public int BaseChunks;

        //On top of these chunks the generated terrain gets added on 
        public int GroundChunks;

        /// <summary>
        /// If this is -1 a random Seed will be used
        /// </summary>
        public int Seed;
        
        /// <summary>
        /// You can add custom passes that should be executed when generating the map in this field. 
        /// </summary>
        public ScriptableGenerationPassSubscriber[] PassSubscribers;
        
        public void OnValidate()
        {
            if (MapHeight <= AirChunks + BaseChunks + GroundChunks)
                MapHeight = AirChunks + BaseChunks + GroundChunks + 1;
        }
    }
}