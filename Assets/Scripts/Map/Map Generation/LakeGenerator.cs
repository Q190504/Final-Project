using UnityEngine;

/// <summary>
/// Generates lakes on the map by randomly placing circular water areas.
/// Deterministic if the same seed is used.
/// </summary>
public class LakeGenerator : IMapGeneratorStep
{
    private readonly MapConfig config;
    private readonly int randomSeed;

    public LakeGenerator(MapConfig config, int seed)
    {
        this.config = config;
        this.randomSeed = seed;
    }

    /// <summary>
    /// Main execution method.
    /// Randomly determines how many lakes to generate,
    /// then places each lake at a random position with a random radius.
    /// </summary>
    public void Execute(Grid<GridCell> grid)
    {
        // Seeded random ensures same map with same seed
        System.Random lakeRandom = new(randomSeed);

        // Random number of lakes within configured range
        int lakeCount = lakeRandom.Next(config.minLakeCount, config.maxLakeCount);

        // Random lake size (radius)
        int w = grid.GetWidth();
        int h = grid.GetHeight();

        float baseSize = Mathf.Sqrt(w * w + h * h);
        float minRadius = config.minLakeRadiusPercent * baseSize;
        float maxRadius = config.maxLakeRadiusPercent * baseSize;
        int radius;

        for (int i = 0; i < lakeCount; i++)
        {
            int x;
            int y;

            do
            {
                // Random lake center position
                x = lakeRandom.Next(grid.GetWidth());
                y = lakeRandom.Next(grid.GetHeight());
            } while (Utility.CheckIsValidPosForRiverOrLake(grid, new Vector2Int(x, y)));

            radius = Mathf.FloorToInt(Mathf.Lerp(minRadius, maxRadius, (float)lakeRandom.NextDouble()));

            // Fill circular area with water cells
            Utility.FillCircleWithWater(grid, x, y, radius, true);
            grid.AddLakeCenterCell(new Vector3Int(x, y, radius));
        }
    }
}