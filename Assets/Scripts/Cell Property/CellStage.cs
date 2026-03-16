using UnityEngine;

public class CellStage
{
    public CellStageType type;
    public CellStageStats stats;

    public CellStage()
    {
        type = CellStageType.None;
        stats = new CellStageStats();
    }

    public CellStage(int infectionLevel)
    {
        foreach (CellStageData cellStageData in CellPropertyManager.Instance.GetCellStageDatas())
        {
            if (cellStageData != null && cellStageData.cellStageStats.minInfectionValue <= infectionLevel 
                && infectionLevel <= cellStageData.cellStageStats.maxInfectionValue)
            {
                type = cellStageData.type;
                stats = cellStageData.cellStageStats;
                return;
            }
        }
    }

    public CellStageType SetCellStageType(int infectionLevel)
    {
        foreach (CellStageData cellStageData in CellPropertyManager.Instance.GetCellStageDatas())
        {
            if (cellStageData != null && cellStageData.cellStageStats.minInfectionValue <= infectionLevel
                && infectionLevel <= cellStageData.cellStageStats.maxInfectionValue)
            {
                type = cellStageData.type;
                stats = cellStageData.cellStageStats;
                return type;
            }
        }

        return CellStageType.None;
    }
}
