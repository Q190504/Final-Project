using UnityEngine;

[System.Serializable]
public struct WaterExtraConfig
{
    public bool increaseRangeIfAffectedByWaterFactory;

    public float infectionGainPercentForWaterCell;

    public bool increaseBaseInfectionPowerIfWaterFactoryDisabled;
    public float bonusBaseInfectionPowerPercentIfAWaterFactoryDisabled;
    public int totalDisabledWaterFactory;

    public SterilizationResistanceModifier sterilizationResistanceModifierCellHasWater;

    public float chanceToSpreadToNonWaterCell;
}

[CreateAssetMenu(fileName = "Water Spread Data", menuName = "Scriptable Objects/Spread Method/Water")]
public class WaterSpreadDataSO : SpreadMethodDataSO
{
    public WaterExtraConfig extraConfig;

    public override ISpreadMethod CreateMethod(SpreadMethodContext context)
    {
        return new WaterSpreadMethod(context, this);
    }

    public override SpreadMethodRuntimeData CreateRuntimeData()
    {
        return new WaterRuntimeData(this);
    }
}