using System.Collections.Generic;
using UnityEngine;

public class SurfaceSpreadMethod : BaseSpreadMethod<SurfaceSpreadDataSO>
{
    private Queue<(Vector2Int pos, int dist)> queue = new();
    private HashSet<Vector2Int> visited = new();

    public SurfaceSpreadMethod(SpreadMethodContext context, SurfaceSpreadDataSO data) : base(context, data)
    {
    }

    public override SpreadResult Execute()
    {
        SpreadResult result = new();

        List<GridCell> sources = context.Grid.GetInfectedSnapshot();

        foreach (var source in sources)
        {
            Vector2Int origin = new(source.X, source.Y);
            Spread(origin, result);
        }

        return result;
    }

    private void Spread(Vector2Int origin, SpreadResult result)
    {
        SurfaceRuntimeData surfaceRuntime = runtimeData as SurfaceRuntimeData;
        SurfaceExtraConfig surfacExtraConfig = surfaceRuntime.extraConfig;

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

            GridCell currentTargetCell = grid.GetCell(current.x, current.y);

            if (currentTargetCell == null)
                continue;

            if (current != origin)
            {
                if (currentTargetCell.Stats.environment.currentEnvironmentType == EnvironmentType.Mountain)
                    continue;

                int increaseInfectionLevel = GetInfectionPowerOfMethod(originCell, currentTargetCell);
                if (increaseInfectionLevel > 0)
                {
                    InfectionInfo cellDelta = new(currentTargetCell, increaseInfectionLevel, surfacExtraConfig.sterilizationImmunityTicks,
                       surfacExtraConfig.minTickToBonusSterilizationResistancePercent,
                       surfacExtraConfig.sterilizationResistanceModifierIfInfectedForALongTime);

                    result.Add(currentTargetCell, cellDelta);
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

                if (cell.Stats.HasWater()) continue;
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