using System.Collections.Generic;
using UnityEngine;

public class EnvironmentRegionAnalysisStep : IMapGeneratorStep
{
    private Grid<GridCell> _grid;

    private float maxHelpExpand = 0;
    //private int maxWaterLength = 0;

    public void Execute(Grid<GridCell> grid)
    {
        _grid = grid;

        bool[,] visitedWaterCells = new bool[_grid.GetWidth(), _grid.GetHeight()];
        bool[,] visitedUrbanCells = new bool[_grid.GetWidth(), _grid.GetHeight()];
        int regionId = 0;

        for (int x = 0; x < _grid.GetWidth(); x++)
        {
            for (int y = 0; y < _grid.GetHeight(); y++)
            {
                GridCell start = _grid.GetCell(x, y);

                EnvironmentType environmentType = start.Stats.environment.currentEnvironmentType;
                PopulationType populationType = start.Stats.population.type;

                bool isWater = environmentType == EnvironmentType.Water;
                if (!visitedWaterCells[x, y] && isWater && start.Region == null)
                    BuildWaterRegion(start, visitedWaterCells, regionId++);

                bool isUrban = populationType == PopulationType.High;
                if (!visitedUrbanCells[x, y] && isUrban && start.Urban == null)
                    BuildUrbanRegion(start, visitedUrbanCells, regionId++);
            }
        }

        Analyze();
    }

    public void Analyze()
    {
        foreach (UrbanRegion urban in _grid.urbanRegions)
            CalculateUrbanStats(urban);

        BuildDistanceToNearestUrbanMap();

        BuildDistanceToNearestWaterRegionMap();

        AnalyzeWater(_grid.waterRegions);

        NormalizeScores(_grid.waterRegions);
    }

