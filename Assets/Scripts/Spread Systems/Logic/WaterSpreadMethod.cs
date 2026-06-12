using System.Collections.Generic;
using UnityEngine;

public class WaterSpreadMethod : BaseSpreadMethod<WaterSpreadDataSO, WaterRuntimeData>
{
    private Queue<(Vector2Int pos, int dist)> queue = new();
    private HashSet<Vector2Int> visited = new();

    public WaterSpreadMethod(SpreadMethodContext context, WaterSpreadDataSO data, WaterRuntimeData runtimeData)
        : base(context, data, runtimeData)
    {
    }

    public override SpreadResult Execute()
    {
        SpreadResult result = new();

        foreach (var source in context.Grid.GetInfectedSnapshot())
        {
            Vector2Int origin = new(source.X, source.Y);
            Spread(origin, result);
        }

        return result;
    }

    private void Spread(Vector2Int origin, SpreadResult result)
    {
        WaterExtraConfig extraConfig = runtimeData.extraConfig;
        Grid<GridCell> grid = context.Grid;

        GridCell originCell = grid.GetCell(origin.x, origin.y);
        if (originCell == null)
        {
            Debug.LogError("originCell is null");
            return;
        }

        queue.Clear();
        visited.Clear();

        queue.Enqueue((origin, 0));
        visited.Add(origin);

        while (queue.Count > 0)
        {
            var (current, dist) = queue.Dequeue();

            GridCell currentTagetCell = grid.GetCell(current.x, current.y);

            if (currentTagetCell == null)
                continue;

            if (current != origin)
            {
                int increaseInfectionLevel = GetInfectionPowerOfMethod(originCell, currentTagetCell);
                if (increaseInfectionLevel > 0)
                {
                    InfectionInfo cellDelta = new(currentTagetCell, increaseInfectionLevel, extraConfig.sterilizationResistanceModifierCellHasWater);
                    result.Add(currentTagetCell, cellDelta);
                }
            }

            int newDist = dist + 1;
            if (newDist < runtimeData.GetFinalMinDistance() - 1 || newDist > runtimeData.GetFinalMaxDistance())
                continue;

            foreach (Vector2Int dir in Utility.Neighbor8Directions)
            {
                Vector2Int next = current + dir;

                if (!grid.IsInBounds(next.x, next.y))
                    continue;

                if (visited.Contains(next))
                    continue;

                GridCell cell = grid.GetCell(next.x, next.y);

                if (!cell.Stats.HasWater())
                {
                    if (extraConfig.chanceToSpreadToNonWaterCell > 0)
                    {
                        float random = Random.Range(0f, 1f);

                        if (random > extraConfig.chanceToSpreadToNonWaterCell)
                            continue;
                    }
                    else
                        continue;
                }

                if (!cell.CanBeInfected()) continue;

                if (cell.Stats.isBlocked)
                {
                    if (runtimeData.GetFinalSpreadChanceWhenBlocked() <= 0
                    || Random.value >= runtimeData.GetFinalSpreadChanceWhenBlocked())
                        continue;
                }

                visited.Add(next);
                queue.Enqueue((next, newDist));
            }
        }
    }
}