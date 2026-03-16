using System.Collections.Generic;
//using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("Grid")]
    private int width;
    private int height;
    private float cellSize = 1f;
    private Vector3 originPosition = Vector3.zero;
    private int seed;
    [SerializeField] private bool showDebug;

    [Header("Prefabs")]
    [SerializeField] private GridCellVisual cellViewPrefab;

    [Header("Parents")]
    [SerializeField] private Transform cellViewParent;

    [Header("Map Config")]
    [SerializeField] private MapConfig config;

    [Header("Input")]
    [SerializeField] private InputAction getCellInfoAction;

    MapGenerator mapGenerator;
    private Grid<GridCell> grid;
    //private Grid<GridCell> frgrid;
    //private Grid<GridCell> scgrid;
    //private Grid<GridCell> thgrid;

    //private Grid<GridCell> currentGrid;
    private Vector2Int currentSize;

    private CellPresenter[,] presenters;

    //bool isFirstTime = true;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is enabled
    /// </summary>
    void OnEnable()
    {
        getCellInfoAction.Enable();
    }

    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is disabled
    /// </summary>
    void OnDisable()
    {
        getCellInfoAction.Disable();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (getCellInfoAction.bindings.Count == 0)
        {
            Debug.LogWarning("The Get Cell Info Action does not have a binding set! Make sure that each Input Action has a binding set or the controller will not work!");
        }

        SetMapData();

        if (config != null && config.generateRandomSeed)
        {
            System.Random random = new();
            seed = random.Next(0, int.MaxValue / 2);
            config.seed = seed;
        }
        mapGenerator = new(seed, config);

        CameraController.Instance.SetBounds(width, height, cellSize, originPosition);
        //BuildGrid();
    }

    // Update is called once per frame
    void Update()
    {
        if (showDebug)
        {
            if (getCellInfoAction.triggered)
            {
                Vector3 mouseWorldPos = Utility.GetMouseWorldPosition();
                Vector2Int cellPos = Utility.WorldToGridPosition(
                    mouseWorldPos,
                    width,
                    height,
                    cellSize
                );
                if (grid != null && grid.IsInBounds(cellPos.x, cellPos.y))
                {
                    GridCell cell = grid.GetCell(cellPos.x, cellPos.y);
                    cell.DebugStats();
                }
            }
        }

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
    //        presenters = new CellPresenter[currentGridSize.x, currentGridSize.y];

    //        for (int x = 0; x < currentGridSize.x; x++)
    //        {
    //            for (int y = 0; y < currentGridSize.y; y++)
    //            {
    //                GridCell cell = currentGrid.GetCell(x, y);
    //                GridCellVisual view = SpawnCellView(x, y);

    //                presenters[x, y] = new CellPresenter(cell, view);
    //                presenters[x, y].Refresh();
    //            }
    //        }

    //        isFirstTime = false;
    //    }
    //}

    void BuildGrid()
    {
        bool isFirstTime = true;

        grid = new Grid<GridCell>(width, height, cellSize, originPosition,
      (grid, x, y) => new GridCell(grid, x, y), showDebug);

        mapGenerator.Generate(grid);

        if (isFirstTime)
        {
            presenters = new CellPresenter[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    GridCell cell = grid.GetCell(x, y);
                    GridCellVisual view = SpawnCellView(x, y);

                    presenters[x, y] = new CellPresenter(cell, view);
                    presenters[x, y].Refresh();
                }
            }

            isFirstTime = false;
        }
    }

    private GridCellVisual SpawnCellView(int x, int y)
    {
        GridCellVisual view =
            Instantiate(cellViewPrefab, cellViewParent);

        Vector3 worldPos = Utility.GridToWorldPosition(x, y, width, height, cellSize);
        view.transform.position = worldPos;

        view.name = $"CellView ({x},{y})";

        return view;
    }

    public float GetBaseSize()
    {
        return Mathf.Sqrt(currentSize.x * currentSize.x + currentSize.y * currentSize.y);
    }

    public Grid<GridCell> GetGrid()
    {
        return grid;
    }

    //public float GetBaseSize()
    //{
    //    return Mathf.Sqrt(width * width + height * height);
    //}

    //public Grid<GridCell> GetGrid()
    //{
    //    return currentGrid;
    //}
}
