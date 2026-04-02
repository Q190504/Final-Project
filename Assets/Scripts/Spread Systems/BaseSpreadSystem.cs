using UnityEngine;

public enum SpreadMethodType
{
    None,
    Surface,
    Air,
    Water,
    Carrier,
}

public interface ISpreadMethod
{
    void Initialize();
    void Start();
    SpreadResult Execute();
    SpreadMethodConfig GetConfig();
}

public abstract class BaseSpreadMethod<T> : ISpreadMethod
    where T : SpreadMethodDataSO
{
    protected SpreadMethodType methodType;
    protected SpreadMethodConfig config;
    protected T data;

    protected Grid<GridCell> grid;

    protected TimeManager timeManager;
    protected MapManager mapManager;

    protected BaseSpreadMethod(Grid<GridCell> grid, T methodDataSO)
    {
        this.grid = grid;
        data = methodDataSO;
        config = methodDataSO.baseConfig;
        methodType = methodDataSO.baseConfig.methodType;
    }

    public void Initialize()
    {
        config.environmentModifiers?.Initialize();
        config.temperatureModifiers?.Initialize();
        config.populationModifiers?.Initialize();

        timeManager = TimeManager.Instance;
        mapManager = MapManager.Instance;
    }

    public virtual int GetInfectionPowerOfMethod(GridCell cell)
    {
        float modifier = 1f;

        EnvironmentModifierTable environmentModifiers = config.environmentModifiers;
        TemperatureModifierTable temperatureModifiers = config.temperatureModifiers;
        PopulationModifierTable populationModifiers = config.populationModifiers;

        if (environmentModifiers != null)
            modifier *= environmentModifiers.GetModifier(cell.Stats.environment.currentEnvironmentType);

        if (temperatureModifiers != null)
            modifier *= temperatureModifiers.GetModifier(cell.Stats.tempurature.type);

        if (populationModifiers != null)
            modifier *= populationModifiers.GetModifier(cell.Stats.population.type);

        return Mathf.RoundToInt(config.baseInfectionPower * modifier);
    }

    public virtual float GetDetectionRate() { return config.detectionRate; }

    public void Start()
    {
        ScheduleNext();
    }

    private void ScheduleNext()
    {
        if (timeManager != null)
        {
            timeManager.ScheduleEvent(
                config.tickToSpread,
                ExecuteInternal,
                config.eventPriority);
        }
    }

    private void ExecuteInternal()
    {
        SpreadResult result = Execute();
        ApplyResult(result);
        mapManager.UpdateDetectionRateOfMap();
        ScheduleNext();
    }

    public abstract SpreadResult Execute();

    public abstract void SetUpStatsAfterUpgrade();

    protected void ApplyResult(SpreadResult result)
    {
        foreach (CellDelta delta in result.GetAll())
        {
            GridCell cell = delta.cell;
            
            if (delta.infectionDelta != 0)
                cell.Stats.UpdateInfectionLevel(delta.infectionDelta);

            mapManager.AddCellNeedToUpdateVisual(new Vector2Int(cell.X, cell.Y));
        }
    }

    public SpreadMethodConfig GetConfig()
    {
        return config;
    }
}