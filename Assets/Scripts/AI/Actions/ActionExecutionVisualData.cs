using System.Collections.Generic;

public struct ActionExecutionVisualData
{
    public List<GridCell> Cells;
    public HumanActionType ActionType;

    public ActionExecutionVisualData(
        List<GridCell> cells,
        HumanActionType actionType)
    {
        Cells = cells;
        ActionType = actionType;
    }
}