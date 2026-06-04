using System.Collections.Generic;
using UnityEngine;

public class CarrierSpreadMethod : BaseSpreadMethod<CarrierSpreadDataSO, CarrierRuntimeData>
{
    private List<Vector2Int> cachedOffsets;

    public CarrierSpreadMethod(SpreadMethodContext context, CarrierSpreadDataSO data, CarrierRuntimeData runtimeData) 
        : base(context, data, runtimeData)
    {
        BuildOffsets();
    }

    public override SpreadResult Execute()
    {
        Grid<GridCell> grid = context.MapManager.GetGrid();
        TimeManager timeManager = context.TimeManager;

        var sources = grid.GetInfectedSnapshot();
        CarrierExtraConfig extraConfig = runtimeData.extraConfig;

        HashSet<Vector2Int> globallySelected = new();
        MinHeapWithSize candidateHeap = new(extraConfig.targetCellCountForEachOriginCell);

        foreach (GridCell source in sources)
        {
            if (source == null) continue;

            if (!source.Stats.canHasCarrier) continue;

            float chance = (extraConfig.baseCarrierSpawnChance * source.Stats.population.weight) + source.Stats.additionalCarrierSpreadChancePercent;
            if (Random.value > chance) continue;

            List<(GridCell cell, float key)> candidates = new();

            foreach (var offset in cachedOffsets)
            {
                // Skip if already selected by other source to prevent multiple sources infecting the same target
                Vector2Int pos = new(source.X + offset.x, source.Y + offset.y);

                if (globallySelected.Contains(pos)) continue;

                GridCell candidate = grid.GetCell(source.X + offset.x, source.Y + offset.y);
                if (candidate == null) continue;

                bool valid = false;

                // --- Check valid ---
                if (candidate.Stats.isBlocked)
                {
                    if (candidate.Stats.environment.currentEnvironmentType != EnvironmentType.Mountain
                        && runtimeData.GetFinalSpreadChanceWhenBlocked() > 0
                        && Random.value < runtimeData.GetFinalSpreadChanceWhenBlocked())
                    {
                        valid = true;
                    }
                }
                else if (candidate.CanBeInfected())
                {
                    valid = true;
                }

                if (!valid) continue;

                // --- Weight ---
                CellStats candidateStats = candidate.Stats;
                float weight = candidateStats.population.weight;

                if (candidateStats.structure.type != StructureType.None)
                    weight += extraConfig.structurePriority;

                if (candidateStats.stage.type == CellStageType.Safe)
                    weight += extraConfig.weightBonusForSafeCells;

                if (weight <= 0) continue;

                // --- A-Res key ---
                float u = Random.value;
                float key = Mathf.Pow(u, 1f / weight);

                candidateHeap.Push((candidate, key));
            }

            List<(GridCell cell, float key)> selected = candidateHeap.GetItems();
            selected.Sort((a, b) => b.key.CompareTo(a.key));

            bool thisSourceInfectedAny = false;

            foreach ((GridCell targetCell, float key) in selected)
            {
                Vector2Int pos = new(targetCell.X, targetCell.Y);

                if (globallySelected.Contains(pos))
                    continue;

                globallySelected.Add(pos);

                int increaseInfectionLevel = GetInfectionPowerOfMethod(source, targetCell);
                if (increaseInfectionLevel <= 0)
                    continue;

                // --- Delay event ---
                timeManager.ScheduleEvent(extraConfig.travelTime, () =>
                {
                    ClearCarrierSource(source);
                }, data.baseConfig.eventPriority);

                timeManager.ScheduleEvent(extraConfig.travelTime, () =>
                {
                    CreateCarrierInfection(targetCell, increaseInfectionLevel);
                }, data.baseConfig.eventPriority);

                thisSourceInfectedAny = true;
            }

            if (thisSourceInfectedAny)
            {
                source.Stats.SetCarrier(true);
                context.MapManager.AddCellNeedToUpdateVisual(new Vector2Int(source.X, source.Y));
            }

            candidateHeap.Clear();
        }

        return new SpreadResult();
    }

    private void ClearCarrierSource(GridCell cell)
    {
        cell.Stats.SetCarrier(false);
        context.MapManager.AddCellNeedToUpdateVisual(new Vector2Int(cell.X, cell.Y));
    }

    private void CreateCarrierInfection(GridCell cell, int increaseInfection)
    {
        cell.Stats.UpdateInfectionLevel(increaseInfection);
        context.MapManager.AddCellNeedToUpdateVisual(new Vector2Int(cell.X, cell.Y));
    }

    private void BuildOffsets()
    {
        cachedOffsets = new();

        int maxDistance = runtimeData.GetFinalMaxDistance();
        int minDistance = runtimeData.GetFinalMinDistance();

        for (int x = -maxDistance; x <= maxDistance; x++)
        {
            for (int y = -maxDistance; y <= maxDistance; y++)
            {
                if (x * x + y * y > maxDistance * maxDistance 
                    || x * x + y * y < minDistance * minDistance 
                    || (x == 0 && y == 0))
                    continue;

                cachedOffsets.Add(new Vector2Int(x, y));
            }
        }
    }
}