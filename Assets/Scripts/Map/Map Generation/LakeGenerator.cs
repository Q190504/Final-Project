using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates lakes on the map by randomly placing circular water areas.
/// Deterministic if the same seed is used.
/// </summary>
public class LakeGenerator : IMapGeneratorStep
{
    private readonly MapConfig config;
    private readonly int randomSeed;

    private readonly float targetRatio;
    private int totalCell;

    private readonly HashSet<Vector2Int> lakeCells = new();

    public LakeGenerator(MapConfig config, int seed)
    {
        this.config = config;
        this.randomSeed = seed;

        targetRatio = config.maxWaterRatio;
    }

    public void Execute(Grid<GridCell> grid)
    {
        // Seeded random ensures same map with same seed
        System.Random lakeRandom = new(randomSeed);

        // Random number of lakes within configured range
        int lakeTarget = lakeRandom.Next(config.minLakeCount, config.maxLakeCount);

        if (lakeTarget <= 0) return;

        // Random lake size (radius)
        int w = grid.GetWidth();
        int h = grid.GetHeight();
        totalCell = w * h;

        float baseSize = Mathf.Sqrt(w * w + h * h);

        for (int i = 0; i < lakeTarget; i++)
        {
            List<Vector2Int> riverCandidates = new();
            List<Vector2Int> normalCandidates = new();

            foreach (GridCell cell in grid.GetGrid())
            {
                Vector2Int pos = new(cell.X, cell.Y);

                if (!CheckIsValidPosForLakeCenter(grid, pos))
                    continue;

                int minRadius = Mathf.RoundToInt(config.minLakeRadiusPercent * baseSize);
                List<Vector2Int> cells = GetStampCells(grid, pos, minRadius);

                if (!CanPlaceWaterCells(grid, cells))
                    continue;

                if (grid.GetWaterGrid()[cell.X, cell.Y])
                {
                    if (!lakeCells.Contains(pos))
                        riverCandidates.Add(pos);
                }
                else
                {
                    if (!grid.GetMountainGrid()[cell.X, cell.Y])
                        normalCandidates.Add(pos);
                }
            }

            if (riverCandidates.Count == 0 && normalCandidates.Count == 0)
            {
                Debug.LogWarning("Can't find valid pos for lake");
                return;
            }

            Vector2Int start;
            if (riverCandidates.Count > 0 && lakeRandom.NextDouble() < config.chanceLakeSpawnOnRiver)
            {
                start = riverCandidates[lakeRandom.Next(riverCandidates.Count)];
            }
            else
            {
                List<Vector2Int> source = normalCandidates.Count > 0 ? normalCandidates : riverCandidates;
                start = source[lakeRandom.Next(source.Count)];
            }

            int radius = GetRandomRadius(baseSize, lakeRandom);

            // Fill circular area with water cells
            if (!CreateLake(grid, start.x, start.y, radius))
                i--;
        }
    }

    public bool CreateLake(Grid<GridCell> grid, int cx, int cy, int radius)
    {
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                int nx = cx + dx;
                int ny = cy + dy;

                Vector2Int currentCell = new(nx, ny);

                if (!CheckIsValidPosForLake(grid, currentCell))
                    continue;

                grid.SetWaterCell(nx, ny, true);
                lakeCells.Add(currentCell);
                grid.IncrementWaterCellCount();
            }
        }

        return true;
    }

    private bool CanPlaceWaterCells(Grid<GridCell> grid, List<Vector2Int> cells)
    {
        bool[,] water = grid.GetWaterGrid();
        int incrementalWaterCells = 0;

        foreach (var c in cells)
            if (!water[c.x, c.y] && !grid.GetMountainGrid()[c.x, c.y])
                incrementalWaterCells++;

        if (incrementalWaterCells <= 0) return false;

        float tempRatio = ((float)grid.GetWaterCellCount() + incrementalWaterCells) / totalCell;

        return tempRatio <= targetRatio;
    }

    private List<Vector2Int> GetStampCells(Grid<GridCell> grid, Vector2Int center, int radius)
    {
        List<Vector2Int> cells = new();

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx * dx + dy * dy > radius * radius)
                    continue;

                int nx = center.x + dx;
                int ny = center.y + dy;

                if (!grid.IsInBounds(nx, ny))
                    continue;

                // Check if cell lies inside the circle
                if (dx * dx + dy * dy <= radius * radius)
                {
                    cells.Add(new Vector2Int(nx, ny));
                }
            }
        }

        return cells;
    }

    private bool IsLessOEqualThanWaterTargetRatio(Grid<GridCell> grid)
    {
        float currentRatio = (float)grid.GetWaterCellCount() / totalCell;
        return currentRatio <= targetRatio;
    }

    private int GetRandomRadius(float baseSize, System.Random random)
    {
        float minRadius = config.minLakeRadiusPercent * baseSize;
        float maxRadius = config.maxLakeRadiusPercent * baseSize;
        return Mathf.FloorToInt(Mathf.Lerp(minRadius, maxRadius, (float)random.NextDouble()));
    }

    public bool CheckIsValidPosForLake(Grid<GridCell> grid, Vector2Int pos)
    {
        return grid.IsInBounds(pos.x, pos.y)
            && !grid.GetMountainGrid()[pos.x, pos.y];
    }

    public bool CheckIsValidPosForLakeCenter(Grid<GridCell> grid, Vector2Int pos)
    {
        return grid.IsInBounds(pos.x, pos.y)
            && !grid.GetMountainGrid()[pos.x, pos.y]
            && !IsInsideExistingLake(pos);
    }

    private bool IsInsideExistingLake(Vector2Int pos)
    {
        return lakeCells.Contains(pos);
    }
}