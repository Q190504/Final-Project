using System.Collections.Generic;
using UnityEngine;

public class BuildHospitalAction : HumanAction
{
    private BuildHospitalExtraConfig extraConfig;

    private int currentHospitals = 0;
    private int maxHospitals = 0;
    private int hospitalEffectRange = 0;
    private int maxInfectionResistance;

    private MapManager mapManager;
    private Grid<GridCell> grid;
    private StructureDataSO hospitalData;

    public BuildHospitalAction(HumanActionSO data, BuildHospitalExtraConfig extraConfig) : base(data)
    {
        this.extraConfig = extraConfig;
        mapManager = MapManager.Instance;
        grid = mapManager.GetGrid();
        hospitalData = PropertyDataManager.Instance.GetStructureData(StructureType.Hospital);
        maxHospitals = hospitalData.maxAmountInAMatch;
        hospitalEffectRange = Mathf.FloorToInt(hospitalData.effectRangePercent * MapManager.Instance.GetBaseSize());

        float highPopulationWeight = PropertyDataManager.Instance.GetPopulationData(PopulationType.High).weight;

        maxInfectionResistance = Utility.maxInfectionResistance;

        data.minUtility = 0;
        int maxCellsInRange = Mathf.FloorToInt(Mathf.PI * hospitalEffectRange * hospitalEffectRange);
        int maxInfectionBonus = Utility.maxInfectionLevel;
        data.maxUtility = (highPopulationWeight * maxInfectionResistance + maxInfectionBonus) * maxCellsInRange;
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

        Fill(ctx.CriticalCells);
        Fill(ctx.InfectedCells);
        Fill(ctx.ExposedCells);

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

    public override float CalculateRawUtility(List<GridCell> targets, AIContext ctx)
    {
        if (targets == null || targets.Count == 0)
            return 0f;

        GridCell targetCell = targets[0];

        float rawUtility = 0f;
        //float hospitalPenalty = 0f;

        List<GridCell> neighbors = grid.GetNeighbourInCircleWithRange(targetCell.X, targetCell.Y, hospitalEffectRange);

        foreach (GridCell neighborCell in neighbors)
        {
            CellStats neighborStats = neighborCell.Stats;

            if (neighborStats.stage.type == CellStageType.Dead)
                continue;

            float detectWeight = neighborStats.isDetected ? 1f : extraConfig.undetectedCellWeight;

            // Distance
            float dist = Mathf.Abs(targetCell.X - neighborCell.X) + Mathf.Abs(targetCell.Y - neighborCell.Y);

            float resistance = neighborStats.finalInfectionResistance * detectWeight;
            float need = maxInfectionResistance - resistance;
            // bonus if build on high infected area
            float infectionBonus = neighborStats.infectionLevel * detectWeight;

            float contribution = neighborStats.population.weight * need + infectionBonus;

            rawUtility += contribution;

            //// HOSPITAL DISTANCE PENALTY
            //if (neighborStats.structure.type == StructureType.Hospital)
            //{
            //    float d = Mathf.Max(1f, dist);
            //    hospitalPenalty += extraConfig.hospitalDistancePenaltyWeight / d;
            //}
        }

        //// penalty if build near other hospitals
        //rawUtility -= hospitalPenalty;

        return rawUtility;
    }

    public override bool CanExecute(AIContext ctx)
    {
        // check if already has max hospitals
        if (currentHospitals >= maxHospitals)
            return false;

        return base.CanExecute(ctx);
    }

    public override float EvaluateCell(GridCell cell, AIContext ctx)
    {
        float totalBenefit = 0f;
        float infectionBonus = 0f;
        float hospitalPenalty = 0f;

        int cellCount = 0;

        List<GridCell> neighbors = grid.GetNeighbourInCircleWithRange(cell.X, cell.Y, hospitalEffectRange);
        foreach (GridCell neighborCell in neighbors)
        {
            cellCount++;

            CellStats neighborStats = neighborCell.Stats;

            if (neighborStats.stage.type == CellStageType.Dead)
                continue;

            float detectWeight = neighborStats.isDetected ? 1f : extraConfig.undetectedCellWeight;

            // Distance
            float dist = Mathf.Abs(neighborCell.X - cell.X) + Mathf.Abs(neighborCell.Y - cell.Y);

            // BENEFIT
            float resistanceLack = (maxInfectionResistance - neighborStats.finalInfectionResistance);
            float benefit =
                neighborStats.population.weight *
                resistanceLack *
                detectWeight;

            totalBenefit += benefit;

            // INFECTION BONUS
            infectionBonus += neighborStats.infectionLevel * detectWeight;

            // HOSPITAL DISTANCE PENALTY
            if (neighborStats.structure.type == StructureType.Hospital)
            {
                float d = Mathf.Max(1f, dist);
                hospitalPenalty += extraConfig.hospitalDistancePenaltyWeight / d;
            }
        }

        float avg = totalBenefit / cellCount;

        // Combine
        float finalScore =
            avg * extraConfig.avgBenefitWeight
            + totalBenefit * extraConfig.totalBenefitWeight
            + infectionBonus
            - hospitalPenalty;

        return finalScore;
    }

    public override void ApplyLightSimulation(List<GridCell> targets, AIContext simCtx, SimulationCache simCache)
    {
        foreach (var cell in targets)
        {
            simCache.HasBuiltStructure(cell);
        }
    }

    public override void Execute(List<GridCell> cells, AIContext ctx, ThreatTier reducedCooldownTier, float reducedCooldownModifier)
    {
        if (cells.Count == 0 || currentHospitals >= maxHospitals)
            return;

        foreach (GridCell cell in cells)
        {
            cell.Stats.SetStructure(StructureType.Hospital, cell);
            currentHospitals++;
        }

        RaiseExecuteVisual(cells, HumanActionType.BuildHospital);

        base.Execute(cells, ctx, reducedCooldownTier, reducedCooldownModifier);
    }
}