    private void BuildDistanceToNearestUrbanMap()
    {
        int width = _grid.GetWidth();
        int height = _grid.GetHeight();

        _grid.distanceToNearestUrbanMap = new int[width, height];
        _grid.nearestUrbanMap = new UrbanRegion[width, height];

        bool[,] visited = new bool[width, height];
        Queue<GridCell> queue = new();

        // Initialize
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _grid.distanceToNearestUrbanMap[x, y] = int.MaxValue;
            }
        }

        // Multi-source
        foreach (UrbanRegion urban in _grid.urbanRegions)
        {
            foreach (GridCell source in urban.Cells)
            {
                visited[source.X, source.Y] = true;

                _grid.distanceToNearestUrbanMap[source.X, source.Y] = 0;
                _grid.nearestUrbanMap[source.X, source.Y] = urban;

                if (urban.BoundaryCells.Contains(source))
                    queue.Enqueue(source);
            }
        }

        bool[,] mountains = _grid.GetMountainGrid();

        while (queue.Count > 0)
        {
            GridCell current = queue.Dequeue();

            foreach (Vector2Int dir in Utility.Neighbor8Directions)
            {
                int nx = current.X + dir.x;
                int ny = current.Y + dir.y;

                if (!_grid.IsInBounds(nx, ny))
                    continue;

                GridCell neighbor = _grid.GetCell(nx, ny);

                if (neighbor.Stats.population.type == PopulationType.High)
                    continue;

                if (visited[nx, ny])
                    continue;

                if (mountains[nx, ny])
                    continue;

                visited[nx, ny] = true;

                _grid.distanceToNearestUrbanMap[nx, ny] = _grid.distanceToNearestUrbanMap[current.X, current.Y] + 1;

                _grid.nearestUrbanMap[nx, ny] = _grid.nearestUrbanMap[current.X, current.Y];

                queue.Enqueue(_grid.GetCell(nx, ny));
            }
        }
    }

    private void BuildDistanceToNearestWaterRegionMap()
    {
        int width = _grid.GetWidth();
        int height = _grid.GetHeight();

        _grid.distanceToNearestWaterRegionMap = new int[width, height];
        _grid.nearestWaterRegionMap = new EnvironmentRegion[width, height];

        bool[,] visited = new bool[width, height];
        Queue<GridCell> queue = new();

        // Initialize
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _grid.distanceToNearestWaterRegionMap[x, y] = int.MaxValue;
            }
        }

        // Multi-source
        foreach (EnvironmentRegion waterRegion in _grid.waterRegions)
        {
            foreach (GridCell source in waterRegion.Cells)
            {
                visited[source.X, source.Y] = true;

                _grid.distanceToNearestWaterRegionMap[source.X, source.Y] = 0;
                _grid.nearestWaterRegionMap[source.X, source.Y] = waterRegion;

                if (waterRegion.BoundaryCells.Contains(source))
                    queue.Enqueue(source);
            }
        }

        bool[,] mountains = _grid.GetMountainGrid();

        while (queue.Count > 0)
        {
            GridCell current = queue.Dequeue();

            foreach (Vector2Int dir in Utility.Neighbor8Directions)
            {
                int nx = current.X + dir.x;
                int ny = current.Y + dir.y;

                if (!_grid.IsInBounds(nx, ny))
                    continue;

                GridCell neighbor = _grid.GetCell(nx, ny);

                if (neighbor.Stats.environment.currentEnvironmentType == EnvironmentType.Water)
                    continue;

                if (visited[nx, ny])
                    continue;

                if (mountains[nx, ny])
                    continue;

                visited[nx, ny] = true;

                _grid.distanceToNearestWaterRegionMap[nx, ny] = _grid.distanceToNearestWaterRegionMap[current.X, current.Y] + 1;

                _grid.nearestWaterRegionMap[nx, ny] = _grid.nearestWaterRegionMap[current.X, current.Y];

                queue.Enqueue(_grid.GetCell(nx, ny));
            }
        }
    }

    private void BuildWaterRegion(GridCell start, bool[,] visited, int id)
    {
        EnvironmentRegion region = new()
        {
            Id = id,
            Type = EnvironmentType.Water
        };

        Queue<GridCell> queue = new();
        queue.Enqueue(start);
        visited[start.X, start.Y] = true;

        while (queue.Count > 0)
        {
            GridCell cell = queue.Dequeue();

            region.Cells.Add(cell);
            cell.Region = region;

            foreach (Vector2Int dir in Utility.Neighbor8Directions)
            {
                int nx = cell.X + dir.x;
                int ny = cell.Y + dir.y;

                if (!_grid.IsInBounds(nx, ny))
                    continue;

                if (visited[nx, ny])
                    continue;

                GridCell neighbor = _grid.GetCell(nx, ny);

                if (neighbor.Stats.environment.currentEnvironmentType != EnvironmentType.Water)
                    continue;

                visited[nx, ny] = true;
                queue.Enqueue(neighbor);
            }
        }

        region.CellCount = region.Cells.Count;

        BuildBoundary(region);
        _grid.waterRegions.Add(region);
    }

    private void BuildUrbanRegion(GridCell start, bool[,] visited, int id)
    {
        UrbanRegion urban = new()
        {
            Id = id,
        };

        Queue<GridCell> queue = new();
        queue.Enqueue(start);
        visited[start.X, start.Y] = true;

        while (queue.Count > 0)
        {
            GridCell cell = queue.Dequeue();

            urban.Cells.Add(cell);
            cell.Urban = urban;

            foreach (Vector2Int dir in Utility.Neighbor8Directions)
            {
                int nx = cell.X + dir.x;
                int ny = cell.Y + dir.y;

                if (!_grid.IsInBounds(nx, ny))
                    continue;

                if (visited[nx, ny])
                    continue;

                GridCell neighbor = _grid.GetCell(nx, ny);

                if (neighbor.Stats.population.type != PopulationType.High)
                    continue;

                visited[nx, ny] = true;
                queue.Enqueue(neighbor);
            }
        }

        urban.CellCount = urban.Cells.Count;

        BuildBoundary(urban);
        CalculateCenterCell(urban);
        _grid.urbanRegions.Add(urban);
    }

    private void BuildBoundary(EnvironmentRegion region)
    {
        foreach (GridCell cell in region.Cells)
        {
            bool isBoundaryCell = false;

            foreach (Vector2Int dir in Utility.Neighbor8Directions)
            {
                int nx = cell.X + dir.x;
                int ny = cell.Y + dir.y;

                if (!_grid.IsInBounds(nx, ny))
                {
                    isBoundaryCell = true;
                    break;
                }

                GridCell neighbor = _grid.GetCell(nx, ny);

                if (neighbor.Region != region)
                {
                    isBoundaryCell = true;
                    break;
                }
            }

            if (isBoundaryCell)
                region.BoundaryCells.Add(cell);
        }
    }

    private void BuildBoundary(UrbanRegion urban)
    {
        foreach (GridCell cell in urban.Cells)
        {
            bool isBoundaryCell = false;

            foreach (Vector2Int dir in Utility.Neighbor8Directions)
            {
                int nx = cell.X + dir.x;
                int ny = cell.Y + dir.y;

                if (!_grid.IsInBounds(nx, ny))
                {
                    isBoundaryCell = true;
                    break;
                }

                GridCell neighbor = _grid.GetCell(nx, ny);

                if (neighbor.Urban != urban)
                {
                    isBoundaryCell = true;
                    break;
                }
            }

            if (isBoundaryCell)
                urban.BoundaryCells.Add(cell);
        }
    }

    private void CalculateCenterCell(UrbanRegion urban)
    {
        GridCell bestCell = null;
        int bestCost = int.MaxValue;

        foreach (GridCell candidate in urban.Cells)
        {
            int cost = 0;

            foreach (GridCell other in urban.Cells)
            {
                cost += Mathf.Max(Mathf.Abs(candidate.X - other.X), Mathf.Abs(candidate.Y - other.Y));
            }

            if (cost < bestCost)
            {
                bestCost = cost;
                bestCell = candidate;
            }
        }

        urban.CenterCell = bestCell;
    }

    private void CalculateUrbanStats(UrbanRegion urban)
    {
        foreach (GridCell cell in urban.Cells)
        {
            urban.PopulationWeight += cell.Stats.population.weight;
            urban.StructureWeight += cell.Stats.structure.currentPriorityToHuman;
        }

        urban.TotalWeight = urban.PopulationWeight + urban.StructureWeight;
    }

    private void AnalyzeWater(List<EnvironmentRegion> regions)
    {
        foreach (EnvironmentRegion region in regions)
        {
            region.CellCount = region.Cells.Count;

            //region.Length = CalculateLongestPath(region);
            //maxWaterLength = (int)Mathf.Max(maxWaterLength, region.Length);

            region.HelpExpandPower = CalculateHelpExpandPower(region);
            maxHelpExpand = Mathf.Max(maxHelpExpand, region.HelpExpandPower);
        }
    }

    public void NormalizeScores(List<EnvironmentRegion> waterRegions)
    {
        foreach (EnvironmentRegion region in waterRegions)
        {
            float areaScore = Mathf.Sqrt(region.CellCount) / Mathf.Sqrt(_grid.GetWaterCellCount());
            region.HelpExpandPower /= Mathf.Max(maxHelpExpand, 1);

            region.AssistScore =
                areaScore * 0.3f +
                region.HelpExpandPower * 0.7f;
        }
    }

    private float CalculateHelpExpandPower(EnvironmentRegion region)
    {
        float score = 0;

        foreach (GridCell waterCell in region.BoundaryCells)
        {
            int distance = _grid.distanceToNearestUrbanMap[waterCell.X, waterCell.Y];
            if (distance == int.MaxValue)
                continue;

            UrbanRegion urban = _grid.nearestUrbanMap[waterCell.X, waterCell.Y];

            score += urban.TotalWeight / ((distance + 1f) * (distance + 1f));
            score /= region.BoundaryCells.Count;
        }

        return score;
    }
}
