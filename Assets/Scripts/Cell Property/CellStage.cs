using System.Collections.Generic;
using UnityEngine;

public class CellStage
{
    public CellStageType type;

    public float detectionPercent;

    public float priorityToHuman;
    public PriorityToMethods priorityToMethods;

    private InfectionResistanceModifier infectionResistanceModifier = null;

    private List<CellStageType> stagesReachedBefore;

    public CellStage()
    {
        type = CellStageType.None;
        stagesReachedBefore = new List<CellStageType> { CellStageType.Safe };
    }

    public (CellStageType, PointsGainedStruct) SetCellStageType(int infectionLevel, CellStats cellStats)
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

                PointsGainedStruct totalPointsGained = new();
                if (!stagesReachedBefore.Contains(type))
                {
                    stagesReachedBefore.Add(type);
                    float evolutionPointMultiplier = SkillManager.Instance.GetEvolutionPointMultiplier();
                    float infectionPointMultiplier = SkillManager.Instance.GetInfectionPointMultiplier();

                    totalPointsGained.evolutionPoints = Mathf.RoundToInt(cellStageData.cellStageStats.evolutionPointsGained
                        * cellStats.population.weight
                        * evolutionPointMultiplier);

                    totalPointsGained.infectionPoints = Mathf.RoundToInt(cellStageData.cellStageStats.infectionPointsGained
                        * cellStats.population.weight
                        * infectionPointMultiplier);
                }

                priorityToHuman = cellStageData.cellStageStats.priorityToHuman;
                detectionPercent = cellStageData.cellStageStats.detectionPercent;

                int infectionResistance = cellStageData.cellStageStats.infectionResistance;
                if (infectionResistance > 0)
                {
                    if (infectionResistanceModifier != null)
                    {
                        infectionResistanceModifier.Value = infectionResistance;
                    }
                    else
                    {
                        infectionResistanceModifier = new(infectionResistance, InfectionResistanceAdditiveSourceType.CellStage, ModifierType.Additive);
                        cellStats.AddInfectionResistanceModifier(infectionResistanceModifier);
                    }
                }

                cellStats.DetermineInfectionStats(cellStageData.cellStageStats);

                if (cellStats.structure != null)
                {
                    if (type == CellStageType.Critical)
                    {
                        float evolutionPointMultiplier = SkillManager.Instance.GetEvolutionPointMultiplier();
                        float infectionPointMultiplier = SkillManager.Instance.GetInfectionPointMultiplier();

                        PointsGainedStruct structurePointsGained = cellStats.structure.DisableStructure();

                        totalPointsGained.evolutionPoints = Mathf.RoundToInt(structurePointsGained.evolutionPoints
                            * evolutionPointMultiplier);

                        totalPointsGained.infectionPoints = Mathf.RoundToInt(structurePointsGained.infectionPoints
                            * infectionPointMultiplier);
                    }
                    else if ((type == CellStageType.Safe
                        || type == CellStageType.Exposed
                        || type == CellStageType.Infected
                        || type == CellStageType.Immune)
                        && !cellStats.structure.isActive)
                    {
                        cellStats.structure.EnableStructure();
                    }
                    else if (type == CellStageType.Dead)
                    {
                        float evolutionPointMultiplier = SkillManager.Instance.GetEvolutionPointMultiplier();
                        float infectionPointMultiplier = SkillManager.Instance.GetInfectionPointMultiplier();

                        PointsGainedStruct structurePointsGained = cellStats.structure.DestroyStructure();

                        totalPointsGained.evolutionPoints = Mathf.RoundToInt(structurePointsGained.evolutionPoints
                            * evolutionPointMultiplier);

                        totalPointsGained.infectionPoints = Mathf.RoundToInt(structurePointsGained.infectionPoints
                            * infectionPointMultiplier);
                    }
                }

                return (type, totalPointsGained);
            }
        }

        return (CellStageType.None, new PointsGainedStruct());
    }
}
