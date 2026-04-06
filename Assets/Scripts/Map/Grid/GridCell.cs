using UnityEngine;

public class GridCell
{
    private Grid<GridCell> grid;

    public CellStats Stats { get; set; }

    public int X { get; set; }
    public int Y { get; set; }
    public bool IsBeingShownInfo { get; set; }

    public GridCell(Grid<GridCell> grid, int x, int y)
    {
        this.grid = grid;
        X = x;
        Y = y;

        Stats = new CellStats();

        SetCellData(PopulationType.None, TemperatureType.None, StructureType.None, 0);
    }

    public GridCell(Grid<GridCell> grid, int x, int y, PopulationType populationType, TemperatureType tempuratureType, StructureType structureType,
        int infectionLevel)
    {
        this.grid = grid;
        X = x;
        Y = y;

        Stats = new CellStats();

        SetCellData(populationType, tempuratureType, structureType, infectionLevel);
    }

    public void SetCellData(PopulationType populationType, TemperatureType tempuratureType, StructureType structureType, int infectionLevel)
    {
        Stats.population.type = populationType;
        Stats.tempurature.type = tempuratureType;
        Stats.environment.SetEnvironmentType(populationType, tempuratureType);
        Stats.structure.type = structureType;
        Stats.infectionLevel = infectionLevel;
        Stats.stage.SetCellStageType(infectionLevel, Stats);
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
            $"Current Infec Restist: {Stats.currentInfectionResistance}\n" +
            $"Original Infec Restist: {Stats.originalInfectionResistance}\n" +
            $"isContagious: {Stats.isContagious}\n" +
            $"isBlocked: {Stats.isBlocked}\n" +
            $"isLockdown: {Stats.isLockdown}\n" +
            $"isDetected: {Stats.isDetected}\n" +
            $"Current Detection Percent: {Stats.currentDetectionPercent}\n" +
            $"Original Detection Percent: {Stats.originalDetectionPercent}\n" +
            $"hasCarrier: {Stats.hasCarrier}\n" +
            $"Current Env: {Stats.environment.currentEnvironmentType}\n" +
            $"Original Env: {Stats.environment.originalEnvironmentType}\n" +
            $"Pop: {Stats.population.type}\n" +
            $"Temp: {Stats.tempurature.type}\n" +
            $"Currnent Water: {Stats.HasWater()}\n" +
            //$"Original Water: {Stats.originalHasWater}\n" +
            $"Structure: {Stats.structure.type}\n" +
            $"Affected by: {Stats.affectedByStructures}\n" +
            $"canSwitchToDead: {Stats.canSwitchToDead}\n" +
            $"toDeadTicksCount: {Stats.toDeadTicksCount}\n" +
            //$"canBeDisinfected: {Stats.canBeDisinfected}\n" +
            //$"disinfectionImmunityTicks: {Stats.disinfectionImmunityTicks}\n" +
            //$"disinfectionImmunityTicksCount: {Stats.disinfectionImmunityTicksCount}\n" +
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
            || Stats.stage.type == CellStageType.Critical;
    }

    public bool CanBeInfected()
    {
        return Stats.stage.type != CellStageType.Critical
            && Stats.stage.type != CellStageType.Dead
            && Stats.stage.type != CellStageType.Immune;
    }

    public float GetStageInfectionIncreasePercent()
    {
        return Stats.targetInfectionIncreasePercent;
    }

    public bool IsAffectedByStructure(StructureType structureType)
    {
        return Stats.affectedByStructures.Contains(structureType);
    }

    public void CheckIsBeingShownInfo()
    {
        if (IsBeingShownInfo)
        {
            CellInfoUIContentManager.Instance.SetVisibity(true, new Vector2Int(X, Y));
        }
    }
}
