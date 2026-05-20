using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildVaccineResearchCenterAction : HumanAction
{
    BuildVaccineResearchCenterActionExtraConfig extraConfig;

    private int currentVaccineResearchCenters = 0;
    private int maxVaccineResearchCenters = 0;

    private StructureDataSO vaccineResearchCenterData;
    private int MAX_EMPTY_INFECTION_LEVEL;
    private int MAX_INFECTION_LEVEL;
    private float maxPopulationWeight;
    private int detectSafeRadius;

    private VaccineSystem vaccineSystem;
    private MapManager mapManager;
    private Grid<GridCell> grid;

    public BuildVaccineResearchCenterAction(HumanActionSO data, BuildVaccineResearchCenterActionExtraConfig extraConfig) : base(data)
    {
        this.extraConfig = extraConfig;
        mapManager = MapManager.Instance;
        grid = mapManager.GetGrid();
        vaccineSystem = VaccineSystem.Instance;

        MAX_EMPTY_INFECTION_LEVEL = Utility.maxInfectionLevel;
        MAX_INFECTION_LEVEL = Utility.maxInfectionLevel;

        PopulationData highPopulationData = PropertyDataManager.Instance.GetPopulationData(PopulationType.High);
        maxPopulationWeight = highPopulationData.weight;
        PopulationData lowPopulationData = PropertyDataManager.Instance.GetPopulationData(PopulationType.Low);

        float minPopulationWeight = lowPopulationData.weight;

        vaccineResearchCenterData = PropertyDataManager.Instance.GetStructureData(StructureType.VaccineResearchCenter);
        detectSafeRadius = Mathf.FloorToInt(extraConfig.detectSafeRadiusPercent * MapManager.Instance.GetBaseSize());

        maxVaccineResearchCenters = vaccineResearchCenterData.maxAmountInAMatch;

        ThreatTierSO vaccineThreatTier = PropertyDataManager.Instance.GetThreatTierData(ThreatTier.Vaccine);

        float minVaccineUrgency = vaccineThreatTier.threatRange.min;
        float maxVaccineUrgency = vaccineThreatTier.threatRange.max;

        float minLackOfResearch = 0;
        float maxLackOfResearch = 1f;

        float maxCellsInRange = Mathf.PI * detectSafeRadius * detectSafeRadius;

        float minCellScore = -MAX_INFECTION_LEVEL * minPopulationWeight;
        float maxCellScore = MAX_EMPTY_INFECTION_LEVEL * maxPopulationWeight;

        float minPlacementQuality = maxCellsInRange * minCellScore;
        float maxPlacementQuality = maxCellsInRange * maxCellScore;

        data.minUtility = minVaccineUrgency + minLackOfResearch + minPlacementQuality;
        data.maxUtility = maxVaccineUrgency + maxLackOfResearch + maxPlacementQuality;
    }

    public override bool CanExecute(AIContext ctx)
    {
        if (vaccineSystem.Stage != VaccineDevelopmentStage.Researching)
            return false;

        // check if already has max vaccine research centers
        if (currentVaccineResearchCenters >= maxVaccineResearchCenters)
            return false;

        return base.CanExecute(ctx);
    }

    public override float EvaluateCell(GridCell cell, AIContext ctx)
    {
        float cellScore = 0;

        List<GridCell> neighbors = grid.GetNeighbourInCircleWithRange(cell.X, cell.Y, detectSafeRadius);

        foreach (GridCell neighborCell in neighbors)
        {
            CellStats neighborStats = neighborCell.Stats;

            if (neighborStats.isDetected)
            {
                if (!neighborStats.IsSafe())
                    cellScore += MAX_EMPTY_INFECTION_LEVEL * neighborStats.population.weight;
                else
                {
                    cellScore -= neighborStats.infectionLevel * neighborStats.population.weight;
                }
            }
            else
            {
                cellScore -= neighborStats.infectionLevel * neighborStats.population.weight
                    * extraConfig.undetectedCellWeight;
            }
        }

        return cellScore;
    }

    public override float CalculateRawUtility(List<GridCell> targets, AIContext ctx)
    {
        if (targets.Count == 0)
            return 0f;

        float vaccineUrgency = ctx.ThreatLevel;

        float lackOfResearch = 1f - vaccineSystem.Progress;

        float placementQuality = EvaluateCell(targets[0], ctx);

        return vaccineUrgency + lackOfResearch + placementQuality;
    }

    public override ActionInstance BuildBestInstances(AIContext ctx, SimulationCache simCache)
    {
        ActionInstance inst = new(this);

        // 1. Collect candidates
        GridCell bestCell = null;
        float bestScore = float.MinValue;

        void Fill(IEnumerable<GridCell> source)
        {
            if (bestCell != null)
                return;

            foreach (GridCell candidateCell in source)
            {
                if (!candidateCell.Stats.canBuildStructure
                    || candidateCell.Stats.structure.type != StructureType.None
                    || simCache.HasBuiltStructure(candidateCell))
                    continue;

                float score = EvaluateCell(candidateCell, ctx);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCell = candidateCell;
                }
            }
        }

        Fill(ctx.ImmuneCells);
        Fill(ctx.SafeCells);
        Fill(ctx.ExposedCells);
        Fill(ctx.InfectedCells);
        Fill(ctx.CriticalCells);

        if (bestCell == null)
        {
            inst.IsValid = false;
            return inst;
        }

        // 2. take top cell
        List<GridCell> targets = new() { bestCell };

        float rawUtility = CalculateRawUtility(targets, ctx);
        float normalizedUtility = NormalizeUtility(rawUtility);

        inst = new(this, targets, rawUtility, normalizedUtility);

        return inst;
    }

    public override void ApplyLightSimulation(List<GridCell> targets, AIContext simCtx, SimulationCache simCache)
    {
        foreach (var cell in targets)
        {
            simCache.AddBuiltStructure(cell);
        }
    }

    public override void Execute(List<GridCell> cells, AIContext ctx, ThreatTier reducedCooldownTier, float reducedCooldownModifier)
    {
        if (cells.Count == 0 || currentVaccineResearchCenters >= maxVaccineResearchCenters)
            return;

        foreach (GridCell cell in cells)
        {
            cell.Stats.SetStructure(StructureType.VaccineResearchCenter, cell);
            currentVaccineResearchCenters++;
        }

        RaiseExecuteVisual(cells, HumanActionType.BuildVaccineResearchCenter);

        base.Execute(cells, ctx, reducedCooldownTier, reducedCooldownModifier);
    }
}
