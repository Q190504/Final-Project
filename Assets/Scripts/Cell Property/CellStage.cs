using UnityEngine;

public class CellStage
{
    public CellStageType type;

    public float detectionPercent;

    public float priorityToHuman;
    public PriorityToMethods priorityToMethods;

    private InfectionResistanceModifier infectionResistanceModifier = null;

    public CellStage()
    {
        type = CellStageType.None;
    }

    public CellStageType SetCellStageType(int infectionLevel, CellStats cellStats)
    {
        CellStageType previousType = type;

        foreach (CellStageData cellStageData in PropertyDataManager.Instance.GetCellStageDatas())
        {
            // Immnune or Dead is only set by stage, not infectionLevel
            if (cellStageData.type == CellStageType.Immune
                || cellStageData.type == CellStageType.Dead
                || cellStageData.type == CellStageType.None)
                continue;

            if (cellStageData != null && cellStageData.cellStageStats.minInfectionValue <= infectionLevel
                && infectionLevel <= cellStageData.cellStageStats.maxInfectionValue
                && cellStageData.type != previousType)
            {
                type = cellStageData.type;
                priorityToHuman = cellStageData.cellStageStats.priorityToHuman;
                detectionPercent = cellStageData.cellStageStats.detectionPercent;

                int infectionResistance = cellStageData.cellStageStats.infectionResistance;
                if (infectionResistance > 0)
                {
                    if (infectionResistanceModifier != null)
                    {
                        cellStats.UpdateInfectionResistanceModifierValue(infectionResistanceModifier, infectionResistance);
                    }
                    else
                    {
                        infectionResistanceModifier = new(infectionResistance, InfectionResistanceAdditiveSourceType.CellStage);
                        cellStats.AddInfectionResistanceModifier(infectionResistanceModifier);
                    }
                }

                cellStats.DetermineInfectionStats(cellStageData.cellStageStats);

                if (cellStats.structure != null)
                {
                    if ((type == CellStageType.Infected || type == CellStageType.Critical) && cellStats.structure.isActive)
                    {
                        cellStats.structure.DisableStructure();
                    }
                    else if ((type == CellStageType.Safe
                        || type == CellStageType.Exposed
                        || type == CellStageType.Immune)
                        && !cellStats.structure.isActive)
                    {
                        cellStats.structure.EnableStructure();
                    }
                    else if (type == CellStageType.Dead)
                    {
                        cellStats.structure.DestroyStructure();
                    }
                }

                return type;
            }
        }

        return CellStageType.None;
    }

    public void SetCellStageType(CellStageType cellStageType, CellStats cellStats)
    {
        CellStageType previousType = type;

        CellStageData cellStageData = PropertyDataManager.Instance.GetCellStageData(cellStageType);
        if (cellStageData != null && cellStageData.type != previousType)
        {
            type = cellStageData.type;
            priorityToHuman = cellStageData.cellStageStats.priorityToHuman;
            detectionPercent = cellStageData.cellStageStats.detectionPercent;

            int infectionResistance = cellStageData.cellStageStats.infectionResistance;
            if (infectionResistance > 0)
            {
                if (infectionResistanceModifier != null)
                {
                    cellStats.UpdateInfectionResistanceModifierValue(infectionResistanceModifier, infectionResistance);
                }
                else
                {
                    infectionResistanceModifier = new(infectionResistance, InfectionResistanceAdditiveSourceType.CellStage);
                    cellStats.AddInfectionResistanceModifier(infectionResistanceModifier);
                }
            }

            cellStats.DetermineInfectionStats(cellStageData.cellStageStats);

            if (cellStats.structure != null)
            {
                if ((type == CellStageType.Infected || type == CellStageType.Critical) && cellStats.structure.isActive)
                {
                    cellStats.structure.DisableStructure();
                }
                else if ((type == CellStageType.Safe
                    || type == CellStageType.Exposed
                    || type == CellStageType.Immune)
                    && !cellStats.structure.isActive)
                {
                    cellStats.structure.EnableStructure();
                }
                else if (type == CellStageType.Dead)
                {
                    cellStats.structure.DestroyStructure();
                }
            }
        }
    }
}
