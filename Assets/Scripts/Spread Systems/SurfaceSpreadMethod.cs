using System.Collections.Generic;
using UnityEngine;

public class SurfaceSpreadMethod : BaseSpreadMethod<SurfaceSpreadDataSO>
{
    private SurfaceExtraConfig extra;

    private Queue<(Vector2Int pos, int dist)> queue = new();
    private HashSet<Vector2Int> visited = new();

    public SurfaceSpreadMethod(Grid<GridCell> grid, SurfaceSpreadDataSO data)
        : base(grid, data)
    {
        extra = data.extraConfig;
    }

    public override void SetUpStatsAfterUpgrade()
    {

    }

    public override SpreadResult Execute()
    {
        SpreadResult result = new();

        List<GridCell> sources = grid.GetInfectedSnapshot();

        foreach (var source in sources)
        {
            Vector2Int origin = new(source.X, source.Y);
            Spread(origin, result);
        }

        return result;
    }

    private void Spread(Vector2Int origin, SpreadResult result)
    {
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

        float originBonus = 1 + originCell.GetStageInfectionIncreasePercent();
        while (queue.Count > 0)
        {
            var (current, dist) = queue.Dequeue();

            GridCell currentCell = grid.GetCell(current.x, current.y);

            if (currentCell == null)
                continue;

            if (current != origin)
            {
                if (currentCell.Stats.environment.currentEnvironmentType == EnvironmentType.Mountain)
                    continue;

                int power = GetInfectionPowerOfMethod(currentCell);

                int increaseInfectionLevel =
                    Mathf.RoundToInt(power * originBonus)
                    - currentCell.Stats.currentInfectionResistance;

                if (increaseInfectionLevel > 0)
                {
                    CellDelta cellDelta = new()
                    {
                        cell = currentCell,
                        infectionDelta = increaseInfectionLevel,
                    };

                    result.Add(currentCell, cellDelta);
                }
            }

            int newDist = dist + 1;
            if (newDist < config.minDistance - 1 || newDist > config.maxDistance)
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
                    if (config.spreadChanceWhenBlocked <= 0
                    || Random.value >= config.spreadChanceWhenBlocked)
                        continue;
                }

                visited.Add(next);
                queue.Enqueue((next, newDist));
            }
        }
    }
}