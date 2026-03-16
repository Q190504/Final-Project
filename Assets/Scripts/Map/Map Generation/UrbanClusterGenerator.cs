using UnityEngine;
using System.Collections.Generic;

public class UrbanClusterGenerator : IMapGeneratorStep
{
    public void Execute(Grid<GridCell> grid)
    {
        bool[,] visited =
            new bool[grid.GetWidth(), grid.GetHeight()];

        grid.urbanClusters = new List<UrbanCluster>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                if (visited[x, y])
                    continue;

                if (!Utility.IsUrban(grid.GetCell(x, y).Stats.environment.currentEnvironmentType))
                    continue;

                var cluster = FloodFill(grid, x, y, visited);

                grid.urbanClusters.Add(cluster);
            }
        }
    }

    private UrbanCluster FloodFill(
    Grid<GridCell> grid,
    int startX,
    int startY,
    bool[,] visited)
    {
        Queue<Vector2Int> queue = new();
        List<Vector2Int> clusterCells = new();

        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            clusterCells.Add(current);

            foreach (Vector2Int neighbor in Utility.Neighbor8Directions)
            {
                int nx = current.x + neighbor.x;
                int ny = current.y + neighbor.y;

                if (!grid.IsInBounds(nx, ny))
                    continue;

                if (visited[nx, ny])
                    continue;

                if (!Utility.IsUrban(grid.GetCell(nx, ny).Stats.environment.currentEnvironmentType))
                    continue;

                visited[nx, ny] = true;
                queue.Enqueue(neighbor);
            }
        }

        return new UrbanCluster(clusterCells);
    }
}
