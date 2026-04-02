using UnityEngine;

[System.Serializable]
public struct CarrierExtraConfig
{
    public float travelTime;
    public float baseCarrierSpawnChance;
    public float structurePriority;
    public int speardCellCount;
}

[CreateAssetMenu(fileName = "Carrier Spread Data", menuName = "Scriptable Objects/Spread Method/Carrier")]
public class CarrierSpreadDataSO : SpreadMethodDataSO
{
    public CarrierExtraConfig extraConfig;

    public override ISpreadMethod CreateMethod(Grid<GridCell> grid)
    {
        return new CarrierSpreadMethod(grid, this);
    }
}