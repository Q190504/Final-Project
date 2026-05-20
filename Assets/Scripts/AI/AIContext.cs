using System.Collections.Generic;
using UnityEngine;

public class AIContext
{
    public int CurrentTick;

    // Threat
    public float ThreatLevel;
    public ThreatTier ThreatTier;

    // Global (detected only)
    public int VirusSkillsUsedCount;
    public float InfectionRateDetected;
    public float DeadRateDetected;
    public float VaccineProgress;

    // Cells
    public List<GridCell> DetectedCells = new();
    public List<GridCell> DetectedContagiousCells = new();
    public List<GridCell> DetectedDeadCells = new();

    public List<GridCell> CriticalCells = new();
    public List<GridCell> InfectedCells = new();
    public List<GridCell> ExposedCells = new();
    public List<GridCell> SafeCells = new();
    public List<GridCell> ImmuneCells = new();

    public List<GridCell> LockdownedCells = new();

    public void Reset()
    {
        DetectedCells.Clear();
        DetectedContagiousCells.Clear();
        DetectedDeadCells.Clear();

        CriticalCells.Clear();
        InfectedCells.Clear();
        ExposedCells.Clear();
        SafeCells.Clear();
        ImmuneCells.Clear();

        LockdownedCells.Clear();
    }

    public AIContext Clone()
    {
        AIContext clone = new();
        clone.CurrentTick = CurrentTick;
        clone.ThreatLevel = ThreatLevel;
        clone.ThreatTier = ThreatTier;

        clone.VirusSkillsUsedCount = VirusSkillsUsedCount;
        clone.InfectionRateDetected = InfectionRateDetected;
        clone.DeadRateDetected = DeadRateDetected;
        clone.VaccineProgress = VaccineProgress;

        clone.DetectedCells = new List<GridCell>(DetectedCells);
        clone.DetectedContagiousCells = new List<GridCell>(DetectedContagiousCells);
        clone.DetectedDeadCells = new List<GridCell>(DetectedDeadCells);
        clone.CriticalCells = new List<GridCell>(CriticalCells);
        clone.InfectedCells = new List<GridCell>(InfectedCells);
        clone.ExposedCells = new List<GridCell>(ExposedCells);
        clone.SafeCells = new List<GridCell>(SafeCells);
        clone.ImmuneCells = new List<GridCell>(ImmuneCells);

        clone.LockdownedCells = new List<GridCell>(LockdownedCells);

        return clone;
    }
}