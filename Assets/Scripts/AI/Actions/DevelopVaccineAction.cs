using System.Collections.Generic;
using UnityEngine;

public class DevelopVaccineAction : HumanAction
{
    private DevelopVaccineActionExtraConfig extraConfig;

    private int maxInfectionResistance;
    private int possibleInfectionResistanceIncreasementIfExecute;

    private VaccineSystem vaccineSystem;

    public DevelopVaccineAction(HumanActionSO data, DevelopVaccineActionExtraConfig extraConfig) : base(data)
    {
        this.extraConfig = extraConfig;
        vaccineSystem = VaccineSystem.Instance;

        maxInfectionResistance = Utility.maxInfectionResistance;
        possibleInfectionResistanceIncreasementIfExecute = 
            vaccineSystem.CalculateInfectionResistanceIncreasementIfExecute(extraConfig.VaccineProgressPerExecution);

        ThreatTierSO vaccineThreatTier = PropertyDataManager.Instance.GetThreatTierData(ThreatTier.Vaccine);

        float minVaccineUrgency = vaccineThreatTier.threatRange.min;
        float maxVaccineUrgency = vaccineThreatTier.threatRange.max;

        float minLackOfResearch = 0;
        float maxLackOfResearch = 1f;

        data.minUtility = minVaccineUrgency + minLackOfResearch;
        data.maxUtility = maxVaccineUrgency + maxLackOfResearch;
    }

    public override float EvaluateCell(GridCell cell, AIContext ctx)
    {
        return 0f;
    }

    public override void Execute(List<GridCell> cells, AIContext ctx, ThreatTier reducedCooldownTier, float reducedCooldownModifier)
    {
        vaccineSystem.UpdateProgress(extraConfig.VaccineProgressPerExecution);

        base.Execute(cells, ctx, reducedCooldownTier, reducedCooldownModifier);
    }

    public override bool CanExecute(AIContext ctx)
    {
        bool isNotOnCooldown = base.CanExecute(ctx);

        return isNotOnCooldown;
    }

    public override float CalculateRawUtility(List<GridCell> targets, AIContext ctx)
    {
        //int totalInfectionResistanceIncreasementIfExecute = 0;

        //List<GridCell> usefulIfIncreaseInfectionResistanceCells = new (ctx.SafeCells);
        //usefulIfIncreaseInfectionResistanceCells.AddRange(ctx.ExposedCells);    
        //usefulIfIncreaseInfectionResistanceCells.AddRange(ctx.InfectedCells);    

        //foreach (GridCell cell in usefulIfIncreaseInfectionResistanceCells)
        //{
        //    CellStats cellStats = cell.Stats;

        //    float undetectedCellWeight = cellStats.isDetected ? 1f : extraConfig.UndetectedCellWeight;

        //    int infectionResistanceRemainingAfterExecutionOfCell = Mathf.FloorToInt((cellStats.finalInfectionResistance * undetectedCellWeight) 
        //        + possibleInfectionResistanceIncreasementIfExecute) 
        //        - maxInfectionResistance;

        //    if (infectionResistanceRemainingAfterExecutionOfCell > 0)
        //        totalInfectionResistanceIncreasementIfExecute += possibleInfectionResistanceIncreasementIfExecute 
        //            - infectionResistanceRemainingAfterExecutionOfCell;
        //    else
        //        totalInfectionResistanceIncreasementIfExecute += possibleInfectionResistanceIncreasementIfExecute;
        //}

        float vaccineUrgency = ctx.ThreatLevel;

        //return vaccineUrgency + (1 - vaccineSystem.Progress) + totalInfectionResistanceIncreasementIfExecute;
        return vaccineUrgency + (1 - vaccineSystem.Progress);
    }

    public override void ApplyLightSimulation(List<GridCell> targets, AIContext simCtx, SimulationCache simCache)
    {
        simCtx.VaccineProgress += extraConfig.VaccineProgressPerExecution;
    }

    public override ActionInstance BuildBestInstances(AIContext ctx, SimulationCache simCache)
    {
        float rawUtility = CalculateRawUtility(null, ctx);
        float normalizedUtility = NormalizeUtility(rawUtility);
        return new ActionInstance(this, null, rawUtility, normalizedUtility);
    }
}
