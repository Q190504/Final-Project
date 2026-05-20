using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DisinfectAction : HumanAction
{
    private DisinfectActionExtraConfig extraConfig;
    private Grid<GridCell> grid;

    public DisinfectAction(HumanActionSO data, DisinfectActionExtraConfig extraConfig) : base(data)
    {
        this.extraConfig = extraConfig;
        grid = MapManager.Instance.GetGrid();

        PopulationData highPopulationData = PropertyDataManager.Instance.GetPopulationData(PopulationType.High);

        data.minUtility = 0;
        data.maxUtility = data.maxTargetPerExecution * Utility.maxInfectionLevel * highPopulationData.weight;
    }

    public override float EvaluateCell(GridCell cell, AIContext ctx)
    {
        var stats = cell.Stats;

        float infection = stats.infectionLevel;
        float populationWeight = stats.population.weight;

        return infection * populationWeight;
    }

    public override void Execute(List<GridCell> cells, AIContext ctx, ThreatTier reducedCooldownTier, float reducedCooldownModifier)
    {
        foreach (GridCell cell in cells)
        {
            cell.Stats.UpdateInfectionLevel(-extraConfig.disinfectAmount);
        }

        RaiseExecuteVisual(cells, HumanActionType.Disinfect);

        base.Execute(cells, ctx, reducedCooldownTier, reducedCooldownModifier);
    }

    public override ActionInstance BuildBestInstances(AIContext ctx, SimulationCache simCache)
    {
        ActionInstance inst = new(this);

        float curve = Mathf.Pow(ctx.ThreatLevel, 2f);
        int targetCount = Mathf.RoundToInt(Mathf.Lerp(data.minTargetPerExecution, data.maxTargetPerExecution, curve));

        HashSet<GridCell> chosen = new();

        List<GridCell> PickFrom(IEnumerable<GridCell> source)
        {
            return source
                .Where(cell =>
                    cell.Stats.canBeDisinfected &&
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
            rawUtility += stats.infectionLevel * stats.population.weight * (stats.isDetected ? 1f : extraConfig.undetectedCellWeight);
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
