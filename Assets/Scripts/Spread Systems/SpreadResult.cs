using System.Collections.Generic;
using UnityEngine;

public class SpreadResult
{
    private Dictionary<GridCell, InfectionInfo> cellChanges = new();

    public void Add(GridCell cell, InfectionInfo delta)
    {
        if (!cellChanges.TryGetValue(cell, out var existing))
        {
            cellChanges[cell] = delta;
            return;
        }

        existing.infectionDelta += delta.infectionDelta;

        cellChanges[cell] = existing;
    }

    public IEnumerable<InfectionInfo> GetAll()
    {
        return cellChanges.Values;
    }
}