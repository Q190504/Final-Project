using UnityEngine;
using System.Collections.Generic;

public class InfectionSeeder : IMapGeneratorStep
{
    private readonly MapConfig config;
    private readonly int randomSeed;

    public InfectionSeeder(MapConfig config, int seed)
    {
        this.config = config;
        this.randomSeed = seed;
    }

    public void Execute(Grid<GridCell> grid)
    {
        List<GridCell> candidates = new();
        System.Random infectionCellRandom = new(randomSeed);

        foreach (var cell in grid.GetGrid())
        {
            cell.Stats.SetInfectionLevel(0);

            if (cell.Stats.environment.currentEnvironmentType != EnvironmentType.Mountain
                && cell.Stats.environment.currentEnvironmentType != EnvironmentType.Water
                && cell.Stats.structure.type == StructureType.None)
                candidates.Add(cell);
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("No candidate for first infected cell!");
            return;
        }

        List<GridCell> bestCandidates = new();
        float bestScore = float.MinValue;

        foreach (GridCell candidate in candidates)
        {
            float score = CalculateScore(candidate, grid);

            candidate.startingScore = score;

            if (score > bestScore)
            {
                bestScore = score;
                bestCandidates.Clear();
                bestCandidates.Add(candidate);
            }
            else if (Mathf.Approximately(score, bestScore))
            {
                bestCandidates.Add(candidate);
            }
        }

        for (int i = 0; i < config.startingInfectedCellCount; i++)
        {
            GridCell selectedCell = bestCandidates[infectionCellRandom.Next(bestCandidates.Count)];
            CellStageData stageData = config.startingInfectionLevelsList[i];
            if (stageData)
                selectedCell.Stats.SetInfectionLevel(stageData.cellStageStats.minInfectionValue);
            else
            {
                selectedCell.Stats.SetInfectionLevel(1);
                Debug.Log($"Don't have enough stageData to spawn starting infected cell. " +
                    $"Starting infected cell: {config.startingInfectedCellCount}, stageData count {config.startingInfectionLevelsList.Count}");
            }

            if (i == 0)
            {
                grid.startingCell = selectedCell;
            }
        }
    }

    private float CalculateScore(GridCell cell, Grid<GridCell> grid)
    {
        float score = 0;

        cell.distanceToNearestUrban = grid.distanceToNearestUrbanMap[cell.X, cell.Y];
        score += grid.distanceToNearestUrbanMap[cell.X, cell.Y] * 10;

        cell.distanceToNearestWaterRegion = grid.distanceToNearestWaterRegionMap[cell.X, cell.Y];
        score += grid.distanceToNearestWaterRegionMap[cell.X, cell.Y] * 5;

        EnvironmentRegion nearestWaterRegion = grid.nearestWaterRegionMap[cell.X, cell.Y];
        cell.nearestWaterRegionAssistScore = nearestWaterRegion.AssistScore;
        score -= nearestWaterRegion.AssistScore;

        return score;
    }
}
