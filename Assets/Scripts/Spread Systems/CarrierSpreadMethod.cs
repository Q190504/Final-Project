using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CarrierSpreadMethod : BaseSpreadMethod<CarrierSpreadDataSO>
{
    private CarrierExtraConfig extra;

    private List<Vector2Int> cachedOffsets;

    public CarrierSpreadMethod(Grid<GridCell> grid, CarrierSpreadDataSO data)
           : base(grid, data)
    {
        extra = data.extraConfig;
        BuildOffsets();
    }

    public override void SetUpStatsAfterUpgrade()
    {

    }

    public override SpreadResult Execute()
    {
        var sources = grid.GetInfectedSnapshot();
        
        HashSet<Vector2Int> globallySelected = new();
        MinHeapWithSize candidateHeap = new(extra.speardCellCount);

        foreach (GridCell source in sources)
        {
            if (source == null) continue;

            float chance = extra.baseCarrierSpawnChance * source.Stats.population.weight;
            if (Random.value > chance) continue;

            source.Stats.SetCarrier(true);
            mapManager.AddCellNeedToUpdateVisual(new Vector2Int(source.X, source.Y));

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
                        && config.spreadChanceWhenBlocked > 0
                        && Random.value < config.spreadChanceWhenBlocked)
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
                float weight = candidate.Stats.population.weight;

                if (candidate.Stats.structure.type != StructureType.None)
                    weight += extra.structurePriority;

                if (weight <= 0) continue;

                // --- A-Res key ---
                float u = Random.value;
                float key = Mathf.Pow(u, 1f / weight);

                candidateHeap.Push((candidate, key));
            }

            List<(GridCell cell, float key)> selected = candidateHeap.GetItems();
            selected.Sort((a, b) => b.key.CompareTo(a.key));

            float originBonus = 1 + source.GetStageInfectionIncreasePercent();

            foreach ((GridCell cell, float key) in selected)
            {
                Vector2Int pos = new(cell.X, cell.Y);

                if (globallySelected.Contains(pos))
                    continue;

                globallySelected.Add(pos);

                int power = GetInfectionPowerOfMethod(cell);

                int increaseInfectionLevel =
                    Mathf.RoundToInt(power * originBonus)
                    - cell.Stats.currentInfectionResistance;

                if (increaseInfectionLevel <= 0)
                    continue;

                // --- Delay event ---
                timeManager.ScheduleEvent(extra.travelTime, () =>
                {
                    ClearCarrier(source);
                }, config.eventPriority);

                timeManager.ScheduleEvent(extra.travelTime, () =>
                {
                    CreateCarrier(cell, increaseInfectionLevel);
                }, config.eventPriority);
            }

            candidateHeap.Clear();
        }

        return new SpreadResult();
    }

    private void ClearCarrier(GridCell cell)
    {
        cell.Stats.SetCarrier(false);
        mapManager.AddCellNeedToUpdateVisual(new Vector2Int(cell.X, cell.Y));
    }

    private void CreateCarrier(GridCell cell, int increaseInfection)
    {
        cell.Stats.UpdateInfectionLevel(increaseInfection);
        mapManager.AddCellNeedToUpdateVisual(new Vector2Int(cell.X, cell.Y));
    }

    private void BuildOffsets()
    {
        cachedOffsets = new();

        int r = config.maxDistance;

        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y > r * r || (x == 0 && y == 0))
                    continue;

                cachedOffsets.Add(new Vector2Int(x, y));
            }
        }
    }
}