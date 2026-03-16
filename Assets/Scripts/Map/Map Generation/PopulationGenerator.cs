using UnityEngine;

public class PopulationGenerator : IMapGeneratorStep
{
    private readonly MapConfig config;
    private readonly int randomSeed;

    // Range used to sample a different area in Perlin noise space.
    // Large range ensures different maps for different seeds.
    [SerializeField] private int NoiseOffsetMin = -10000;
    [SerializeField] private int NoiseOffsetMax = 10000;

    // Frequency multiplier for the secondary noise layer.
    [SerializeField, Tooltip("Higher value = smaller and more detailed variations.")]
    private float SecondaryFrequencyMultiplier = 2f;

    // Blending weights between large-scale and small-scale noise.
    [Range(0, 1), Tooltip("Must sum to 1 for normalized blending.")]
    [SerializeField] private float BaseNoiseWeight = 0.7f;
    [Range(0, 1), Tooltip("Must sum to 1 for normalized blending.")]
    [SerializeField] private float SecondaryNoiseWeight = 0.3f;

    public PopulationGenerator(MapConfig config, int seed)
    {
        this.config = config;
        this.randomSeed = seed;
    }

    public void Execute(Grid<GridCell> grid)
    {
        int width = grid.GetWidth();
        int height = grid.GetHeight();

        System.Random populationRandom = new(randomSeed);

        /*
         * Offsets are derived from the seeded random instance.
         * This guarantees:
         * - Deterministic generation (same seed = same map)
         * - Different sampling regions in Perlin space per run
         * 
         * Conceptually, we are shifting the "camera position"
         * in infinite Perlin noise space.
         */
        float baseOffsetX = populationRandom.Next(NoiseOffsetMin, NoiseOffsetMax);
        float baseOffsetY = populationRandom.Next(NoiseOffsetMin, NoiseOffsetMax);

        float secondaryOffsetX = populationRandom.Next(NoiseOffsetMin, NoiseOffsetMax);
        float secondaryOffsetY = populationRandom.Next(NoiseOffsetMin, NoiseOffsetMax);

        float baseScale = config.populationNoiseScale;
        float secondaryScale = config.populationNoiseScale * SecondaryFrequencyMultiplier;

        /*
         * Population generation algorithm:
         * 
         * 1. Sample a large-scale Perlin noise layer (macro population distribution).
         * 2. Sample a higher-frequency Perlin noise layer (local population variation).
         * 3. Blend both layers using weighted interpolation.
         * 
         * This creates:
         * - Large coherent high/low population regions
         * - Natural local variation inside each region
         * 
         * Result range: approximately [0.1, 0.6]
         */
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // ----- Base Layer (Large-Scale Population Structure) -----
                float baseSampleX = (x + baseOffsetX) * baseScale;
                float baseSampleY = (y + baseOffsetY) * baseScale;

                float baseNoise = Mathf.PerlinNoise(baseSampleX, baseSampleY);

                // ----- Secondary Layer (Small-Scale Population Variation) -----
                float secondarySampleX = (x + secondaryOffsetX) * secondaryScale;
                float secondarySampleY = (y + secondaryOffsetY) * secondaryScale;

                float secondaryNoise = Mathf.PerlinNoise(secondarySampleX, secondarySampleY);

                // ----- Multi-Layer Blending -----
                // Weighted combination to preserve macro distribution
                // while adding fine-grained density variation.
                float population =
                    BaseNoiseWeight * baseNoise +
                    SecondaryNoiseWeight * secondaryNoise;

                population = Mathf.Clamp(population, 0.1f, 0.6f); // Ensure final value is in [0.1, 0.6]

                grid.SetPopulationCell(x, y, population);
            }
        }
    }
}
