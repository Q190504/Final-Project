using UnityEngine;

public class SurfaceRuntimeData : SpreadMethodRuntimeData
{
    public SurfaceExtraConfig extraConfig;

    public SurfaceRuntimeData(SurfaceSpreadDataSO methodDataSO) : base(methodDataSO)
    {
        methodType = SpreadMethodType.Surface;
        extraConfig = methodDataSO.extraConfig;
    }

    public override int GetFinalInfectionPower(GridCell originCell, GridCell targetCell)
    {
        CellStats originStats = originCell.Stats;
        CellStats targetStats = targetCell.Stats;
        float addition = baseInfectionPower + additiveInfectionPower;

        float multiplier = multiplicativeInfectionPower;

        if (environmentModifiers != null)
            multiplier *= environmentModifiers.GetModifier(originStats.environment.currentEnvironmentType);

        if (temperatureModifiers != null)
            multiplier *= temperatureModifiers.GetModifier(originStats.tempurature.type);

        if (populationModifiers != null)
            multiplier *= populationModifiers.GetModifier(originStats.population.type);

        multiplier *= 1 + originStats.bonusTargetInfectionGainPercent;

        if (extraConfig.infectionPowerPercentBonusEachNeighbor > 0)
        {
            int infectedNeigbors = 0;
            foreach (GridCell neighbor in MapManager.Instance.GetGrid().GetNeighborsInRange(originCell, 1))
            {
                if (neighbor.Stats.isContagious)
                    infectedNeigbors++;
            }

            multiplier *= 1 + infectedNeigbors * extraConfig.infectionPowerPercentBonusEachNeighbor;
        }

        if (extraConfig.targetInfectionGainPercentForCriticalOriginCell > 0
            && originStats.stage.type == CellStageType.Critical)
            multiplier *= 1 + extraConfig.targetInfectionGainPercentForCriticalOriginCell;

        float valueFloat = Mathf.Clamp(addition * multiplier, Utility.minInfectionLevel, Utility.maxInfectionLevel);
        int valueInt = Mathf.FloorToInt(valueFloat);

        int increaseInfectionLevel = valueInt - targetStats.finalInfectionResistance;

        return increaseInfectionLevel;
    }
}