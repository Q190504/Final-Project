using UnityEngine;

[System.Serializable]
public struct CarrierExtraConfig
{
    public float travelTime;
    public float baseCarrierSpawnChance;
    public float structurePriority;
    public int targetCellCountForEachOriginCell;
    public float weightBonusForSafeCells;
}

[CreateAssetMenu(fileName = "Carrier Spread Data", menuName = "Scriptable Objects/Spread Method/Carrier")]
public class CarrierSpreadDataSO : SpreadMethodDataSO
{
    public CarrierExtraConfig extraConfig;

    public override ISpreadMethod CreateMethod(SpreadMethodContext context)
    {
        return new CarrierSpreadMethod(context, this, new CarrierRuntimeData(this));
    }
}