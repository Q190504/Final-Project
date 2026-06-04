using UnityEngine;

public class AirRuntimeData : SpreadMethodRuntimeData
{
    public AirExtraConfig extraConfig;

    public AirRuntimeData(AirSpreadDataSO methodDataSO) : base(methodDataSO)
    {
        methodType = SpreadMethodType.Air;
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

        float dist = new Vector2Int(originCell.X - targetCell.X, originCell.Y - targetCell.Y).magnitude;
        float distanceMultiplier = Mathf.Pow(extraConfig.distanceDecayFactor, dist);
        distanceMultiplier = Mathf.Max(0f, distanceMultiplier);
        multiplier *= 1 - distanceMultiplier;

        float valueFloat = Mathf.Clamp(addition * multiplier, Utility.minInfectionLevel, Utility.maxInfectionLevel);
        int valueInt = Mathf.FloorToInt(valueFloat);

        int increaseInfectionLevel = valueInt - targetStats.finalInfectionResistance;

        return increaseInfectionLevel;
    }
}
