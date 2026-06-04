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
    void Tick(float deltaTime);
    SpreadResult Execute();
    SpreadMethodConfig GetBaseConfig();

    SpreadMethodRuntimeData GetRuntimeData();

    SpreadMethodContext GetContext();
}

public abstract class BaseSpreadMethod<TData, TRuntime> : ISpreadMethod
    where TData : SpreadMethodDataSO
    where TRuntime : SpreadMethodRuntimeData
{
    protected TData data;
    protected TRuntime runtimeData;
    protected SpreadMethodContext context;

    protected BaseSpreadMethod(SpreadMethodContext context, TData data, TRuntime runtimeData)
    {
        this.context = context;
        this.data = data;
        this.runtimeData = runtimeData;
    }

    public virtual int GetInfectionPowerOfMethod(GridCell originCell, GridCell targetCell)
    {
        return runtimeData.GetFinalInfectionPower(originCell, targetCell);
    }

    public void Tick(float deltaTime)
    {
        if (GameManager.Instance.GetGameState() == GameState.Playing
            && runtimeData != null)
        {
            runtimeData.remainingTicksToSpread -= deltaTime;
            if (runtimeData.remainingTicksToSpread <= 0)
            {
                ExecuteInternal();
                runtimeData.remainingTicksToSpread = runtimeData.GetFinalTickToSpread();
            }
        }
    }

    private void ExecuteInternal()
    {
        SpreadResult result = Execute();
        ApplyResult(result);
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

    public SpreadMethodConfig GetBaseConfig()
    {
        return data.baseConfig;
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