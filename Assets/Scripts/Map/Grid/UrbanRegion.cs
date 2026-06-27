using System.Collections.Generic;
using UnityEngine;

public class UrbanRegion
{
    public int Id;

    public List<GridCell> Cells = new();
    public List<GridCell> BoundaryCells = new();
    public int CellCount;

    public GridCell CenterCell;

    public float PopulationWeight;

    public float StructureWeight;

    public float TotalWeight;
}
