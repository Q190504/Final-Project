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
    [SerializeField] private BoolPublisherSO onGameEndedSO;

    MapGenerator mapGenerator;
    private Grid<GridCell> grid;
    //private Grid<GridCell> frgrid;
    //private Grid<GridCell> scgrid;
    //private Grid<GridCell> thgrid;

    //private Grid<GridCell> currentGrid;
    private Vector2Int currentSize;

    private HashSet<Vector2Int> lockdownedCells = new();
    private HashSet<Vector2Int> cellsNeedToUpdateVisualNextTick = new();
    private HashSet<Vector2Int> cellsNeedToUpdateVisualInstantly = new();
    //bool isFirstTime = true;

    private float totalPopulation;
    private float totalDead;
    private float totalInfected;
    private float deadRate;
    private float infectedRate;
    private bool deadRateChanged = false;
    private bool infectedRateChanged = false;

    private HashSet<Vector2Int> safeCells = new();
    private HashSet<Vector2Int> infectedCells = new();
    private HashSet<Vector2Int> deadCells = new();

    private float previousUpdateTick = 0f;

    private UIManager uiManager;
    private EvolutionManager evolutionManager;

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
        uiManager = UIManager.Instance;
        evolutionManager = EvolutionManager.Instance;

        SetMapData();

        if (config != null && config.generateRandomSeed)
        {
            Random.InitState(System.Environment.TickCount);
            seed = Random.Range(0, int.MaxValue / 2);
            mapGenerator = new(seed, config);
        }
        else
            mapGenerator = new(config.seed, config);
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.B))
        //    BuildGrid(new Vector2Int(50, 50), frgrid);
        //if (Input.GetKeyDown(KeyCode.N))
        //    BuildGrid(new Vector2Int(100, 100), scgrid);
        //if (Input.GetKeyDown(KeyCode.M))
        //    BuildGrid(new Vector2Int(200, 200), thgrid);
    }

    public void Tick()
    {
        float currentTick = TimeManager.Instance.CurrentTick;
        float deltaTime = currentTick - previousUpdateTick;

        UpdateLockdownedCellsStatus(deltaTime);

        previousUpdateTick = currentTick;
    }

    public void StartMatch()
    {
        TimeManager.Instance.ScheduleEvent(0f, () => Tick(), EventPriority.MapUpdate);
    }

    void SetMapData()
    {
        width = config.width;
        height = config.height;
        cellSize = config.cellSize;
        originPosition = config.originPosition;

        mapBoundary.size = new Vector2(width + Mathf.FloorToInt(0.5f * width), height + Mathf.FloorToInt(0.5f * height));
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
        currentSize = new Vector2Int(width, height);
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

    public void UpdateMap(AIContext ctx)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CellStats cellStats = grid.GetCell(x, y).Stats;
                CellStructure cellStructure = cellStats.structure;

                cellStats.UpdateTickInfectedCount();
                cellStats.UpdateSterilizationImmunityTicks();

                if (cellStructure.type != StructureType.None && cellStructure.isActive)
                {
                    cellStructure.ApplyTickEffectToCellsInRange();
                }
                else if(!cellStructure.isActive)
                {
                    cellStructure.UpdateDisableTime();
                }

                if (!cellStats.isDetected)
                {
                    cellStats.RecalculateDetection(ctx);
                    AddCellNeedToUpdateVisualNotInstantly(new Vector2Int(x, y));
                }
            }
        }

        updateGridVisualSO.RaiseEvent();
    }

    public void AddInfectionResistanceByVaccineToMap(InfectionResistanceModifier infectionResistanceModifier)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridCell cell = grid.GetCell(x, y);
                cell.Stats.AddInfectionResistanceModifier(infectionResistanceModifier);
            }
        }
    }

    public void AddCellNeedToUpdateVisualNotInstantly(Vector2Int cell)
    {
        cellsNeedToUpdateVisualNextTick.Add(cell);
    }

    public HashSet<Vector2Int> GetCellNeedToUpdateVisualNextTickList()
    {
        return cellsNeedToUpdateVisualNextTick;
    }

    public void AddCellNeedToUpdateVisualInstantly(Vector2Int cell)
    {
        cellsNeedToUpdateVisualInstantly.Add(cell);
    }

    public HashSet<Vector2Int> GetCellNeedToUpdateVisualInstantlyList()
    {
        return cellsNeedToUpdateVisualInstantly;
    }


    //public float GetBaseSize()
    //{
    //    return Mathf.Sqrt(width * width + height * height);
    //}

    //public Grid<GridCell> GetGrid()
    //{
    //    return currentGrid;
    //}

    public void UpdateInfectedRateAndDeadRate(Vector2Int cellPos, CellStageType cellStageType, float value)
    {
        switch (cellStageType)
        {
            case CellStageType.Safe:
            case CellStageType.Immune:
                if (infectedCells.Contains(cellPos))
                {
                    totalInfected -= value;
                    infectedCells.Remove(cellPos);
                }

                if (!safeCells.Contains(cellPos))
                    safeCells.Add(cellPos);
                break;
            case CellStageType.Exposed:
                if (safeCells.Contains(cellPos))
                    safeCells.Remove(cellPos);

                if (!infectedCells.Contains(cellPos))
                {
                    totalInfected += value;
                    infectedCells.Add(cellPos);
                }

                break;
            case CellStageType.Dead:
                if (safeCells.Contains(cellPos))
                    safeCells.Remove(cellPos);

                if (!deadCells.Contains(cellPos))
                {
                    totalDead += value;
                    deadCells.Add(cellPos);
                }

                break;
            default:
                break;
        }

        float previousInfectedRate = infectedRate;
        float previousDeadRate = deadRate;

        infectedRate = totalInfected / totalPopulation;
        deadRate = totalDead / totalPopulation;

        if (infectedRate > previousInfectedRate)
            infectedRateChanged = true;

        if (deadRate > previousDeadRate)
            deadRateChanged = true;

        if (infectedRateChanged || deadRateChanged)
        {
            uiManager.UpdateActualInfectedRateAndDeadRate(totalInfected, infectedRate, totalDead, deadRate);
            uiManager.UpdateDeadSlider(deadRate, MatchManager.Instance.endGameDeadRate);
            evolutionManager.OnInfectedRateChanged(infectedRate);

            if (infectedRate == 0)
            {
                onGameEndedSO.RaiseEvent(false);
            }
            else if (deadRate == 1)
            {
                onGameEndedSO.RaiseEvent(true);
            }
        }
    }

    public void AddLockdownedCell(Vector2Int cell)
    {
        lockdownedCells.Add(cell);
    }

    public void RemoveLockdownedCell(Vector2Int cell)
    {
        lockdownedCells.Remove(cell);
    }

    public void UpdateLockdownedCellsStatus(float deltaTime)
    {
        foreach (Vector2Int cellPos in lockdownedCells)
        {
            GridCell cell = grid.GetCell(cellPos.x, cellPos.y);
            cell.Stats.UpdateLockdownDuration(deltaTime);
        }
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

    public float GetInfectedRate()
    {
        return infectedRate;
    }

    public MapConfig GetMapConfig()
    {
        return config;
    }
}
