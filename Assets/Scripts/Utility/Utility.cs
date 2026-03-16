using System;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static readonly Vector2Int[] Neighbor8Directions =
    {
        new Vector2Int(-1, -1),
        new Vector2Int( 0, -1),
        new Vector2Int( 1, -1),
        new Vector2Int(-1,  0),
        new Vector2Int( 1,  0),
        new Vector2Int(-1,  1),
        new Vector2Int( 0,  1),
        new Vector2Int( 1,  1),
    };

    public static readonly Vector2Int[] NeighborCardinalDirections =
    {
        new Vector2Int( 0, -1),
        new Vector2Int(-1,  0),
        new Vector2Int( 1,  0),
        new Vector2Int( 0,  1),
    };

    public static Vector3 GetMouseWorldPosition()
    {
        Vector3 vector = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        vector.z = 0;
        return vector;
    }

    public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
    {
        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }

    public static Vector3 GridToWorldPosition(int x, int y, float width, float height, float cellSize)
    {
        float offsetX = (width - 1) * cellSize * 0.5f;
        float offsetY = (height - 1) * cellSize * 0.5f;

        return new Vector3(
            x * cellSize - offsetX,
            y * cellSize - offsetY,
            0f
        );
    }

    public static Vector2Int WorldToGridPosition(
    Vector3 worldPos,
    int width,
    int height,
    float cellSize)
    {
        float offsetX = (width - 1) * cellSize * 0.5f;
        float offsetY = (height - 1) * cellSize * 0.5f;

        int x = Mathf.FloorToInt((worldPos.x + offsetX) / cellSize);
        int y = Mathf.FloorToInt((worldPos.y + offsetY) / cellSize);

        return new Vector2Int(x, y);
    }

    public static int GetCellIndex(int x, int y, int width)
    {
        return x + y * width;
    }

    public static List<Vector2Int> Get8Neighbors(Grid<GridCell> grid, Vector2Int pos)
    {
        List<Vector2Int> neighbors = new(Neighbor8Directions.Length);

        foreach (var dir in Neighbor8Directions)
        {
            Vector2Int neighborPos = pos + dir;

            if (!grid.IsInBounds(neighborPos.x, neighborPos.y))
                continue;

            neighbors.Add(neighborPos);
        }

        return neighbors;
    }

    public static void Get8Neighbors(Grid<GridCell> grid, Vector2Int pos, List<Vector2Int> result)
    {
        result.Clear();

        foreach (var dir in Neighbor8Directions)
        {
            Vector2Int neighborPos = pos + dir;

            if (!grid.IsInBounds(neighborPos.x, neighborPos.y))
                continue;

            result.Add(neighborPos);
        }
    }

    public static Vector2Int GetRandomPosition(
    Grid<GridCell> grid,
    System.Random random)
    {
        return new Vector2Int(
            random.Next(0, grid.GetWidth() - 1),
            random.Next(0, grid.GetHeight() - 1));
    }

    public static Vector2Int GetRandomCardinalDirection(System.Random random)
    {
        int dir = random.Next(0, 4);

        return dir switch
        {
            0 => NeighborCardinalDirections[0],
            1 => NeighborCardinalDirections[1],
            2 => NeighborCardinalDirections[2],
            _ => NeighborCardinalDirections[3],
        };
    }

    public static Vector2Int GetRandom8Direction(System.Random random)
    {
        int dir = random.Next(0, 8);

        return dir switch
        {
            0 => Neighbor8Directions[0],
            1 => Neighbor8Directions[1],
            2 => Neighbor8Directions[2],
            3 => Neighbor8Directions[3],
            4 => Neighbor8Directions[4],
            5 => Neighbor8Directions[5],
            6 => Neighbor8Directions[6],
            _ => Neighbor8Directions[7],
        };
    }

    public static bool IsEnviromentMatch(CellStats stats, List<EnvironmentType> environmentTypes)
    {
        foreach (var envType in environmentTypes)
        {
            if (stats.environment.currentEnvironmentType == envType)
                return true;
        }
        return false;
    }

    public static bool IsUrban(EnvironmentType type)
    {
        return type == EnvironmentType.NormalUrban
            || type == EnvironmentType.HotUrban
            || type == EnvironmentType.ColdUrban;
    }

    public static void Shuffle<T>(List<T> list, System.Random random)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1); // 0 <= j <= i
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    /// Fills a circular area centered at (cx, cy) with water.
    /// Uses the circle equation:
    ///     dx^2 + dy^2 <= radius^2
    /// to determine whether a cell lies inside the circle.
    /// </summary>
    public static void FillCircleWithWater(
        Grid<GridCell> grid,
        int cx,
        int cy,
        int radius,
        bool value)
    {
        // Iterate over square bounding the circle
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                int nx = cx + dx;
                int ny = cy + dy;

                // Skip if outside grid boundaries
                if (!grid.IsInBounds(nx, ny))
                    continue;

                // Check if cell lies inside the circle
                if (dx * dx + dy * dy <= radius * radius)
                {
                    if (value && !grid.GetMountainGrid()[nx, ny])
                    {
                        grid.SetWaterCell(nx, ny, value);
                        grid.IncrementWaterCellCount();
                    }
                    else if (!value && grid.GetWaterGrid()[nx, ny])
                    {
                        grid.SetWaterCell(nx, ny, value);
                        grid.DecrementWaterCellCount();
                    }
                }
            }
        }
    }

    public static void StampRiver(Grid<GridCell> grid, Vector2Int cell, int radius, RiverSegment segment)
    {
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                Vector2Int neighbour = new(cell.x + dx, cell.y + dy);

                if (!CheckIsValidPosForRiverOrLake(grid, neighbour))
                    continue;

                if (!grid.GetWaterGrid()[neighbour.x, neighbour.y])
                {
                    grid.SetWaterCell(neighbour.x, neighbour.y, true);
                    grid.IncrementWaterCellCount();

                    segment.stampedCells.Add(neighbour);
                }
            }
        }
    }

    public static int CreateUrban(Grid<GridCell> grid, int w, int h, MapConfig config, System.Random populationRandom, PopulationData urbanData)
    {
        int cx, cy;
        do
        {
            // Random center position
            cx = populationRandom.Next(0, w);
            cy = populationRandom.Next(0, h);
        }
        while (CheckIsValidPosForUrban(grid, new Vector2Int(cx, cy)));

        // Strength of this center (how dense it is at peak)
        float strength = urbanData.maxPopulationValue;

        // Random spread radius (how far influence extends)
        float baseSize = Mathf.Sqrt(w * w + h * h);
        float minRadius = config.minUrbanRadiusPercent * baseSize;
        float maxRadius = config.maxUrbanRadiusPercent * baseSize;
        int radius = Mathf.FloorToInt(Mathf.Lerp(minRadius, maxRadius, (float)populationRandom.NextDouble()));

        // Apply Gaussian distribution from this center
        return ApplyGaussianForPopulation(grid, cx, cy, strength, radius);
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
    private static int ApplyGaussianForPopulation(
        Grid<GridCell> grid,
        int cx,
        int cy,
        float strength,
        float radius)
    {
        int w = grid.GetWidth();
        int h = grid.GetHeight();

        float r2 = radius * radius * 2f;
        float radius2 = radius * radius;

        int minX = Mathf.Max(0, Mathf.FloorToInt(cx - radius));
        int maxX = Mathf.Min(w - 1, Mathf.CeilToInt(cx + radius));

        int minY = Mathf.Max(0, Mathf.FloorToInt(cy - radius));
        int maxY = Mathf.Min(h - 1, Mathf.CeilToInt(cy + radius));

        float[,] pop = grid.GetPopulationGrid();
        bool [,] water = grid.GetWaterGrid();
        bool[,] mountain = grid.GetMountainGrid();

        int cellsUpdated = 0;   

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (mountain[x, y] || water[x, y])
                    continue;

                float dx = x - cx;
                float dy = y - cy;
                float dist2 = dx * dx + dy * dy;

                if (dist2 > radius2)
                    continue;

                float value = strength * Mathf.Exp(-dist2 / r2);

                pop[x, y] += value;
                cellsUpdated++;
            }
        }

        return cellsUpdated;
    }

    public static bool CheckIsValidPosForRiverOrLake(Grid<GridCell> grid, Vector2Int pos)
    {
        return grid.IsInBounds(pos.x, pos.y) && !grid.GetMountainGrid()[pos.x, pos.y];
    }

    public static bool CheckIsValidPosForUrban(Grid<GridCell> grid, Vector2Int pos)
    {
        return grid.IsInBounds(pos.x, pos.y)
            && !grid.GetWaterGrid()[pos.x, pos.y]
            && !grid.GetMountainGrid()[pos.x, pos.y];
    }
}
