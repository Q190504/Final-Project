using System.Collections.Generic;
using UnityEngine;

public class MapGenerator
{
    private List<IMapGeneratorStep> steps;

    public MapGenerator(int seed, MapConfig config)
    {
        steps = new List<IMapGeneratorStep>
        {
            new TemperatureGenerator(config, seed + 1000),
            new PopulationGenerator(config, seed + 2000),
            new MountainGenerator(config, seed + 3000),
            new LakeGenerator(config, seed + 4000),
            new RiverGenerator(config, seed + 5000),
            new UrbanGenerator(config, seed + 6000),
            new MapBalancer(config, seed),
            new EnvironmentDerivationStep(),
            new StructureSpawner(config, seed + 7000),
            new InfectionSeeder(config, seed + 8000)
        };
    }

    public Grid<GridCell> Generate(Grid<GridCell> grid)
    {
        foreach (IMapGeneratorStep step in steps)
            step.Execute(grid);

        return grid;
    }
}
