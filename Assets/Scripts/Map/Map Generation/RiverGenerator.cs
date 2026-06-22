using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RiverGenerator : IMapGeneratorStep
{
    private readonly MapConfig config;
    private readonly int randomSeed;

    const int MaxStartAttempts = 100;

    private readonly float targetRatio;
    private int totalCell;

    public RiverGenerator(MapConfig config, int seed)
    {
        this.config = config;
        this.randomSeed = seed;

        targetRatio = config.maxWaterRatio;
    }

    public void Execute(Grid<GridCell> grid)
    {
        int w = grid.GetWidth();
        int h = grid.GetHeight();
        float baseSize = Mathf.Sqrt(w * w + h * h);

        totalCell = w * h;

        System.Random riverRandom = new(randomSeed);

        int riverTarget = riverRandom.Next(config.minRiverCount, config.maxRiverCount + 1);
        if (riverTarget <= 0) return;

        for (int i = 0; i < riverTarget; i++)
        {
            for (int attempt = 0; attempt < MaxStartAttempts; attempt++)
            {
                Vector2Int start = Utility.GetRandomPosition(grid, riverRandom);

                if (!CheckIsValidPosForRiver(grid, start))
                    continue;

                int minRadius = Mathf.RoundToInt(config.minMountainRadiusPercent * baseSize);

                List<Vector2Int> cells = GetStampCells(grid, start, minRadius);

                if (!CanPlaceWaterCells(grid, cells))
                    continue;

                RiverData riverData = GenerateRiver(grid, i, start, baseSize, riverRandom);
                if (riverData != null && riverData.length > 0)
                {
                    grid.AddRiverData(riverData);
                    break;
                }
            }
        }
    }

    private RiverData GenerateRiver(Grid<GridCell> grid, int riverId, Vector2Int start, float baseSize, System.Random random)
    {
        RiverData riverData = new();

        float minLength = config.minRiverLengthPercent * baseSize;
        float maxLength = config.maxRiverLengthPercent * baseSize;

        // First segment
        int previousRadius = GetRandomRadius(baseSize, random);

        RiverSegment firstSegment = new();
        List<Vector2Int> startCells = GetStampCells(grid, start, previousRadius);

        if (!CanPlaceWaterCells(grid, startCells)) return riverData;

        StampRiver(grid, startCells, firstSegment);

        riverData.id = riverId;

        int targetLength = Mathf.FloorToInt(Mathf.Lerp(minLength, maxLength, (float)random.NextDouble()));
        targetLength--;

        int actualLength = 1;

        Vector2Int current = start;
        Vector2Int preferredDirection = Utility.GetRandom8Direction(random);

        for (int i = 0; i < targetLength; i++)
        {
            int radius = GetRandomRadius(baseSize, random);

            if (i > 0) preferredDirection = RandomizeDirection(preferredDirection, random);

            List<Vector2Int> directions = new() { preferredDirection };

            foreach (Vector2Int dir in Utility.NeighborCardinalDirections)
            {
                if (dir != preferredDirection)
                    directions.Add(dir);
            }

            // Shuffle sub directions
            for (int j = 1; j < directions.Count; j++)
            {
                int swapIndex = random.Next(j, directions.Count);

                (directions[j], directions[swapIndex]) = (directions[swapIndex], directions[j]);
            }

            bool foundValid = false;
            List<Vector2Int> validCells = null;
            foreach (Vector2Int dir in directions)
            {
                int distance = previousRadius + radius - 1;

                Vector2Int candidatePos = current + dir * distance;

                if (!CheckIsValidPosForRiver(grid, candidatePos))
                    continue;

                List<Vector2Int> cells = GetStampCells(grid, candidatePos, radius);

                if (!CanPlaceWaterCells(grid, cells))
                    continue;

                current = candidatePos;
                validCells = cells;
                preferredDirection = dir;
                previousRadius = radius;

                foundValid = true;
                break;
            }

            if (!foundValid)
                break;

            RiverSegment segment = new();
            StampRiver(grid, validCells, segment);
            riverData.segments.Add(segment);
            actualLength++;
        }

        riverData.length = actualLength;
        return riverData;
    }

    public void StampRiver(Grid<GridCell> grid, List<Vector2Int> cells, RiverSegment segment)
    {
        bool[,] water = grid.GetWaterGrid();
        bool[,] mountain = grid.GetMountainGrid();

        foreach (var c in cells)
        {
            if (water[c.x, c.y] || mountain[c.x, c.y])
                continue;

            grid.SetWaterCell(c.x, c.y, true);

            segment.stampedCells.Add(c);

            grid.IncrementWaterCellCount();
        }
    }

    private bool CanPlaceWaterCells(Grid<GridCell> grid, List<Vector2Int> cells)
    {
        bool[,] water = grid.GetWaterGrid();
        int incrementalWaterCells = 0;

        foreach (var c in cells)
            if (!water[c.x, c.y] && !grid.GetMountainGrid()[c.x, c.y])
                incrementalWaterCells++;

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
                int nx = center.x + dx;
                int ny = center.y + dy;

                if (!grid.IsInBounds(nx, ny))
                    continue;

                cells.Add(new Vector2Int(nx, ny));
            }
        }

        return cells;
    }

    private Vector2Int RandomizeDirection(Vector2Int currentDir, System.Random random)
    {
        int roll = random.Next(100);

        if (roll < 65)
            return currentDir;

        if (roll < 80)
            return new Vector2Int(-currentDir.y, currentDir.x);

        if (roll < 95)
            return new Vector2Int(currentDir.y, -currentDir.x);

        return Utility.GetRandomCardinalDirection(random);
    }

    private int GetRandomRadius(float baseSize, System.Random random)
    {
        float minRadius = config.minRiverRadiusPercent * baseSize;
        float maxRadius = config.maxRiverRadiusPercent * baseSize;
        return Mathf.FloorToInt(Mathf.Lerp(minRadius, maxRadius, (float)random.NextDouble()));
    }

    public bool CheckIsValidPosForRiver(Grid<GridCell> grid, Vector2Int pos)
    {
        return grid.IsInBounds(pos.x, pos.y)
            && !grid.GetMountainGrid()[pos.x, pos.y];
    }
}
