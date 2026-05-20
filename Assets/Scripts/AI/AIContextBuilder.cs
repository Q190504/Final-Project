using System.Collections.Generic;
using UnityEngine;

public class AIContextBuilder
{
    private Grid<GridCell> grid;
    private HumanAIManager humanAIManager;
    private UIManager uiManager;
    private MapManager mapManager;
    private AIContextConfigSO config;

    public AIContextBuilder(Grid<GridCell> grid, AIContextConfigSO config)
    {
        this.grid = grid;
        this.config = config;
        humanAIManager = HumanAIManager.Instance;
        uiManager = UIManager.Instance;
        mapManager = MapManager.Instance;
    }

    public void Build(AIContext ctx, int tick)
    {
        ctx.Reset();
        ctx.CurrentTick = tick;
        ctx.VaccineProgress = VaccineSystem.Instance.Progress;

        CollectDetectedCells(ctx);
        ComputeGlobalStats(ctx);
        ClassifyCells(ctx);
    }

    private void CollectDetectedCells(AIContext ctx)
    {
        foreach (GridCell cell in grid.GetGrid())
        {
            if (!cell.Stats.isDetected)
                continue;

            if (!ctx.DetectedCells.Contains(cell))
                ctx.DetectedCells.Add(cell);
        }
    }

    private void ComputeGlobalStats(AIContext ctx)
    {
        float infectedWeight = 0f;
        float deadWeight = 0f;

        float weightedInfectionLevel = 0f;
        float infectedPopulation = 0f;

        float totalPopulation = mapManager.GetTotalPopulation();

        foreach (GridCell cell in ctx.DetectedCells)
        {
            float popWeight = cell.Stats.population.weight;

            int infectionLevel = cell.Stats.infectionLevel;

            if (popWeight <= 0)
                continue;

            if (cell.IsInfectious())
            {
                infectedWeight += popWeight;

                weightedInfectionLevel += infectionLevel * popWeight;

                infectedPopulation += popWeight;
            }
            else if (cell.Stats.stage.type == CellStageType.Dead)
            {
                deadWeight += popWeight;

                weightedInfectionLevel += infectionLevel * popWeight;

                infectedPopulation += popWeight;
            }
        }

        if (totalPopulation > 0)
        {
            ctx.InfectionRateDetected = infectedWeight / totalPopulation;

            ctx.DeadRateDetected = deadWeight / totalPopulation;
        }

        uiManager
            .UpdateDetectedInfectedRateAndDeadRate(
                infectedWeight,
                ctx.InfectionRateDetected,
                deadWeight,
                ctx.DeadRateDetected
            );

        // ----------------------------
        // Base threat
        // ----------------------------

        float infectedThreat = ctx.InfectionRateDetected * humanAIManager.InfectionRateDetectedWeight;

        float deathThreat = ctx.DeadRateDetected * humanAIManager.DeadRateDetectedWeight;

        float baseThreat = infectedThreat + deathThreat;

        // ----------------------------
        // Infection pressure multiplier
        // ----------------------------

        float avgInfectionLevel = 0f;

        if (infectedPopulation > 0)
            avgInfectionLevel = weightedInfectionLevel / infectedPopulation;

        float normalizedInfectionPressure = Mathf.Clamp01(avgInfectionLevel / Utility.maxInfectionLevel);

        float infectionPressureMultiplier = Mathf.Lerp(1f, humanAIManager.MaxInfectionPressureMultiplier, normalizedInfectionPressure);

        // ----------------------------
        // Skill multiplier
        // ----------------------------

        float normalizedSkillUse = Mathf.Clamp01(ctx.VirusSkillsUsedCount / humanAIManager.MaxSkillUseThreat);

        float skillMultiplier = Mathf.Lerp(1f, humanAIManager.MaxSkillMultiplier, normalizedSkillUse);

        // ----------------------------
        // Final threat
        // ----------------------------

        float threatLevel =
            baseThreat
            * infectionPressureMultiplier
            * skillMultiplier;

        ctx.ThreatLevel =
            Mathf.Clamp01(threatLevel);

        UpdateThreatTier(ctx);

        Debug.Log(
        $"ThreatLevel: {ctx.ThreatLevel:F2} | " +
        $"ThreatTier: {ctx.ThreatTier} | " +
        $"InfectedRate: {ctx.InfectionRateDetected:F2} | " +
        $"DeadRate: {ctx.DeadRateDetected:F2} | " +
        $"AvgInfectionLevel: {avgInfectionLevel:F2} | " +
        $"SkillCount: {ctx.VirusSkillsUsedCount}"
        );
    }

    private void UpdateThreatTier(AIContext ctx)
    {
        List<ThreatTierSO> threatTierSOs = PropertyDataManager.Instance.GetThreatTierSOs();

        for (int i = 0; i < threatTierSOs.Count; i++)
        {
            var tier = threatTierSOs[i];
            bool isLast = i == threatTierSOs.Count - 1;

            bool inRange = isLast
                ? ctx.ThreatLevel >= tier.threatRange.min &&
                  ctx.ThreatLevel <= tier.threatRange.max
                : ctx.ThreatLevel >= tier.threatRange.min &&
                  ctx.ThreatLevel < tier.threatRange.max;

            if (inRange)
            {
                ctx.ThreatTier = tier.tierType;
                return;
            }
        }

        ctx.ThreatTier = ThreatTier.Unaware; // Default to lowest tier if no match
    }

    private void ClassifyCells(AIContext ctx)
    {
        ctx.SafeCells.Clear();
        ctx.ExposedCells.Clear();
        ctx.InfectedCells.Clear();
        ctx.CriticalCells.Clear();
        ctx.ImmuneCells.Clear();
        ctx.DetectedDeadCells.Clear();
        ctx.DetectedContagiousCells.Clear();
        ctx.LockdownedCells.Clear();

        foreach (GridCell cell in ctx.DetectedCells)
        {
            if (cell.Stats.isLockdown)
                ctx.LockdownedCells.Add(cell);

            CellStageType type = cell.Stats.stage.type;

            switch (type)
            {
                case CellStageType.Safe:
                    ctx.SafeCells.Add(cell);
                    break;

                case CellStageType.Exposed:
                    ctx.ExposedCells.Add(cell);
                    ctx.DetectedContagiousCells.Add(cell);
                    break;

                case CellStageType.Infected:
                    ctx.InfectedCells.Add(cell);
                    ctx.DetectedContagiousCells.Add(cell);
                    break;

                case CellStageType.Critical:
                    ctx.CriticalCells.Add(cell);
                    ctx.DetectedContagiousCells.Add(cell);
                    break;

                case CellStageType.Dead:
                    ctx.DetectedDeadCells.Add(cell);
                    break;

                case CellStageType.Immune:
                    ctx.ImmuneCells.Add(cell);
                    break;
            }
        }
    }
}