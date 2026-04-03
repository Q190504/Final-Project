using System.Collections.Generic;
//using System.Diagnostics;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("Grid")]
    private int width;
    private int height;
    private float cellSize = 1f;
    private Vector3 originPosition = Vector3.zero;
    private int seed;

    [Header("Map Config")]
    [SerializeField] private MapConfig config;
    [SerializeField] private BoxCollider2D mapBoundary;

    [Header("Events")]
    [SerializeField] private VoidPublisherSO spawnGridVisualSO;
    [SerializeField] private VoidPublisherSO updateGridVisualSO;

    MapGenerator mapGenerator;
    private Grid<GridCell> grid;
    //private Grid<GridCell> frgrid;
    //private Grid<GridCell> scgrid;
    //private Grid<GridCell> thgrid;

    //private Grid<GridCell> currentGrid;
    private Vector2Int currentSize;

    private HashSet<Vector2Int> cellsNeedToUpdateVisual = new();
    //bool isFirstTime = true;

    private float totalPopulation;
    private float totalDead;
    private float totalInfected;
    private float deadRate;
    private float infectedRate;
    private bool deadRateChanged = false;
    private bool infectedRateChanged = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetMapData();

        if (config != null && config.generateRandomSeed)
        {
            System.Random random = new();
            seed = random.Next(0, int.MaxValue / 2);
            config.seed = seed;
        }

        mapGenerator = new(seed, config);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            BuildGrid();

        //if (Input.GetKeyDown(KeyCode.B))
        //    BuildGrid(new Vector2Int(50, 50), frgrid);
        //if (Input.GetKeyDown(KeyCode.N))
        //    BuildGrid(new Vector2Int(100, 100), scgrid);
        //if (Input.GetKeyDown(KeyCode.M))
        //    BuildGrid(new Vector2Int(200, 200), thgrid);
    }

    void SetMapData()
    {
        width = config.width;
        height = config.height;
        cellSize = config.cellSize;
        originPosition = config.originPosition;

        mapBoundary.size = new Vector2(width + Mathf.RoundToInt(0.3f * width), height + Mathf.RoundToInt(0.3f * height));
        CameraController.Instance.Init();
    }

    //void BuildGrid(Vector2Int currentGridSize, Grid<GridCell> grid)
    //{
    //    currentGrid = grid;
    //    currentSize = currentGridSize;

    //    List<double> times = new();
    //    double avg = 0;
    //    double min = double.MaxValue;
    //    double max = double.MinValue;

    //    for (int i = 0; i < 100; i++)
    //    {
    //        currentGrid = new Grid<GridCell>(currentGridSize.x, currentGridSize.y, cellSize, originPosition,
    //            (grid, x, y) => new GridCell(grid, x, y), showDebug);

    //        Stopwatch sw = Stopwatch.StartNew();

    //        mapGenerator.Generate(currentGrid);

    //        sw.Stop();

    //        double t = sw.Elapsed.TotalMilliseconds;

    //        times.Add(t);
    //        avg += t;
    //        if (t < min) min = t;
    //        if (t > max) max = t;
    //    }

    //    avg /= times.Count;

    //    UnityEngine.Debug.Log($"Average: {avg} ms");
    //    UnityEngine.Debug.Log($"Min: {min} ms");
    //    UnityEngine.Debug.Log($"Max: {max} ms");

    //    if (isFirstTime)
    //    {
    //        spawnGridVisualSO.RaiseEvent();
    //        isFirstTime = false;
    //    }
    //}

    public void BuildGrid()
    {
        grid = new Grid<GridCell>(width, height, cellSize, originPosition,
      (grid, x, y) => new GridCell(grid, x, y));

        mapGenerator.Generate(grid);

        spawnGridVisualSO.RaiseEvent();
    }

    public float GetBaseSize()
    {
        return Mathf.Sqrt(currentSize.x * currentSize.x + currentSize.y * currentSize.y);
    }

    public Grid<GridCell> GetGrid()
    {
        return grid;
    }


    public void UpdateDetectionRateOfMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridCell cell = grid.GetCell(x, y);
                if (cell.Stats.infectionLevel > 0)
                {
                    cell.Stats.UpdateDetectionRate();
                    AddCellNeedToUpdateVisual(new Vector2Int(x, y));
                }
            }
        }

        updateGridVisualSO.RaiseEvent();
    }

    public void AddCellNeedToUpdateVisual(Vector2Int cell)
    {
        cellsNeedToUpdateVisual.Add(cell);
    }

    public HashSet<Vector2Int> GetCellNeedToUpdateVisualList()
    {
        return cellsNeedToUpdateVisual;
    }


    //public float GetBaseSize()
    //{
    //    return Mathf.Sqrt(width * width + height * height);
    //}

    //public Grid<GridCell> GetGrid()
    //{
    //    return currentGrid;
    //}

    public void UpdateDeadRate(float value)
    {
        float previousDeadRate = deadRate;

        totalDead += value;
        deadRate = totalDead / totalPopulation;

        if (deadRate > previousDeadRate)
            deadRateChanged = true;
    }

    public void UpdateInfectedRate(float value)
    {
        float previousInfectedRate = infectedRate;

        totalInfected += value;
        infectedRate = totalInfected / totalPopulation;

        if (infectedRate > previousInfectedRate)
            infectedRateChanged = true;
    }

    public void UpdateTotalPopulation(float value)
    {
        totalPopulation += value;
    }

    public float GetTotalPopulation()
    { return totalPopulation; }

    public float GetTotalDead()
    { return totalDead; }

    public float GetDeadRate()
    { return deadRate; }
}
