using UnityEngine;

[System.Serializable]
public struct AirExtraConfig
{

}

[CreateAssetMenu(fileName = "Airborne Spread Data", menuName = "Scriptable Objects/Spread Method/Airborne")]
public class AirSpreadDataSO : SpreadMethodDataSO
{
    public AirExtraConfig extraConfig;

    public override ISpreadMethod CreateMethod(Grid<GridCell> grid)
    {
        return new AirSpreadMethod(grid, this);
    }
}