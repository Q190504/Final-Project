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

            if (!cell.Stats.isBlocked
                && cell.Stats.population.type == PopulationType.Low
                && cell.Stats.structure.type == StructureType.None)
                candidates.Add(cell);
        }

        if (candidates.Count == 0)
            return;

        int index;
        for (int i = 0; i < config.startingInfectedCellCount; i++)
        {
            index = infectionCellRandom.Next(0, candidates.Count);
            CellStageData stageData = config.startingInfectionLevelsList[i];
            if (stageData)
                candidates[index].Stats.SetInfectionLevel(stageData.cellStageStats.minInfectionValue);
            else
            {
                candidates[index].Stats.SetInfectionLevel(1);
                Debug.Log($"Don't have enough stageData to spawn starting infected cell. " +
                    $"Starting infected cell: {config.startingInfectedCellCount}, stageData count {config.startingInfectionLevelsList.Count}");
            }
        }
    }
}
