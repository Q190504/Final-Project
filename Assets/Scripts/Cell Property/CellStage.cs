using UnityEngine;

public class CellStage
{
    public CellStageType type;

    public float priorityToHuman;
    public PriorityToMethods priorityToMethods;

    public CellStage()
    {
        type = CellStageType.None;
    }

    public CellStageType SetCellStageType(int infectionLevel, CellStats cellStats)
    {
        foreach (CellStageData cellStageData in CellPropertyManager.Instance.GetCellStageDatas())
        {
            if (cellStageData != null && cellStageData.cellStageStats.minInfectionValue <= infectionLevel
                && infectionLevel <= cellStageData.cellStageStats.maxInfectionValue)
            {
                type = cellStageData.type;
                cellStats.DetermineInfectionFlags(cellStageData.cellStageStats);
                return type;
            }
        }

        return CellStageType.None;
    }

    public void SetCellStageType(CellStageType cellStageType, CellStats cellStats)
    {
        CellStageData cellStageData = CellPropertyManager.Instance.GetCellStageData(cellStageType);
        if (cellStageData != null)
        {
            type = cellStageData.type;
            cellStats.DetermineInfectionFlags(cellStageData.cellStageStats);
        }
    }
}
