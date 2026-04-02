using UnityEngine;

[System.Serializable]
public struct WaterExtraConfig
{

}

[CreateAssetMenu(fileName = "Water Spread Data", menuName = "Scriptable Objects/Spread Method/Water")]
public class WaterSpreadDataSO : SpreadMethodDataSO
{
    public WaterExtraConfig extraConfig;
    public override ISpreadMethod CreateMethod(Grid<GridCell> grid)
    {
        return new WaterSpreadMethod(grid, this);
    }
}