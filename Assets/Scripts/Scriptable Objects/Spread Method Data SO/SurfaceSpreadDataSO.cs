using UnityEngine;

[System.Serializable]
public struct SurfaceExtraConfig
{
    public int sterilizationImmunityTicks;
    public float infectionPowerPercentBonusEachNeighbor;

    public float targetInfectionGainPercentForCriticalOriginCell;
    public int minTickToBonusSterilizationResistancePercent;
    public SterilizationResistanceModifier sterilizationResistanceModifierIfInfectedForALongTime;
}

[CreateAssetMenu(fileName = "Surface Spread Data", menuName = "Scriptable Objects/Spread Method/Surface")]
public class SurfaceSpreadDataSO : SpreadMethodDataSO
{
    public SurfaceExtraConfig extraConfig;

    public override ISpreadMethod CreateMethod(SpreadMethodContext context, SpreadMethodRuntimeData runtimeData)
    {
        return new SurfaceSpreadMethod(context, this, (SurfaceRuntimeData)runtimeData);
    }

    public override SpreadMethodRuntimeData CreateRuntimeData()
    {
        return new SurfaceRuntimeData(this);
    }
}