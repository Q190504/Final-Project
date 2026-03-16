using System.Collections.Generic;
using UnityEngine;

public class UrbanCluster
{
    public List<Vector2Int> Cells { get; }

    public UrbanCluster(List<Vector2Int> cells)
    {
        Cells = cells;
    }

    public int Size => Cells.Count;
}
