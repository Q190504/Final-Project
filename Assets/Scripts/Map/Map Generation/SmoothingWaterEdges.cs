using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SmoothingWaterEdges : IMapGeneratorStep
{
    private readonly MapConfig config;
    private readonly int randomSeed;

    public SmoothingWaterEdges(MapConfig config, int seed)
    {
        this.config = config;
        this.randomSeed = seed;
    }

    public void Execute(Grid<GridCell> grid)
    {
        if (grid.GetWaterCellCount() <= 0) return;

        System.Random random = new(randomSeed);
        SmoothingEdges(grid, random);
    }

    private void SmoothingEdges(Grid<GridCell> grid, System.Random random)
    {
        List<Vector2Int> borderCells = GetBorderCells(grid);

        Utility.Shuffle(borderCells, random);

        bool[,] water = grid.GetWaterGrid();

        foreach (Vector2Int cell in borderCells)
        {
            if (random.NextDouble() > config.removeEdgesChance)
                continue;

            if (!CanRemoveWaterCell(grid, cell))
                continue;

            water[cell.x, cell.y] = false;
            grid.DecrementWaterCellCount();
        }
    }

    private List<Vector2Int> GetBorderCells(Grid<GridCell> grid)
    {
        List<Vector2Int> result = new();

        bool[,] water = grid.GetWaterGrid();

        int w = grid.GetWidth();
        int h = grid.GetHeight();

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                if (!water[i, j])
                    continue;

                bool isBorder = false;

                foreach (Vector2Int dir in Utility.Neighbor8Directions)
                {
                    int nx = i + dir.x;
                    int ny = j + dir.y;

                    if (!grid.IsInBounds(nx, ny))
                        break;

                    if (Utility.NeighborCardinalDirections.Contains(dir))
                        if (!water[nx, ny])
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

    private bool CanRemoveWaterCell(Grid<GridCell> grid, Vector2Int cell)
    {
        bool[,] water = grid.GetWaterGrid();

        List<Vector2Int> neighbors = new();

        foreach (Vector2Int dir in Utility.NeighborCardinalDirections)
        {
            Vector2Int n = cell + dir;

            if (!grid.IsInBounds(n.x, n.y))
                continue;

            if (water[n.x, n.y])
                neighbors.Add(n);
        }

        if (neighbors.Count <= 1)
            return true;

        int connectedNeighbors = 1;

        bool[,] visited = new bool[grid.GetWidth(), grid.GetHeight()];
        Queue<Vector2Int> queue = new();

        queue.Enqueue(neighbors[0]);
        visited[neighbors[0].x, neighbors[0].y] = true;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (neighbors.Contains(current))
            {
                connectedNeighbors++;

                if (connectedNeighbors == neighbors.Count)
                    return true;
            }

            foreach (Vector2Int dir in Utility.NeighborCardinalDirections)
            {
                Vector2Int next = current + dir;

                if (next == cell)
                    continue;

                if (!grid.IsInBounds(next.x, next.y))
                    continue;

                if (!water[next.x, next.y])
                    continue;

                if (visited[next.x, next.y])
                    continue;

                visited[next.x, next.y] = true;
                queue.Enqueue(next);
            }
        }

        return false;
    }
}
