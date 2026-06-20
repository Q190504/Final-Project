using System.Collections.Generic;
using UnityEngine;

public class RiverGenerator : IMapGeneratorStep
{
    private readonly MapConfig config;
    private readonly int randomSeed;

    public RiverGenerator(MapConfig config, int seed)
    {
        this.config = config;
        this.randomSeed = seed;
    }

    public void Execute(Grid<GridCell> grid)
    {
        System.Random riverRandom = new(randomSeed);

        int riverCount = riverRandom.Next(config.minRiverCount, config.maxRiverCount + 1);

        if (riverCount > 0)
        {
            bool found = false;
            foreach (GridCell cell in grid.GetGrid())
            {
                if (Utility.CheckIsValidPosForRiverOrLake(grid, new Vector2Int(cell.X, cell.Y)))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Debug.LogError("Can't find valid pos for river");
                return;
            }
        }
        else return;

        for (int i = 0; i < riverCount; i++)
        {
            Vector2Int start;
            do
            {
                start = Utility.GetRandomPosition(grid, riverRandom);
            }
            while (!Utility.CheckIsValidPosForRiverOrLake(grid, start));

            grid.AddRiverData(GenerateRiver(grid, start, riverRandom));
        }
    }

    private RiverData GenerateRiver(Grid<GridCell> grid, Vector2Int start, System.Random random)
    {
        RiverData riverData = new();

        int w = grid.GetWidth();
        int h = grid.GetHeight();

        float baseSize = Mathf.Sqrt(w * w + h * h);

        float minLength = config.minRiverLengthPercent * baseSize;
        float maxLength = config.maxRiverLengthPercent * baseSize;

        int length = Mathf.FloorToInt(Mathf.Lerp(minLength, maxLength, (float)random.NextDouble()));
        riverData.targetLength = length;

        // First segment
        int radius = GetRandomRadius(baseSize, random);
        RiverSegment firstSegment = new();
        Utility.StampRiver(grid, start, radius, firstSegment);
        riverData.segments.Add(firstSegment);

        length--;

        Vector2Int current = start;
        Vector2Int preferredDirection = Utility.GetRandom8Direction(random);

        for (int i = 0; i < length; i++)
        {
            if (i > 0) preferredDirection = RandomizeDirection(preferredDirection, random);

            List<Vector2Int> directions = new() { preferredDirection };

            foreach (Vector2Int dir in Utility.Neighbor8Directions)
            {
                if (dir != preferredDirection)
                    directions.Add(dir);
            }

            // Shuffle sub directions
            for (int j = 1; j < directions.Count; j++)
            {
                int swapIndex = random.Next(j, directions.Count);

                (directions[j], directions[swapIndex]) = (directions[swapIndex], directions[j]);
            }

            bool foundValid = false;
            foreach (Vector2Int dir in directions)
            {
                Vector2Int candidatePos = current + dir;

                if (!Utility.CheckIsValidPosForRiverOrLake(grid, candidatePos))
                    continue;

                current = candidatePos;
                preferredDirection = dir;
                foundValid = true;
                break;
            }

            if (!foundValid)
                break;

            RiverSegment segment = new();
            radius = GetRandomRadius(baseSize, random);
            Utility.StampRiver(grid, current, radius, segment);
            riverData.segments.Add(segment);
        }

        return riverData;
    }

    private Vector2Int RandomizeDirection(Vector2Int currentDir, System.Random random)
    {
        int roll = random.Next(100);

        if (roll < 65)
            return currentDir;

        if (roll < 80)
            return new Vector2Int(-currentDir.y, currentDir.x);

        if (roll < 95)
            return new Vector2Int(currentDir.y, -currentDir.x);

        return Utility.GetRandomCardinalDirection(random);
    }

    private int GetRandomRadius(float baseSize, System.Random mountainRandom)
    {
        float minRadius = config.minRiverRadiusPercent * baseSize;
        float maxRadius = config.maxRiverRadiusPercent * baseSize;
        return Mathf.FloorToInt(Mathf.Lerp(minRadius, maxRadius, (float)mountainRandom.NextDouble()));
    }
}
