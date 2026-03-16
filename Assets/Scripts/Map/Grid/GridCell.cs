using UnityEngine;

public class GridCell
{
    private Grid<GridCell> grid;

    public CellStats Stats { get; set; }

    public int X { get; set; }
    public int Y { get; set; }
    public bool HasVisualChange { get; set; }

    public GridCell(Grid<GridCell> grid, int x, int y)
    {
        this.grid = grid;
        X = x;
        Y = y;

        Stats = new CellStats();

        SetCellData(PopulationType.None, TempuratureType.None, StructureType.None, 0);
    }

    public GridCell(Grid<GridCell> grid, int x, int y, PopulationType populationType, TempuratureType tempuratureType, StructureType structureType,
        int infectionLevel)
    {
        this.grid = grid;
        X = x;
        Y = y;

        Stats = new CellStats();

        SetCellData(populationType, tempuratureType, structureType, infectionLevel);
    }

    public void SetCellData(PopulationType populationType, TempuratureType tempuratureType, StructureType structureType, int infectionLevel)
    {
        Stats.population.type = populationType;
        Stats.tempurature.type = tempuratureType;
        Stats.environment.SetEnvironmentType(populationType, tempuratureType);
        Stats.structure.type = structureType;
        Stats.infectionLevel = infectionLevel;
        Stats.stage.SetCellStageType(infectionLevel);
    }

    public string GetIndexToString()
    {
        return X + ", " + Y;
    }

    public void DebugStats()
    {
        Debug.Log($"Cell ({X}, {Y})\n" +
            $"Pop: {Stats.population.type}\n" +
            $"Temp: {Stats.tempurature.type}\n" +
            $"Env: {Stats.environment.currentEnvironmentType}\n" +
            $"Struc: {Stats.structure.type}\n" +
            $"Infec: {Stats.infectionLevel}\n" +
            $"Stage: {Stats.stage.type}\n" +
            $"Water: {Stats.hasWater}\n" +
            $"isContagious: {Stats.isContagious}\n" +
            $"isBlocked: {Stats.isBlocked}\n" +
            $"isLockdown: {Stats.isLockdown}\n" +
            $"hasCarrier: {Stats.hasCarrier}\n" +
            $"canSwitchToDead: {Stats.canSwitchToDead}\n" +
            $"toDeadTicks: {Stats.toDeadTicks}\n" +
            $"toDeadTicksCount: {Stats.toDeadTicksCount}\n" +
            $"canBeDisinfected: {Stats.canBeDisinfected}\n" +
            $"disinfectionImmunityTicks: {Stats.disinfectionImmunityTicks}\n" +
            $"disinfectionImmunityTicksCount: {Stats.disinfectionImmunityTicksCount}\n" +
            $"priorityToHuman: {Stats.priorityToHuman}\n" +
            $"priorityToSurface: {Stats.priorityToMethods.surfacePriority}\n" +
            $"priorityToAir: {Stats.priorityToMethods.airPriority}\n" +
            $"priorityToWater: {Stats.priorityToMethods.waterPriority}\n" +
            $"priorityToCarrier: {Stats.priorityToMethods.carrierPriority}\n +");
    }
}
