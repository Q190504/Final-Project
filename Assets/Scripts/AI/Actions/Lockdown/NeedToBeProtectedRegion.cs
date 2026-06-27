using System.Collections.Generic;
using UnityEngine;

public class NeedToBeProtectedRegion
{
    public HashSet<GridCell> Cells = new();

    public HashSet<GridCell> BoundaryCells = new();

    public float TotalProtectValue;
}
