using UnityEngine;

public class WaterRuntimeData : SpreadMethodRuntimeData
{
    public WaterExtraConfig extraConfig;

    public WaterRuntimeData(WaterSpreadDataSO methodDataSO) : base(methodDataSO)
    {
        methodType = SpreadMethodType.Water;
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

        if (extraConfig.increaseBaseInfectionPowerIfWaterFactoryDisabled
            && extraConfig.totalDisabledWaterFactory > 0
            && extraConfig.bonusBaseInfectionPowerPercentIfAWaterFactoryDisabled > 0)
            multiplier *= 1 + extraConfig.totalDisabledWaterFactory * extraConfig.bonusBaseInfectionPowerPercentIfAWaterFactoryDisabled;

        multiplier *= 1 + originStats.bonusTargetInfectionGainPercent;

        if (extraConfig.infectionGainPercentForWaterCell > 0
            && targetStats.HasWater())
            multiplier *= 1 + extraConfig.infectionGainPercentForWaterCell;

        float valueFloat = Mathf.Clamp(addition * multiplier, Utility.minInfectionLevel, Utility.maxInfectionLevel);
        int valueInt = Mathf.FloorToInt(valueFloat);

        int increaseInfectionLevel = valueInt - targetStats.finalInfectionResistance;

        return increaseInfectionLevel;
    }
}
