using UnityEditor.Rendering.LookDev;
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
    void Start();
    SpreadResult Execute();
    SpreadMethodConfig GetConfig();

    SpreadMethodRuntimeData GetRuntimeData();

    SpreadMethodContext GetContext();
}

public abstract class BaseSpreadMethod<T> : ISpreadMethod where T : SpreadMethodDataSO
{
    protected SpreadMethodType methodType;
    protected SpreadMethodConfig config;
    protected SpreadMethodRuntimeData runtimeData;
    protected SpreadMethodContext context;

    protected BaseSpreadMethod(SpreadMethodContext context, T methodDataSO)
    {
        this.context = context;
        config = methodDataSO.baseConfig;
        methodType = methodDataSO.baseConfig.methodType;
        runtimeData = methodDataSO.CreateRuntimeData();
    }

    public virtual int GetInfectionPowerOfMethod(GridCell originCell, GridCell targetCell)
    {
        return runtimeData.GetFinalInfectionPower(originCell, targetCell);
    }

    public void Start()
    {
        ScheduleNext();
    }

    private void ScheduleNext()
    {
        if (context.TimeManager != null)
        {
            context.TimeManager.ScheduleEvent(
                config.tickToSpread,
                ExecuteInternal,
                config.eventPriority,
                config.methodOrder);
        }
    }

    private void ExecuteInternal()
    {
        SpreadResult result = Execute();
        ApplyResult(result);
        ScheduleNext();
    }

    public abstract SpreadResult Execute();

    protected void ApplyResult(SpreadResult result)
    {
        bool hasSpread = false;
        PointsGainedStruct totalPointsGained = new();

        foreach (InfectionInfo infectionInfo in result.GetAll())
        {
            GridCell cell = infectionInfo.cell;
            CellStats stats = cell.Stats;

            if (infectionInfo.infectionDelta != 0)
            {
                hasSpread = true;

                CellStageType previousStage = stats.stage.type;

                PointsGainedStruct pointsGained = stats.UpdateInfectionLevel(infectionInfo.infectionDelta);
                CellStageType afterStage = stats.stage.type;

                totalPointsGained.evolutionPoints += pointsGained.evolutionPoints;
                totalPointsGained.infectionPoints += pointsGained.infectionPoints;

                // If the cell got infected, check if need to add sterilization resistance (source is method) to it
                if (runtimeData.GetFinalSterilizationResistance() > 0
                    && !stats.HasSterilizationResistanceModifier(runtimeData.methodSterilizationResistanceModifier))
                    stats.AddSterilizationResistanceModifier(runtimeData.methodSterilizationResistanceModifier);

                if (infectionInfo.minTickToBonusSterilizationResistance > 0
                    && infectionInfo.sterilizationResistanceModifierIfInfectedForALongTime != null)
                {
                    stats.AddSterilizationResistancBonusPercentIfInfectedForALongTime(infectionInfo.minTickToBonusSterilizationResistance,
                        infectionInfo.sterilizationResistanceModifierIfInfectedForALongTime);
                }

                //if (infectionInfo.sterilizationResistanceBonusEachInfectedNeighbor > 0)
                //    stats.UpdateSterilizationResistanceBonusEachInfectedNeighbor(infectionInfo.sterilizationResistanceBonusEachInfectedNeighbor);

                if (stats.HasWater() && infectionInfo.sterilizationResistanceModifierForWaterCell != null)
                {
                    stats.AddSterilizationResistancBonusPercentIfCellHasWater(infectionInfo.sterilizationResistanceModifierForWaterCell);
                }

                if (stats.canBeSterilized
                    && previousStage == CellStageType.Safe
                    && (afterStage != CellStageType.Safe || afterStage != CellStageType.Immune))
                    stats.SetSterilizationImmunityTicks(infectionInfo.disinfectionImmunityTicks);

                context.MapManager.AddCellNeedToUpdateVisual(new Vector2Int(cell.X, cell.Y));

            }
        }

        if (hasSpread)
        {
            PointsManager.Instance.AddEvolutionPoints(totalPointsGained.evolutionPoints);
            PointsManager.Instance.AddInfectionPoints(totalPointsGained.infectionPoints);
        }
    }

    public SpreadMethodConfig GetConfig()
    {
        return config;
    }

    public SpreadMethodRuntimeData GetRuntimeData()
    {
        return runtimeData;
    }

    public SpreadMethodContext GetContext()
    {
        return context;
    }
}