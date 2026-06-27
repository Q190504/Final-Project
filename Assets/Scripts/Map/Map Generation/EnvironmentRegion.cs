using System.Collections.Generic;

public class EnvironmentRegion
{
    public int Id;
    public EnvironmentType Type;

    public List<GridCell> Cells = new();
    public List<GridCell> BoundaryCells = new();

    public int CellCount;
    public float Length;

    public float HelpExpandPower;

    public float AssistScore;
}