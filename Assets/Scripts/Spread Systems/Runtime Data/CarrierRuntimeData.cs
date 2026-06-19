using UnityEngine;

public class CarrierRuntimeData : SpreadMethodRuntimeData
{
    public CarrierExtraConfig extraConfig;

    public CarrierRuntimeData(CarrierSpreadDataSO methodDataSO) : base(methodDataSO)
    {
        methodType = SpreadMethodType.Carrier;
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

        float valueFloat = Mathf.Clamp(addition * multiplier, Utility.minInfectionLevel, Utility.maxInfectionLevel);
        int valueInt = Mathf.FloorToInt(valueFloat);

        return valueInt;
    }
}
