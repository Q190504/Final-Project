using System;
using System.Collections.Generic;
using UnityEngine;

public class Grid<TGridObject>
{
    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;

    private GridCell[,] gridArray;
    private float[,] temperatureGridArray;
    private float[,] populationGridArray;
    private bool[,] waterGridArray;
    private bool[,] mountainGridArray;

    private List<RiverData> riverDatas = new();

    public List<EnvironmentRegion> waterRegions = new();
    //public List<EnvironmentRegion> mountainRegions = new();
    public List<UrbanRegion> urbanRegions = new();

    public int[,] distanceToNearestUrbanMap;
    public UrbanRegion[,] nearestUrbanMap;

    public int[,] distanceToNearestWaterRegionMap;
    public EnvironmentRegion[,] nearestWaterRegionMap;

    private int waterCellCount = 0;
    private int mountainCellCount = 0;

    public Grid(int width, int height, float cellSize, Vector3 originPosition,
    Func<Grid<TGridObject>, int, int, GridCell> createCell)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new GridCell[width, height];
        temperatureGridArray = new float[width, height];
        populationGridArray = new float[width, height];
        waterGridArray = new bool[width, height];
        mountainGridArray = new bool[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createCell(this, x, y);
            }
        }
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt(((worldPosition.x - originPosition.x) / cellSize) + cellSize / 2);
        y = Mathf.FloorToInt(((worldPosition.y - originPosition.y) / cellSize) + cellSize / 2);
    }

    public GridCell GetCell(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
            return gridArray[x, y];
        else return null;
    }

    public GridCell GetCell(Vector3 worldPosition)
    {
        GetXY(worldPosition, out int x, out int y);
        if (x >= 0 && y >= 0 && x < width && y < height)
            return gridArray[x, y];
        else return null;
    }

    public void SetTemperatureCell(int x, int y, float value)
    {
        if (IsInBounds(x, y))
            temperatureGridArray[x, y] = value;
    }

    public void SetPopulationCell(int x, int y, float value)
    {
        value = Mathf.Clamp01(value);

        if (IsInBounds(x, y))
            populationGridArray[x, y] = value;
    }

    public void SetMoutainCell(int x, int y, bool value)
    {
        if (IsInBounds(x, y))
            mountainGridArray[x, y] = value;
    }

    public void NormalizePopulationMap()
    {
        float max = 0f;

        // Find maximum value in grid
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (populationGridArray[x, y] > max)
                    max = populationGridArray[x, y];
            }
        }

        // Avoid division by zero
        if (max <= 0f) return;

        // Normalize and clamp
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float normalized = populationGridArray[x, y] / max;
                populationGridArray[x, y] = Mathf.Clamp(normalized, 0f, 1f);
            }
        }
    }

    public void SetWaterCell(int x, int y, bool value)
    {
        if (IsInBounds(x, y))
            waterGridArray[x, y] = value;
    }

    public GridCell[,] GetGrid()
    {
        return gridArray;
    }

    public List<GridCell> GetInfectedSnapshot()
    {
        List<GridCell> snapshot = new();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridCell cell = gridArray[x, y];

                if (cell.IsInfectious() || cell.Stats.isContagious)
                    snapshot.Add(gridArray[x, y]);
            }
        }

        return snapshot;
    }

    public float[,] GetPopulationGrid()
    {
        return populationGridArray;
    }

    public float[,] GetTemperatureGrid()
    {
        return temperatureGridArray;
    }

    public bool[,] GetMountainGrid()
    {
        return mountainGridArray;
    }

    public bool[,] GetWaterGrid()
    {
        return waterGridArray;
    }

    public int GetWidth() { return width; }
    public int GetHeight() { return height; }
    public float GetCellSize() { return cellSize; }
    public Vector3 GetOriginPosition() { return originPosition; }

    public bool IsInBounds(int x, int y)
    {
        return (x >= 0 && y >= 0 && x < width && y < height);
    }

    public bool IsMapEdge(int x, int y)
    {
        return x == 0 || y == 0 || x == width - 1 || y == height - 1;
    }

    public void IncrementWaterCellCount()
    {
        waterCellCount++;
    }

    public void DecrementWaterCellCount()
    {
        waterCellCount--;
    }

    public void ResetWaterCellCount()
    {
        waterCellCount = 0;
    }

    public int GetWaterCellCount()
    {
        return waterCellCount;
    }

    #region River   

    public void AddRiverData(RiverData riverData)
    {
        riverDatas.Add(riverData);
    }

    public List<RiverData> GetRiverDatas()
    {
        return riverDatas;
    }

    public void ClearRiverData()
    {
        riverDatas.Clear();
    }

    #endregion

    #region Mountain

    public void IncrementMountainCellCount()
    {
        mountainCellCount++;
    }

    public void DecrementMountainCellCount()
    {
        mountainCellCount--;
    }

    public void ResetMountainCellCount()
    {
        mountainCellCount = 0;
    }

    public int GetMountainCellCount()
    {
        return mountainCellCount;
    }

    //public void AddMountainData(MountainData mountainData)
    //{
    //    mountainDatasList.Add(mountainData);
    //}

    //public List<MountainData> GetMountainDataList()
    //{
    //    return mountainDatasList;
    //}

    //public void ClearMountainData()
    //{
    //    mountainDatasList.Clear();
    //}

    #endregion

    public List<GridCell> GetNeighborsInRange(GridCell cell, int range, bool includeDiagonals = true)
    {
        List<GridCell> neighbors = new();

        for (int x = cell.X - range; x <= cell.X + range; x++)
        {
            for (int y = cell.Y - range; y <= cell.Y + range; y++)
            {
                if (!IsInBounds(x, y))
                    continue;

                // Skip self
                if (x == cell.X && y == cell.Y)
                    continue;

                // Skip diagonals if disabled
                if (!includeDiagonals
                    && x != cell.X
                    && y != cell.Y)
                {
                    continue;
                }

                neighbors.Add(gridArray[x, y]);
            }
        }

        return neighbors;
    }

    public List<GridCell> GetNeighbourInCircleWithRange(int cx, int cy, int radius)
    {
        List<GridCell> neighbours = new();

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                int nx = cx + dx;
                int ny = cy + dy;

                // Skip self
                if (nx == cx && ny == cy)
                    continue;

                if (IsInBounds(nx, ny) && (dx * dx + dy * dy <= radius * radius))
                {
                    GridCell cell = GetCell(nx, ny);
                    neighbours.Add(cell);
                }
            }
        }

        return neighbours;
    }

    public int GetShortestDistance(GridCell start, GridCell target, bool ignoreMountain)
    {
        if (start == null || target == null)
            return -1;

        if (start == target)
            return 0;

        bool[,] visited = new bool[width, height];
        int[,] distance = new int[width, height];

        Queue<GridCell> queue = new();

        visited[start.X, start.Y] = true;
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            GridCell current = queue.Dequeue();

            foreach (Vector2Int dir in Utility.Neighbor8Directions)
            {
                int nx = current.X + dir.x;
                int ny = current.Y + dir.y;

                if (!IsInBounds(nx, ny))
                    continue;

                if (visited[nx, ny])
                    continue;

                GridCell next = GetCell(nx, ny);

                if (!ignoreMountain &&
                    next.Stats.environment.currentEnvironmentType == EnvironmentType.Mountain)
                    continue;

                visited[nx, ny] = true;
                distance[nx, ny] = distance[current.X, current.Y] + 1;

                if (next == target)
                    return distance[nx, ny];

                queue.Enqueue(next);
            }
        }

        return -1; // can't find a way
    }
}
