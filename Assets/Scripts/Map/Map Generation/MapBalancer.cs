using System.Collections.Generic;
using UnityEngine;

public class MapBalancer : IMapGeneratorStep
{
    private readonly MapConfig config;
    private readonly int randomSeed;

    public MapBalancer(MapConfig config, int seed)
    {
        this.config = config;
        this.randomSeed = seed;
    }

    public void Execute(Grid<GridCell> grid)
    {
        System.Random random = new(randomSeed);

        int w = grid.GetWidth();
        int h = grid.GetHeight();
        int totalCell = w * h;

        BalanceUrbanRatio(grid, random, totalCell);
        BalanceWaterRatio(grid, totalCell);
    }

    private void BalanceUrbanRatio(Grid<GridCell> grid, System.Random random, int totalCell)
    {
        int w = grid.GetWidth();
        int h = grid.GetHeight();

        float[,] popGrid = grid.GetPopulationGrid();

        PopulationData highPopulationData = PropertyDataManager.Instance.GetPopulationData(PopulationType.High);

        PopulationData mediumPopulationData = PropertyDataManager.Instance.GetPopulationData(PopulationType.Medium);

        float minUrbanValue = highPopulationData.minPopulationValue;

        MinHeap<Vector3Int> urbanHeap = new();
        int urbanCount = 0;

        // Build urban heap once
        #region Build urban list

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (popGrid[x, y] >= minUrbanValue)
                {
                    int neighbors = CountUrbanNeighbors(x, y, grid, minUrbanValue);

                    urbanHeap.Push(new Vector3Int(x, y, neighbors), neighbors);

                    urbanCount++;
                }
            }
        }

        #endregion

        float ratio = (float)urbanCount / totalCell;

        //Debug.Log($"Initial Urban Ratio: {ratio * 100}%");

        while (ratio < config.urbanRatioRange.x || ratio > config.urbanRatioRange.y)
        {
            if (ratio < config.urbanRatioRange.x)
            {
                urbanCount += Utility.CreateUrban(grid, w, h, config, random, highPopulationData);
            }
            else
            {
                if (urbanHeap.Count == 0)
                {
                    //Debug.LogWarning("No urban cells available.");
                    break;
                }

                Vector3Int cell = urbanHeap.Pop();
                DemoteACellToLand(cell, grid, random, mediumPopulationData);
                urbanCount--;
            }

            ratio = (float)urbanCount / totalCell;
        }

        //grid.NormalizePopulationMap();

        //Debug.Log($"Final Urban Ratio: {ratio * 100}%");
    }

    private void BalanceWaterRatio(Grid<GridCell> grid, int totalCell)
    {
        float targetRatio = config.maxWaterRatio;

        MinHeap<Vector3Int> lakeCenterCellsHeap = grid.GetLakeCenterCellsHeap();
        MinHeap<RiverData> riverDataHeap = grid.GetRiverDataHeap();

        float currentRatio = (float)grid.GetWaterCellCount() / totalCell;

        while (currentRatio > targetRatio)
        {
            if (lakeCenterCellsHeap.Count > 0)
            {
                var lake = lakeCenterCellsHeap.Pop();

                Utility.FillCircleWithWater(grid, lake.x, lake.y, lake.z, false);
            }
            else
            {
                if (riverDataHeap.Count == 0)
                    return;

                RiverData river = riverDataHeap.Pop();

                for (int i = river.segments.Count - 1; i >= 0; i--)
                {
                    RiverSegment seg = river.segments[i];

                    foreach (var cell in seg.stampedCells)
                    {
                        grid.SetWaterCell(cell.x, cell.y, false);
                        grid.DecrementWaterCellCount();
                    }

                    river.segments.RemoveAt(i);

                    currentRatio = (float)grid.GetWaterCellCount() / totalCell;

                    if (currentRatio <= targetRatio)
                        return;
                }
            }

            currentRatio = (float)grid.GetWaterCellCount() / totalCell;
        }
    }

    private int CountUrbanNeighbors(int x, int y, Grid<GridCell> grid, float minUrbanValue)
    {
        int count = 0;

        foreach (Vector2Int n in Utility.Neighbor8Directions)
        {
            int nx = x + n.x;
            int ny = y + n.y;

            if (grid.IsInBounds(nx, ny) &&
                grid.GetPopulationGrid()[nx, ny] >= minUrbanValue)
            {
                count++;
            }
        }

        return count;
    }

    private void DemoteACellToLand(Vector3Int cell, Grid<GridCell> grid,
        System.Random random, PopulationData mediumPopulationData)
    {
        float mediumPopulationValue =
            Mathf.Lerp(
                mediumPopulationData.minPopulationValue,
                mediumPopulationData.maxPopulationValue,
                (float)random.NextDouble());

        grid.GetPopulationGrid()[cell.x, cell.y] = mediumPopulationValue;
    }
}