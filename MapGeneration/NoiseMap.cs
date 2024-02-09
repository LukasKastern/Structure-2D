using System.Collections.Generic;

namespace Structure2D.MapGeneration
{
    public class NoiseMap
    {
        private static TerrainNoise Noise;

        /// <summary>
        /// Use this to use your custom noise map instead of the default one.
        /// </summary>
        /// <param name="noise"></param>
        public static void SetCustomNoiseMap(TerrainNoise noise)
        {
            Noise = noise;
        }
        
        public static IEnumerable<float> GetTerrain(int width, int height, int seed)
        {
            if(Noise == null)
                Noise = new TerrainNoise();

            foreach (var noise in Noise.GetTerrain(width, height, seed))
            {
                yield return noise;
            }  
        }

        public static void SetScale(float settingsNoiseMapXScale, float settingsNoiseMapYScale)
        {      
            if(Noise == null)
                Noise = new TerrainNoise();
        
            Noise.SetScale(settingsNoiseMapXScale, settingsNoiseMapYScale);
        }
    }
}