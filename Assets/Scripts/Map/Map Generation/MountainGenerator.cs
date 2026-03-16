using System.Collections.Generic;
using UnityEngine;

public class MountainGenerator : IMapGeneratorStep
{
    private readonly MapConfig config;
    private readonly int randomSeed;

    private readonly float targetRatio;
    private int totalCell;

    public MountainGenerator(MapConfig config, int seed)
    {
        this.config = config;
        randomSeed = seed;
        targetRatio = config.maxMountainRatio;
    }

    public void Execute(Grid<GridCell> grid)
    {
        int w = grid.GetWidth();
        int h = grid.GetHeight();
        totalCell = w * h;
        System.Random mountainRandom = new(randomSeed);

        int chainCount = mountainRandom.Next(
            config.minMountainChains,
            config.MaxMountainChains + 1);

        for (int i = 0; i < chainCount; i++)
        {
            Vector2Int start;

            do
            {
                start = Utility.GetRandomPosition(grid, mountainRandom);
            }
            while (!CheckIsValidPosForMountain(grid, start));

            //grid.AddMountainData(DrawMountain(grid, start, mountainRandom));
            DrawMountain(grid, start, mountainRandom);
        }
    }

    private bool CanPlaceMountain(
        Grid<GridCell> grid,
        List<Vector2Int> cells)
    {
        bool[,] mountain = grid.GetMountainGrid();
        int incrementalMountainCells = 0;

        #region Check mountain ratio

        foreach (var c in cells)
            if (!mountain[c.x, c.y])
                incrementalMountainCells++;

        float tempRatio = ((float)grid.GetMountainCellCount() + incrementalMountainCells) / totalCell;

        if (tempRatio > targetRatio)
            return false;

        #endregion

        #region Flood fill connectivity check

        return CheckConnectivityAfterPlacement(grid, cells);

        #endregion
    }

    private bool CheckConnectivityAfterPlacement(
    Grid<GridCell> grid,
    List<Vector2Int> newMountains)
    {
        int w = grid.GetWidth();
        int h = grid.GetHeight();

        bool[,] mountain = grid.GetMountainGrid();

        HashSet<Vector2Int> tempMountain = new(newMountains);

        bool[,] visited = new bool[w, h];

        Queue<Vector2Int> queue = new();

        Vector2Int start = new(-1, -1);

        for (int x = 0; x < w && start.x == -1; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (mountain[x, y])
                    continue;

                if (tempMountain.Contains(new Vector2Int(x, y)))
                    continue;

                start = new Vector2Int(x, y);
                break;
            }
        }

        if (start.x == -1)
            return true;

        queue.Enqueue(start);
        visited[start.x, start.y] = true;

        int reachable = 0;

        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            reachable++;

            foreach (var d in Utility.Neighbor8Directions)
            {
                int nx = c.x + d.x;
                int ny = c.y + d.y;

                if (!grid.IsInBounds(nx, ny))
                    continue;

                if (visited[nx, ny])
                    continue;

                if (mountain[nx, ny])
                    continue;

                if (tempMountain.Contains(new Vector2Int(nx, ny)))
                    continue;

                visited[nx, ny] = true;
                queue.Enqueue(new Vector2Int(nx, ny));
            }
        }

        int totalLand = totalCell - grid.GetMountainCellCount();

        int futureLand = totalLand;

        foreach (var c in newMountains)
            if (!mountain[c.x, c.y])
                futureLand--;

        return reachable == futureLand;
    }


    private void DrawMountain(
        Grid<GridCell> grid,
        Vector2Int start,
        System.Random mountainRandom)
    {
        //MountainData mountainData = new();

        Vector2Int current = start;

        int w = grid.GetWidth();
        int h = grid.GetHeight();

        float baseSize = Mathf.Sqrt(w * w + h * h);

        float minLength = config.minMountainLengthPercent * baseSize;
        float maxLength = config.maxMountainLengthPercent * baseSize;

        int length = Mathf.FloorToInt(
            Mathf.Lerp(minLength, maxLength, (float)mountainRandom.NextDouble()));

        //mountainData.targetLength = length;

        Vector2Int direction = Utility.GetRandom8Direction(mountainRandom);
        //Debug.Log($"Mountain chain length: {length}");
        for (int i = 0; i < length; i++)
        {
            if (!CheckIsValidPosForMountain(grid, current))
                break;

            float minRadius = config.minMountainRadiusPercent * baseSize;
            float maxRadius = config.maxMountainRadiusPercent * baseSize;

            int radius = Mathf.FloorToInt(
                Mathf.Lerp(minRadius, maxRadius, (float)mountainRandom.NextDouble()));

            List<Vector2Int> cells =
                GetMountainStampCells(grid, current, radius);

            if (!CanPlaceMountain(grid, cells))
                break;

            MountainSegment segment = new();

            StampMountain(grid, cells, segment);

            //mountainData.segments.Add(segment);

            direction = RandomizeDirection(direction, mountainRandom);
            current += direction;
        }

        //return mountainData;
    }

    private Vector2Int RandomizeDirection(
        Vector2Int currentDir,
        System.Random random)
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

    private List<Vector2Int> GetMountainStampCells(
    Grid<GridCell> grid,
    Vector2Int center,
    int radius)
    {
        List<Vector2Int> cells = new();

        int w = grid.GetWidth();
        int h = grid.GetHeight();

        int r2 = radius * radius;

        for (int dx = -radius; dx <= radius; dx++)
        {
            int dx2 = dx * dx;

            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx2 + dy * dy > r2)
                    continue;

                int nx = center.x + dx;
                int ny = center.y + dy;

                if (!grid.IsInBounds(nx, ny))
                    continue;

                cells.Add(new Vector2Int(nx, ny));
            }
        }

        return cells;
    }

    private void StampMountain(
    Grid<GridCell> grid,
    List<Vector2Int> cells,
    MountainSegment segment)
    {
        bool[,] mountain = grid.GetMountainGrid();

        foreach (var c in cells)
        {
            if (mountain[c.x, c.y])
                continue;

            mountain[c.x, c.y] = true;

            segment.stampedCells.Add(c);

            grid.IncrementMountainCellCount();
        }
    }

    private bool CheckIsValidPosForMountain(Grid<GridCell> grid, Vector2Int pos)
    {
        return grid.IsInBounds(pos.x, pos.y) && !grid.GetWaterGrid()[pos.x, pos.y];
    }
}