using System.Collections.Generic;

public struct SpreadMethodExecutionVisualData
{
    public List<GridCell> Cells;
    public SpreadMethodType MethodType;

    public SpreadMethodExecutionVisualData(List<GridCell> cells, SpreadMethodType methodType)
    {
        Cells = cells;
        MethodType = methodType;
    }
}
