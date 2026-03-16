using UnityEngine;

/// <summary>
/// Generate a population density map using multiple Gaussian population centers.
/// The result is a normalized float grid (0 -> 1) representing relative population density.
/// Deterministic if the same seed is used.
/// </summary>
public class UrbanGenerator : IMapGeneratorStep
{
    private readonly MapConfig config;   // Configuration values (min/max centers, etc.)
    private readonly int randomSeed;     // Seed to ensure deterministic generation

    public UrbanGenerator(MapConfig config, int seed)
    {
        this.config = config;
        this.randomSeed = seed;
    }

    /// <summary>
    /// Main entry point of this generation step.
    /// Creates multiple population centers and blends them together.
    /// </summary>
    public void Execute(Grid<GridCell> grid)
    {
        int w = grid.GetWidth();
        int h = grid.GetHeight();

        // Use seeded random to guarantee same map with same seed
        System.Random populationRandom = new(randomSeed);

        // Randomly decide how many population centers will appear
        int centerCount = populationRandom.Next(
            config.minUrbans,
            config.maxUrbans + 1);

        PopulationData highPopulationData = CellPropertyManager.Instance.GetPopulationData(PopulationType.High);

        for (int i = 0; i < centerCount; i++)
        {
            Utility.CreateUrban(grid, w, h, config, populationRandom, highPopulationData);
        }

        grid.NormalizePopulationMap();
    }
}