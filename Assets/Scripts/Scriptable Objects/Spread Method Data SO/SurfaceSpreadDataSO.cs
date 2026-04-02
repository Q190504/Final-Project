using UnityEngine;

[System.Serializable]
public struct SurfaceExtraConfig
{

}

[CreateAssetMenu(fileName = "Surface Spread Data", menuName = "Scriptable Objects/Spread Method/Surface")]
public class SurfaceSpreadDataSO : SpreadMethodDataSO
{
    public SurfaceExtraConfig extraConfig;

    public override ISpreadMethod CreateMethod(Grid<GridCell> grid)
    {
        return new SurfaceSpreadMethod(grid, this);
    }
}