using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MountainGenerator : IMapGeneratorStep
{
    private readonly MapConfig config;
    private readonly int randomSeed;

    private readonly float targetRatio;
    private int totalCell;

    const int MaxStartAttempts = 100;

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
        float baseSize = Mathf.Sqrt(w * w + h * h);

        System.Random mountainRandom = new(randomSeed);

        int mountainCount = mountainRandom.Next(
            config.minMountainChains,
            config.MaxMountainChains + 1);

        if (mountainCount <= 0) return;

        for (int i = 0; i < mountainCount; i++)
        {
            for (int attempt = 0; attempt < MaxStartAttempts; attempt++)
            {
                Vector2Int start = Utility.GetRandomPosition(grid, mountainRandom);

                if (!CheckIsValidPosForMountain(grid, start))
                    continue;

                int minRadius = Mathf.RoundToInt(config.minMountainRadiusPercent * baseSize);

                List<Vector2Int> cells = GetStampCells(grid, start, minRadius);

                if (!CanPlaceMountain(grid, cells))
                    continue;

                if (DrawMountain(grid, start, baseSize, mountainRandom))
                    break;
            }
        }

        if (grid.GetMountainCellCount() > 0)
            SmoothingMountainsEdge(grid, mountainRandom);
    }

    private bool CanPlaceMountain(Grid<GridCell> grid, List<Vector2Int> cells)
    {
        bool[,] mountain = grid.GetMountainGrid();
        int incrementalMountainCells = 0;

        #region Check mountain ratio

        foreach (var c in cells)
            if (!mountain[c.x, c.y] && !grid.GetWaterGrid()[c.x, c.y])
                incrementalMountainCells++;

        if (incrementalMountainCells <= 0) return false;

        float tempRatio = ((float)grid.GetMountainCellCount() + incrementalMountainCells) / totalCell;

        if (tempRatio > targetRatio)
            return false;

        #endregion

        #region Flood fill connectivity check

        return CheckMapConnectivityAfterCreateASegment(grid, cells);

        #endregion
    }

    private bool CheckMapConnectivityAfterCreateASegment(Grid<GridCell> grid, List<Vector2Int> newMountains)
    {
        int w = grid.GetWidth();
        int h = grid.GetHeight();

        bool[,] mountain = grid.GetMountainGrid();

        bool[,] tempBlocked = new bool[w, h];
        foreach (Vector2Int newMountain in newMountains)
            tempBlocked[newMountain.x, newMountain.y] = true;

        bool[,] visited = new bool[w, h];

        Queue<Vector2Int> queue = new();

        Vector2Int start = new(-1, -1);

        for (int x = 0; x < w && start.x == -1; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (mountain[x, y])
                    continue;

                if (tempBlocked[x, y])
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

                if (tempBlocked[nx, ny])
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

    private bool DrawMountain(Grid<GridCell> grid, Vector2Int start, float baseSize, System.Random mountainRandom)
    {
        float minLength = config.minMountainLengthPercent * baseSize;
        float maxLength = config.maxMountainLengthPercent * baseSize;

        int length = Mathf.FloorToInt(Mathf.Lerp(minLength, maxLength, (float)mountainRandom.NextDouble()));

        // First segment
        int previousRadius = GetRandomRadius(baseSize, mountainRandom);

        List<Vector2Int> startCells = GetStampCells(grid, start, previousRadius);

        if (!CanPlaceMountain(grid, startCells))
            return false;

        MountainSegment firstSegment = new();
        StampMountain(grid, startCells, firstSegment);

        length--;
        int actualLength = 1;

        Vector2Int current = start;
        Vector2Int preferredDirection = Utility.GetRandom8Direction(mountainRandom);

        for (int i = 0; i < length; i++)
        {
            int radius = GetRandomRadius(baseSize, mountainRandom);

            if (i > 0) preferredDirection = RandomizeDirection(preferredDirection, mountainRandom);

            List<Vector2Int> directions = new() { preferredDirection };

            foreach (Vector2Int dir in Utility.NeighborCardinalDirections)
            {
                if (dir != preferredDirection)
                    directions.Add(dir);
            }

            // Shuffle secondary directions, keep preferred direction first
            for (int j = 1; j < directions.Count; j++)
            {
                int swapIndex = mountainRandom.Next(j, directions.Count);

                (directions[j], directions[swapIndex]) = (directions[swapIndex], directions[j]);
            }

            bool foundValid = false;
            List<Vector2Int> validCells = null;

            foreach (Vector2Int dir in directions)
            {
                int distance = previousRadius + radius - 1;

                Vector2Int candidatePos = current + dir * distance;

                if (!CheckIsValidPosForMountain(grid, candidatePos))
                    continue;

                List<Vector2Int> cells = GetStampCells(grid, candidatePos, radius);

                if (!CanPlaceMountain(grid, cells))
                    continue;

                current = candidatePos;
                preferredDirection = dir;
                previousRadius = radius;

                validCells = cells;
                foundValid = true;

                break;
            }

            if (!foundValid)
                break;

            MountainSegment segment = new();
            StampMountain(grid, validCells, segment);
            actualLength++;
        }

        return actualLength > 0;
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

    private int GetRandomRadius(float baseSize, System.Random mountainRandom)
    {
        float minRadius = config.minMountainRadiusPercent * baseSize;
        float maxRadius = config.maxMountainRadiusPercent * baseSize;
        return Mathf.FloorToInt(Mathf.Lerp(minRadius, maxRadius, (float)mountainRandom.NextDouble()));
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

    private void StampMountain(Grid<GridCell> grid, List<Vector2Int> cells, MountainSegment segment)
    {
        bool[,] mountain = grid.GetMountainGrid();

        foreach (var c in cells)
        {
            if (mountain[c.x, c.y])
                continue;

            grid.SetMoutainCell(c.x, c.y, true);

            segment.stampedCells.Add(c);

            grid.IncrementMountainCellCount();
        }
    }

    private bool CheckIsValidPosForMountain(Grid<GridCell> grid, Vector2Int pos)
    {
        return grid.IsInBounds(pos.x, pos.y)
            && !grid.GetWaterGrid()[pos.x, pos.y];
    }

    private void SmoothingMountainsEdge(Grid<GridCell> grid, System.Random random)
    {
        List<Vector2Int> borderCells = GetBorderCells(grid);

        Utility.Shuffle(borderCells, random);

        bool[,] mountain = grid.GetMountainGrid();

        foreach (Vector2Int cell in borderCells)
        {
            if (random.NextDouble() > config.removeEdgesChance)
                continue;

            if (!CanRemoveMountainCell(grid, cell))
                continue;

            mountain[cell.x, cell.y] = false;
            grid.DecrementMountainCellCount();
        }
    }

    private List<Vector2Int> GetBorderCells(Grid<GridCell> grid)
    {
        List<Vector2Int> result = new();

        bool[,] mountain = grid.GetMountainGrid();

        int w = grid.GetWidth();
        int h = grid.GetHeight();

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                if (!mountain[i, j])
                    continue;

                bool isBorder = false;

                foreach (Vector2Int dir in Utility.Neighbor8Directions)
                {
                    int nx = i + dir.x;
                    int ny = j + dir.y;

                    if (!grid.IsInBounds(nx, ny))
                        break;

                    if (Utility.NeighborCardinalDirections.Contains(dir))
                        if (!mountain[nx, ny])
                        {
                            isBorder = true;
                            break;
                        }
                }

                if (isBorder)
                    result.Add(new Vector2Int(i, j));
            }
        }

        return result;
    }

    private bool CanRemoveMountainCell(Grid<GridCell> grid, Vector2Int cell)
    {
        bool[,] mountain = grid.GetMountainGrid();

        // ===== Check mountain connectivity =====

        List<Vector2Int> mountainNeighbors = new();

        foreach (Vector2Int dir in Utility.NeighborCardinalDirections)
        {
            Vector2Int n = cell + dir;

            if (!grid.IsInBounds(n.x, n.y))
                continue;

            if (mountain[n.x, n.y])
                mountainNeighbors.Add(n);
        }

        if (mountainNeighbors.Count > 1)
        {
            bool[,] visited = new bool[grid.GetWidth(), grid.GetHeight()];
            Queue<Vector2Int> queue = new();

            queue.Enqueue(mountainNeighbors[0]);
            visited[mountainNeighbors[0].x, mountainNeighbors[0].y] = true;

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

                foreach (Vector2Int dir in Utility.NeighborCardinalDirections)
                {
                    Vector2Int next = current + dir;

                    if (next == cell)
                        continue;

                    if (!grid.IsInBounds(next.x, next.y))
                        continue;

                    if (!mountain[next.x, next.y])
                        continue;

                    if (visited[next.x, next.y])
                        continue;

                    visited[next.x, next.y] = true;
                    queue.Enqueue(next);
                }
            }

            foreach (Vector2Int n in mountainNeighbors)
            {
                if (!visited[n.x, n.y])
                    return false;
            }
        }

        // ===== Check land connectivity around removed cell =====

        List<Vector2Int> landNeighbors = new();

        foreach (Vector2Int dir in Utility.NeighborCardinalDirections)
        {
            Vector2Int n = cell + dir;

            if (!grid.IsInBounds(n.x, n.y))
                continue;

            if (!mountain[n.x, n.y])
                landNeighbors.Add(n);
        }

        if (landNeighbors.Count == 0)
            return false;

        if (landNeighbors.Count > 4) // has at least 1 cardinal neighbor
            return true;

        bool[,] landVisited = new bool[grid.GetWidth(), grid.GetHeight()];
        Queue<Vector2Int> landQueue = new();

        landQueue.Enqueue(landNeighbors[0]);
        landVisited[landNeighbors[0].x, landNeighbors[0].y] = true;

        while (landQueue.Count > 0)
        {
            Vector2Int current = landQueue.Dequeue();

            foreach (Vector2Int dir in Utility.NeighborCardinalDirections)
            {
                Vector2Int next = current + dir;

                if (!grid.IsInBounds(next.x, next.y))
                    continue;

                bool isLand =
                    next == cell ||
                    !mountain[next.x, next.y];

                if (!isLand)
                    continue;

                if (landVisited[next.x, next.y])
                    continue;

                landVisited[next.x, next.y] = true;
                landQueue.Enqueue(next);
            }
        }

        foreach (Vector2Int n in landNeighbors)
        {
            if (!landVisited[n.x, n.y])
                return false;
        }

        return true;
    }
}