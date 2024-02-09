using System.Collections.Generic;
using AccidentalNoise;

namespace Structure2D.MapGeneration
{
    /// <summary>
    /// This is the Noise base class
    /// To add your custom Noise Map all you have to do is override the GetTerrain function and call SetCustomNoiseMap on the NoiseMap class
    /// </summary> 
    public class TerrainNoise
    {
        private float relativeScaleX = 0;
        private float relativeScaleY = 0;

        private const int baseWidth = 100 * CellMetrics.ChunkSize;
        private const int baseHeight = 5 * CellMetrics.ChunkSize;

        protected float GetXScale(int width)
        {
            return (float) width / baseWidth * relativeScaleX;
        }

        protected float GetYScale(int height)
        {
            return (float) height / baseHeight * relativeScaleY;
        }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public virtual IEnumerable<float> GetTerrain(int width, int height, int seed)
        {
            AccidentalNoise.Gradient gradient = new AccidentalNoise.Gradient(0, 0, 0, 1);

            Fractal fractal = new Fractal(FractalType.FBM, BasisTypes.GRADIENT, InterpTypes.QUINTIC, 3, 0.5,
                (uint) seed);
            AutoCorrect autoCorrect = new AutoCorrect(fractal, 0, 1);
            Cache cache = new Cache(autoCorrect);

            var highland = GetHighland(seed, ref gradient);

            var mountains = GetMountain(seed, ref gradient);
            var lowLand = GetLowLandDomain(seed, ref gradient);

            Select highlandMountainSelect = new Select(cache, highland, mountains, 0.55, 0.2);
            Select highland_lowland = new Select(cache, lowLand, highlandMountainSelect, 0.25, 0.15);

            ScaleDomain scale = new ScaleDomain(highland_lowland, GetXScale(width), GetYScale(height));
            Select groundSelect = new Select(scale, 0, 1, 0.5f, null);

            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    yield return (float) groundSelect.Get((float) x / width, (float) y / height);
                }
            }
        }

        private static TranslatedDomain GetLowLandDomain(int seed, ref AccidentalNoise.Gradient gradient)
        {
            Fractal fractal = new Fractal(FractalType.FBM, BasisTypes.GRADIENT, InterpTypes.QUINTIC, 2, 1, (uint) seed);
            AutoCorrect correct = new AutoCorrect(fractal, 0, 1);
            ScaleOffset offset = new ScaleOffset(0.2, 0.25, correct);
            ScaleDomain scale = new ScaleDomain(offset, null, 0);
            TranslatedDomain domain = new TranslatedDomain(gradient, null, scale);

            return domain;
        }

        private static TranslatedDomain GetHighland(int seed, ref AccidentalNoise.Gradient gradient)
        {
            Fractal highlandShapeFractal = new Fractal(FractalType.RIDGEDMULTI, BasisTypes.GRADIENT,
                InterpTypes.QUINTIC, 2, 2, (uint) seed);
            AutoCorrect autoCorrect = new AutoCorrect(highlandShapeFractal, 0, 1);
            ScaleOffset offset = new ScaleOffset(0.45, 0, autoCorrect);
            ScaleDomain domain = new ScaleDomain(offset, null, 0);

            return new TranslatedDomain(gradient, null, domain);
        }

        private static TranslatedDomain GetMountain(int seed, ref AccidentalNoise.Gradient gradient)
        {
            Fractal highlandShapeFractal = new Fractal(FractalType.BILLOW, BasisTypes.GRADIENT, InterpTypes.QUINTIC, 4,
                1, (uint) seed);
            AutoCorrect autoCorrect = new AutoCorrect(highlandShapeFractal, 0, 1);
            ScaleOffset offset = new ScaleOffset(0.75, -0.2, autoCorrect);
            ScaleDomain domain = new ScaleDomain(offset, null, 0.1);

            return new TranslatedDomain(gradient, null, domain);

        }

        public void SetScale(float settingsNoiseMapXScale, float settingsNoiseMapYScale)
        {
            relativeScaleX = settingsNoiseMapXScale;
            relativeScaleY = settingsNoiseMapYScale;
        }
    }
}