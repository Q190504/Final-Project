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

        Vector2Int current = start;

        int w = grid.GetWidth();
        int h = grid.GetHeight();

        float baseSize = Mathf.Sqrt(w * w + h * h);

        float minLength = config.minRiverLengthPercent * baseSize;
        float maxLength = config.maxRiverLengthPercent * baseSize;

        int length = Mathf.FloorToInt(Mathf.Lerp(minLength, maxLength, (float)random.NextDouble()));
        riverData.targetLength = length;

        Vector2Int direction = Utility.GetRandom8Direction(random);

        for (int i = 0; i < length; i++)
        {
            if (!Utility.CheckIsValidPosForRiverOrLake(grid, current))
                break;

            float minRadius = config.minRiverRadiusPercent * baseSize;
            float maxRadius = config.maxRiverRadiusPercent * baseSize;

            int radius = Mathf.FloorToInt(Mathf.Lerp(minRadius, maxRadius, (float)random.NextDouble()));
            RiverSegment segment = new();
            Utility.StampRiver(grid, current, radius, segment);
            riverData.segments.Add(segment);

            direction = RandomizeDirection(direction, random);
            current += direction;
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
}
