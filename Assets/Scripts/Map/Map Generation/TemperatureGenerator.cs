using UnityEngine;

public class TemperatureGenerator : IMapGeneratorStep
{
    private readonly MapConfig config;
    private readonly int randomSeed;

    // Range used to sample a different area in Perlin noise space.
    // Large range ensures different maps for different seeds.
    private int noiseOffsetMin = -100000;
    private int noiseOffsetMax = 100000;

    // Frequency multiplier for the secondary noise layer.
    // Higher value = smaller and more detailed variations.
    private float secondaryFrequencyMultiplier = 2f;

    // Blending weights between large-scale and small-scale noise.
    // Must sum to 1 for normalized blending
    private float baseNoiseWeight = 0.7f;
    // Must sum to 1 for normalized blending
    private float secondaryNoiseWeight = 0.3f;

    public TemperatureGenerator(MapConfig config, int seed)
    {
        this.config = config;
        this.randomSeed = seed;
    }

    public void Execute(Grid<GridCell> grid)
    {
        int width = grid.GetWidth();
        int height = grid.GetHeight();

        System.Random temperatureRandom = new(randomSeed);

        /*
         * Offsets are derived from the seeded random instance.
         * This guarantees:
         * - Deterministic generation (same seed = same map)
         * - Different sampling regions in Perlin space per run
         * 
         * Conceptually, we are shifting the "camera position"
         * in infinite Perlin noise space.
         */
        float baseOffsetX = temperatureRandom.Next(noiseOffsetMin, noiseOffsetMax);
        float baseOffsetY = temperatureRandom.Next(noiseOffsetMin, noiseOffsetMax);

        float secondaryOffsetX = temperatureRandom.Next(noiseOffsetMin, noiseOffsetMax);
        float secondaryOffsetY = temperatureRandom.Next(noiseOffsetMin, noiseOffsetMax);

        float baseScale = config.temperatureNoiseScale;
        float secondaryScale = config.temperatureNoiseScale * secondaryFrequencyMultiplier;

        /*
         * Temperature generation algorithm:
         * 
         * 1. Sample a large-scale Perlin noise layer (macro climate).
         * 2. Sample a higher-frequency Perlin noise layer (micro variation).
         * 3. Blend both layers using weighted interpolation.
         * 
         * This creates:
         * - Large coherent hot/cold regions
         * - Natural local variation inside each region
         * 
         * Result range: approximately [0, 1]
         */
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // ----- Base Layer (Large-Scale Climate Structure) -----
                float baseSampleX = (x + baseOffsetX) * baseScale;
                float baseSampleY = (y + baseOffsetY) * baseScale;

                float baseNoise = Mathf.PerlinNoise(baseSampleX, baseSampleY);

                // ----- Secondary Layer (Small-Scale Variation) -----
                float secondarySampleX = (x + secondaryOffsetX) * secondaryScale;
                float secondarySampleY = (y + secondaryOffsetY) * secondaryScale;

                float secondaryNoise = Mathf.PerlinNoise(secondarySampleX, secondarySampleY);

                // ----- Multi-Layer Blending -----
                // Weighted combination to preserve macro structure
                // while adding fine details.
                float temperature =
                    baseNoiseWeight * baseNoise +
                    secondaryNoiseWeight * secondaryNoise;

                grid.SetTemperatureCell(x, y, temperature);
            }
        }
    }
}
