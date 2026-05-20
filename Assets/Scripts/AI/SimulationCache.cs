using System.Collections.Generic;

public class SimulationCache
{
    public HashSet<GridCell> affectedCells = new();
    public HashSet<GridCell> lockdownedCells = new();
    public HashSet<GridCell> builtStructureCells = new();

    public Dictionary<GridCell, float> cellPenalty = new();

    public void AddAffected(GridCell cell, float penalty = 0.5f)
    {
        affectedCells.Add(cell);
        cellPenalty[cell] = penalty;
    }

    public void AddLockdowned(GridCell cell)
    {
        lockdownedCells.Add(cell);
    }

    public void AddBuiltStructure(GridCell cell)
    {
        builtStructureCells.Add(cell);
    }

    public bool IsLockdowned(GridCell cell) => lockdownedCells.Contains(cell);

    public bool HasBuiltStructure(GridCell cell) => builtStructureCells.Contains(cell);

    public float GetPenalty(GridCell cell)
    {
        if (cellPenalty.TryGetValue(cell, out var p))
            return p;
        return 1f;
    }

    public SimulationCache Clone()
    {
        SimulationCache clone = new()
        {
            affectedCells = new HashSet<GridCell>(affectedCells),
            lockdownedCells = new HashSet<GridCell>(lockdownedCells),
            builtStructureCells = new HashSet<GridCell>(builtStructureCells),
            cellPenalty = new Dictionary<GridCell, float>(cellPenalty)
        };
        return clone;
    }
}