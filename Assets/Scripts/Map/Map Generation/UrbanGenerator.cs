using System.Collections.Generic;
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

    private readonly Vector2 targetRatioRange;
    private int totalCell;


    public UrbanGenerator(MapConfig config, int seed)
    {
        this.config = config;
        this.randomSeed = seed;

        targetRatioRange = config.urbanRatioRange;
    }

    /// <summary>
    /// Main entry point of this generation step.
    /// Creates multiple population centers and blends them together.
    /// </summary>
    public void Execute(Grid<GridCell> grid)
    {
        PopulationData highPopulationData = PropertyDataManager.Instance.GetPopulationData(PopulationType.High);
        int w = grid.GetWidth();
        int h = grid.GetHeight();
        totalCell = w * h;

        if (!IsLesserOrEqualToMaxRatio(grid, highPopulationData.minPopulationValue)) return;

        // Use seeded random to guarantee same map with same seed
        System.Random populationRandom = new(randomSeed);

        float baseSize = Mathf.Sqrt(w * w + h * h);

        while (!IsGreaterOrEqualToMinRatio(grid, highPopulationData.minPopulationValue))
        {
            bool createdAnyUrban = false;

            // Randomly decide how many population centers will appear
            int centerCount = populationRandom.Next(config.minUrbans, config.maxUrbans + 1);

            for (int i = 0; i < centerCount; i++)
            {
                if (!IsLesserOrEqualToMaxRatio(grid, highPopulationData.minPopulationValue)) return;

                List<Vector2Int> validCenters = new();

                foreach (GridCell cell in grid.GetGrid())
                {
                    Vector2Int pos = new(cell.X, cell.Y);

                    if (IsValidPosForUrbanCenter(grid, pos, highPopulationData.minPopulationValue))
                    {
                        int minRadius = Mathf.FloorToInt(config.minUrbanRadiusPercent * baseSize);
                        List<Vector2Int> cells = GetStampCells(grid, pos.x, pos.y, minRadius);

                        if (CanCreateUrban(grid, pos.x, pos.y, cells, minRadius, highPopulationData))
                            validCenters.Add(pos);
                    }
                }

                if (validCenters.Count == 0)
                {
                    Debug.LogWarning("Can't find valid position for urban.");
                    break;
                }

                Vector2Int center = validCenters[populationRandom.Next(validCenters.Count)];
                int cx = center.x;
                int cy = center.y;

                if (CreateUrban(grid, cx, cy, baseSize, populationRandom, highPopulationData))
                    createdAnyUrban = true;
            }

            if (!createdAnyUrban)
            {
                Debug.LogWarning("Urban generation stopped because no more urban can be created.");
                break;
            }
        }
    }

    private bool CreateUrban(Grid<GridCell> grid, int cx, int cy, float mapBaseSize, System.Random populationRandom, PopulationData highPopulationData)
    {
        float minRadiusFloat = config.minUrbanRadiusPercent * mapBaseSize;
        float maxRadiusFloat = config.maxUrbanRadiusPercent * mapBaseSize;

        int minRadius = Mathf.FloorToInt(minRadiusFloat);
        int currentMaxRadius = Mathf.FloorToInt(maxRadiusFloat);

        while (currentMaxRadius >= minRadius)
        {
            int radius = populationRandom.Next(minRadius, currentMaxRadius + 1);

            List<Vector2Int> cells = GetStampCells(grid, cx, cy, radius);

            if (CanCreateUrban(grid, cx, cy, cells, radius, highPopulationData))
            {
                ApplyGaussianForPopulation(grid, cx, cy, cells, highPopulationData.maxPopulationValue, radius);

                return true;
            }

            currentMaxRadius = radius - 1;
        }

        return false;
    }

    /// <summary>
    /// Applies a 2D Gaussian distribution centered at (cx, cy).
    /// Adds its contribution to the population grid.
    /// 
    /// Formula:
    /// value = strength * e^(-distance^2 / (2 * radius^2))
    /// 
    /// This creates a smooth "city-like" density falloff.
    /// </summary>
    private void ApplyGaussianForPopulation(Grid<GridCell> grid, int cx, int cy, List<Vector2Int> cells, float strength, float radius)
    {
        float[,] pop = grid.GetPopulationGrid();
        float radius2 = radius * radius;

        foreach (Vector2Int cell in cells)
        {
            float dx = cell.x - cx;
            float dy = cell.y - cy;
            float dist2 = dx * dx + dy * dy;

            if (dist2 > radius2)
                continue;

            float value = strength * Mathf.Exp(-dist2 / (2 * radius2));

            pop[cell.x, cell.y] += value;
            pop[cell.x, cell.y] = Mathf.Min(pop[cell.x, cell.y], 1f);
        }
    }

    private int CountSuccessfulUpgradeToUrbanCells(Grid<GridCell> grid, int cx, int cy, List<Vector2Int> cells, float radius, float strength,
        float minUrbanPopulationValue)
    {
        float[,] pop = grid.GetPopulationGrid();

        float radius2 = radius * radius;

        int successfulCells = 0;

        foreach (Vector2Int cell in cells)
        {
            float dx = cell.x - cx;
            float dy = cell.y - cy;
            float dist2 = dx * dx + dy * dy;

            if (dist2 > radius2)
                continue;

            float value = strength * Mathf.Exp(-dist2 / (2 * radius2));

            bool before = pop[cell.x, cell.y] >= minUrbanPopulationValue;
            bool after = pop[cell.x, cell.y] + value >= minUrbanPopulationValue;

            if (!before && after)
                successfulCells++;
        }

        return successfulCells;
    }

    private List<Vector2Int> GetStampCells(Grid<GridCell> grid, int cx, int cy, int radius)
    {
        List<Vector2Int> cells = new();

        int w = grid.GetWidth();
        int h = grid.GetHeight();

        float radius2 = radius * radius;

        int minX = Mathf.Max(0, Mathf.FloorToInt(cx - radius));
        int maxX = Mathf.Min(w - 1, Mathf.CeilToInt(cx + radius));

        int minY = Mathf.Max(0, Mathf.FloorToInt(cy - radius));
        int maxY = Mathf.Min(h - 1, Mathf.CeilToInt(cy + radius));

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (!IsValidPosToHavePopulation(grid, x, y))
                    continue;

                float dx = x - cx;
                float dy = y - cy;
                float dist2 = dx * dx + dy * dy;

                if (dist2 > radius2)
                    continue;

                cells.Add(new Vector2Int(x, y));
            }
        }

        return cells;
    }

    private bool CanCreateUrban(Grid<GridCell> grid, int cx, int cy, List<Vector2Int> cells, int radius, PopulationData highPopulationData)
    {
        float[,] populationGrid = grid.GetPopulationGrid();
        int incrementalUrbanCells = CountSuccessfulUpgradeToUrbanCells(grid, cx, cy, cells, radius, highPopulationData.maxPopulationValue,
            highPopulationData.minPopulationValue);

        if (incrementalUrbanCells <= 0) return false;

        int currentUrbanCells = 0;
        foreach (float population in populationGrid)
            if (population >= highPopulationData.minPopulationValue)
                currentUrbanCells++;

        float tempRatio = ((float)currentUrbanCells + incrementalUrbanCells) / totalCell;

        return tempRatio <= targetRatioRange.y;
    }

    private bool IsLesserOrEqualToMaxRatio(Grid<GridCell> grid, float minUrbanPopulationValue)
    {
        float[,] populationGrid = grid.GetPopulationGrid();
        int currentUrbanCells = 0;
        foreach (float population in populationGrid)
            if (population >= minUrbanPopulationValue)
                currentUrbanCells++;

        float ratio = ((float)currentUrbanCells) / totalCell;

        return ratio <= targetRatioRange.y;
    }

    private bool IsGreaterOrEqualToMinRatio(Grid<GridCell> grid, float minUrbanPopulationValue)
    {
        float[,] populationGrid = grid.GetPopulationGrid();
        int currentUrbanCells = 0;
        foreach (float population in populationGrid)
            if (population >= minUrbanPopulationValue)
                currentUrbanCells++;

        float tempRatio = ((float)currentUrbanCells) / totalCell;

        return tempRatio >= targetRatioRange.x;
    }

    private bool IsValidPosToHavePopulation(Grid<GridCell> grid, int x, int y)
    {
        return grid.IsInBounds(x, y) &&
            !grid.GetMountainGrid()[x, y]
            && !grid.GetWaterGrid()[x, y];
    }

    public static bool IsValidPosForUrbanCenter(Grid<GridCell> grid, Vector2Int pos, float minUrbanPopulationValue)
    {
        return grid.IsInBounds(pos.x, pos.y)
            && !grid.GetWaterGrid()[pos.x, pos.y]
            && !grid.GetMountainGrid()[pos.x, pos.y]
            && grid.GetPopulationGrid()[pos.x, pos.y] < minUrbanPopulationValue;
    }
}