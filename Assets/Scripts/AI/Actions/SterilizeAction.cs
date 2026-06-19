using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SterilizeAction : HumanAction
{
    private SterilizeActionExtraConfig extraConfig;
    private Grid<GridCell> grid;

    public SterilizeAction(HumanActionSO data, SterilizeActionExtraConfig extraConfig) : base(data)
    {
        this.extraConfig = extraConfig;
        grid = MapManager.Instance.GetGrid();

        PopulationData highPopulationData = PropertyDataManager.Instance.GetPopulationData(PopulationType.High);

        data.minUtility = data.maxTargetPerExecution * (-extraConfig.deadCellPenalty);
        data.maxUtility = data.maxTargetPerExecution * (Utility.maxInfectionLevel * highPopulationData.weight);
    }

    public override float EvaluateCell(GridCell cell, AIContext ctx)
    {
        var stats = cell.Stats;

        if (stats.isDetected && !stats.canBeSterilized)
            return 0f;

        float infection = stats.isDetected ? stats.infectionLevel : stats.infectionLevel * extraConfig.undetectedCellWeight;
        int sterilizationResistance = stats.isDetected ? stats.GetSterilizationResistance() : 0;

        float populationWeight = stats.population.weight;

        float deadCellPenalty = (stats.isDetected && stats.stage.type == CellStageType.Dead) ? extraConfig.deadCellPenalty : 0f;

        return (infection - sterilizationResistance) * populationWeight
            - deadCellPenalty;
    }

    public override void Execute(List<GridCell> cells, AIContext ctx, ThreatTier reducedCooldownTier, float reducedCooldownModifier)
    {
        float curve = Mathf.Pow(ctx.ThreatLevel, 2f);
        int sterilizeAmount = Mathf.FloorToInt(Mathf.Lerp(extraConfig.minSterilizedAmount, extraConfig.maxSterilizedAmount, curve));

        foreach (GridCell cell in cells)
        {
            CellStats cellStats = cell.Stats;
            if (cellStats.canBeSterilized)
            {
                int sterilizationResistance = cell.Stats.GetSterilizationResistance();
                int effectiveSterilizeAmount = sterilizeAmount - sterilizationResistance;
                if (effectiveSterilizeAmount > 0)
                    cellStats.UpdateInfectionLevel(-effectiveSterilizeAmount, true);
            }
        }

        RaiseExecuteVisual(cells, HumanActionType.Sterilize);

        base.Execute(cells, ctx, reducedCooldownTier, reducedCooldownModifier);
    }

    public override ActionInstance BuildBestInstances(AIContext ctx, SimulationCache simCache)
    {
        ActionInstance inst = new(this);

        float curve = Mathf.Pow(ctx.ThreatLevel, 2f);
        int targetCount = Mathf.FloorToInt(Mathf.Lerp(data.minTargetPerExecution, data.maxTargetPerExecution, curve));

        HashSet<GridCell> chosen = new();

        List<GridCell> PickFrom(IEnumerable<GridCell> source)
        {
            return source
                .Where(cell =>
                    cell.Stats.canBeSterilized &&
                    !cell.Stats.isLockdown &&
                    !simCache.lockdownedCells.Contains(cell))
                .OrderByDescending(cell => EvaluateCell(cell, ctx))
                .ToList();
        }

        void Fill(IEnumerable<GridCell> source)
        {
            if (chosen.Count >= targetCount)
                return;

            var sorted = PickFrom(source);

            foreach (var cell in sorted)
            {
                if (chosen.Count >= targetCount)
                    break;

                chosen.Add(cell);
            }
        }

        // Priority order
        Fill(ctx.CriticalCells);
        Fill(ctx.InfectedCells);
        Fill(ctx.ExposedCells);

        if (chosen.Count == 0)
        {
            inst.IsValid = false;
            return inst;
        }

        inst.Cells = chosen.ToList();
        inst.RawUtility = CalculateRawUtility(inst.Cells, ctx);
        inst.NormalizedUtility = NormalizeUtility(inst.RawUtility);
        inst.IsValid = true;

        return inst;
    }

    public override float CalculateRawUtility(List<GridCell> targets, AIContext ctx)
    {
        float rawUtility = 0;

        foreach (GridCell cell in targets)
        {
            CellStats stats = cell.Stats;

            if (!stats.canBeSterilized)
                continue;

            float infection = stats.isDetected ? stats.infectionLevel : stats.infectionLevel * extraConfig.undetectedCellWeight;
            int sterilizationResistance = stats.isDetected ? stats.GetSterilizationResistance() : 0;

            float populationWeight = stats.population.weight;

            rawUtility += (infection - sterilizationResistance) * populationWeight;
        }

        return rawUtility;
    }

    public override void ApplyLightSimulation(List<GridCell> targets, AIContext simCtx, SimulationCache simCache)
    {
        foreach (var cell in targets)
        {
            simCache.AddAffected(cell, 0.5f);
        }
    }
}
