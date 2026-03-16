using System.Collections.Generic;
using UnityEngine;

public class RiverData
{
    public int targetLength;

    // each step of a river
    public List<RiverSegment> segments = new();
}

public class RiverSegment
{
    // store stamped cell to undo
    public List<Vector2Int> stampedCells = new();
}