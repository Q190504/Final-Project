using UnityEngine;

public abstract class SpreadMethodDataSO : ScriptableObject
{
    public SpreadMethodConfig baseConfig;

    public abstract ISpreadMethod CreateMethod(Grid<GridCell> grid);
}
