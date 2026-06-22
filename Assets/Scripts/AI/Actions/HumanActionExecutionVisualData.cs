using System.Collections.Generic;

public struct HumanActionExecutionVisualData
{
    public List<GridCell> Cells;
    public HumanActionType ActionType;

    public HumanActionExecutionVisualData(List<GridCell> cells, HumanActionType actionType)
    {
        Cells = cells;
        ActionType = actionType;
    }
}