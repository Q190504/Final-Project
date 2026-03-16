using System.Collections.Generic;
using UnityEngine;

public class MountainData
{
    public int targetLength;

    // each step of a mountain
    public List<MountainSegment> segments = new();
}

public class MountainSegment
{
    // store stamped cell to undo
    public List<Vector2Int> stampedCells = new();
}