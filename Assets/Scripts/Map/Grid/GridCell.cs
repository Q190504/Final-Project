using UnityEngine;

public class GridCell
{
    private Grid<GridCell> grid;
    public EnvironmentRegion Region { get; set; }
    public UrbanRegion Urban { get; set; }

    public CellStats Stats { get; set; }

    public int X { get; set; }
    public int Y { get; set; }
    public bool IsBeingShownInfo { get; set; }

    public float startingScore = -1;
    public float distanceToNearestUrban = -1;
    public float distanceToNearestWaterRegion = -1;
    public float nearestWaterRegionAssistScore = -1;

    public GridCell(Grid<GridCell> grid, int x, int y)
    {
        this.grid = grid;
        X = x;
        Y = y;

        Stats = new CellStats();
    }

    public string GetIndexToString()
    {
        return X + ", " + Y;
    }

    public void DebugStats()
    {
        Debug.Log($"Cell ({X}, {Y})\n" +
            $"Infec Level: {Stats.infectionLevel}\n" +
            $"Stage: {Stats.stage.type}\n" +
            $"Current Infec Restist: {Stats.GetInfectionResistance()}\n" +
            $"Base Infec Restist: {Stats.baseInfectionResistance}\n" +
            $"Current Sterilization Restist: {Stats.GetSterilizationResistance()}\n" +
            $"Base Sterilization Restist: {Stats.baseSterilizationResistance}\n" +
            $"isContagious: {Stats.isContagious}\n" +
            $"isBlocked: {Stats.isBlocked}\n" +
            $"isLockdown: {Stats.isLockdown}\n" +
            $"isDetected: {Stats.isDetected}\n" +
            $"Final Detection Percent: {Stats.finalDetection}\n" +
            $"Base Detection Percent: {Stats.baseDetection}\n" +
            $"hasCarrier: {Stats.hasCarrier}\n" +
            $"Current Env: {Stats.environment.currentEnvironmentType}\n" +
            $"Original Env: {Stats.environment.originalEnvironmentType}\n" +
            $"Pop: {Stats.population.type}\n" +
            $"Temp: {Stats.tempurature.type}\n" +
            $"Current Water: {Stats.HasWater()}\n" +
            $"Structure: {Stats.structure.type}\n" +
            $"Affected by: {Stats.affectedByStructures}\n" +
            $"canSwitchToDead: {Stats.canSwitchToDead}\n" +
            $"toDeadTicksCount: {Stats.toDeadTicksCount}\n" +
            $"canBeSterilized: {Stats.canBeSterilized}\n" +
            $"sterilizationImmunityTicks: {Stats.sterilizationImmunityTicks}\n" +
            $"priorityToHuman: {Stats.priorityToHuman}\n" +
            $"priorityToSurface: {Stats.priorityToMethods.surfacePriority}\n" +
            $"priorityToAir: {Stats.priorityToMethods.airPriority}\n" +
            $"priorityToWater: {Stats.priorityToMethods.waterPriority}\n" +
            $"priorityToCarrier: {Stats.priorityToMethods.carrierPriority}\n +");
    }

    public bool IsInfectious()
    {
        return Stats.stage.type == CellStageType.Exposed
            || Stats.stage.type == CellStageType.Infected
            || Stats.stage.type == CellStageType.Critical
            || Stats.stage.type == CellStageType.Dead;
    }

    public bool CanBeInfected()
    {
        return Stats.stage.type != CellStageType.Critical
            && Stats.stage.type != CellStageType.Dead
            && Stats.stage.type != CellStageType.Immune;
    }

    public float GetBonusInfectionGainPercentOfStage()
    {
        return Stats.bonusTargetInfectionGainPercent;
    }

    public bool IsAffectedByStructure(StructureType structureType)
    {
        return Stats.affectedByStructures.Contains(structureType);
    }

    public void CheckIsBeingShownInfo()
    {
        if (IsBeingShownInfo)
        {
            CellInfoUIContentManager.Instance.SetVisibility(true, new Vector2Int(X, Y));
        }
    }
}
