using System;
using System.Collections.Generic;
using UnityEngine;

public class StructureSpawner : IMapGeneratorStep
{
    private readonly MapConfig config;
    private readonly int randomSeed;
    private float gridBaseSize = 0;
    Dictionary<StructureType, List<Vector2Int>> structurePositions = new();

    public StructureSpawner(MapConfig config, int randomSeed)
    {
        this.config = config;
        this.randomSeed = randomSeed;
    }

    public void Execute(Grid<GridCell> grid)
    {
        List<GridCell> candidates = new();
        System.Random structureSpawningRandom = new(randomSeed);
        gridBaseSize = MapManager.Instance.GetBaseSize();

        foreach (var cell in grid.GetGrid())
        {
            List<EnvironmentType> environmentsCantSpawnInList = new()
                { EnvironmentType.Water, EnvironmentType.Mountain };

            if (!Utility.IsEnviromentMatch(cell.Stats, environmentsCantSpawnInList))
                candidates.Add(cell);
        }

        if (candidates.Count == 0)
            return;

        Utility.Shuffle(candidates, structureSpawningRandom);

        SpawnStructures(grid, structureSpawningRandom, candidates);
    }

    private void SpawnStructures(
    Grid<GridCell> grid,
    System.Random random,
    List<GridCell> candidates)
    {
        float rangeBase = gridBaseSize;

        foreach (StructureDataSO structureData in CellPropertyManager.Instance.GetStructureDatas())
        {
            if (random.NextDouble() > structureData.spawnRate)
                continue;

            structurePositions[structureData.type] = new List<Vector2Int>();

            int structureCount = random.Next(1, structureData.maxAmountInAMatch + 1);
            int range = Mathf.FloorToInt(structureData.effectRangePercent * rangeBase);

            int spawned = 0;

            foreach (var cell in candidates)
            {
                if (spawned >= structureCount)
                    break;

                var stats = cell.Stats;

                // reject fast
                if (stats.structure.type != StructureType.None)
                    continue;

                if (stats.affectedByStructures.Contains(structureData.type))
                    continue;

                var envType = stats.environment.currentEnvironmentType;

                bool envValid = false;

                foreach (var envData in structureData.spawnEnvironmentAndRateList)
                {
                    if (envData.environmentTypeCanSpawn != envType)
                        continue;

                    if (random.NextDouble() <= envData.spawnRateOnEnvironment)
                        envValid = true;

                    break;
                }

                if (!envValid)
                    continue;

                if (HasStructureInRange(cell.X, cell.Y, range, structureData.type))
                    continue;

                stats.SetStructure(structureData.type, cell);
                structurePositions[structureData.type].Add(new Vector2Int(cell.X, cell.Y));
                //Debug.Log("Spawned structure " + structureData.type + " at (" + cell.X + ", " + cell.Y + ")");

                spawned++;
            }
        }
    }

    private bool HasStructureInRange(
        int x,
        int y,
        int range,
        StructureType type)
    {
        var list = structurePositions[type];

        int rangeSq = range * range;

        foreach (var pos in list)
        {
            int dx = pos.x - x;
            int dy = pos.y - y;

            if (dx * dx + dy * dy <= rangeSq)
                return true;
        }

        return false;
    }
}
