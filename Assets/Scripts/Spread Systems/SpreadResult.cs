using System.Collections.Generic;
using UnityEngine;

public class SpreadResult
{
    private Dictionary<GridCell, CellDelta> cellChanges = new();

    public void Add(GridCell cell, CellDelta delta)
    {
        if (!cellChanges.TryGetValue(cell, out var existing))
        {
            cellChanges[cell] = delta;
            return;
        }

        existing.infectionDelta += delta.infectionDelta;

        cellChanges[cell] = existing;
    }

    public IEnumerable<CellDelta> GetAll()
    {
        return cellChanges.Values;
    }
}