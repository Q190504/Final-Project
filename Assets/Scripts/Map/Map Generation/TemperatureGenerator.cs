using UnityEngine;

public class TemperatureGenerator : IMapGeneratorStep
{
    private readonly MapConfig config;
    private readonly int randomSeed;

    private int noiseOffsetMin = -100000;
    private int noiseOffsetMax = 100000;

    private float secondaryFrequencyMultiplier = 2f;

    private float baseNoiseWeight = 0.7f;
    private float secondaryNoiseWeight = 0.3f;

    public TemperatureGenerator(MapConfig config, int seed)
    {
        this.config = config;
        this.randomSeed = seed;
    }

    public void Execute(Grid<GridCell> grid)
    {
        int width = grid.GetWidth();
        int height = grid.GetHeight();

        System.Random temperatureRandom = new(randomSeed);

        float baseOffsetX = temperatureRandom.Next(noiseOffsetMin, noiseOffsetMax);
        float baseOffsetY = temperatureRandom.Next(noiseOffsetMin, noiseOffsetMax);

        float secondaryOffsetX = temperatureRandom.Next(noiseOffsetMin, noiseOffsetMax);
        float secondaryOffsetY = temperatureRandom.Next(noiseOffsetMin, noiseOffsetMax);

        float baseScale = config.temperatureNoiseScale;
        float secondaryScale = config.temperatureNoiseScale * secondaryFrequencyMultiplier;

        float[,] temperatures = new float[width, height];

        float minTemperature = float.MaxValue;
        float maxTemperature = float.MinValue;

        // Pass 1: Generate temperatures and find min/max
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float baseSampleX = (x + baseOffsetX) * baseScale;
                float baseSampleY = (y + baseOffsetY) * baseScale;

                float baseNoise = Mathf.PerlinNoise(baseSampleX, baseSampleY);

                float secondarySampleX = (x + secondaryOffsetX) * secondaryScale;
                float secondarySampleY = (y + secondaryOffsetY) * secondaryScale;

                float secondaryNoise = Mathf.PerlinNoise(secondarySampleX, secondarySampleY);

                float temperature =
                    baseNoiseWeight * baseNoise +
                    secondaryNoiseWeight * secondaryNoise;

                temperatures[x, y] = temperature;

                if (temperature < minTemperature)
                    minTemperature = temperature;

                if (temperature > maxTemperature)
                    maxTemperature = temperature;
            }
        }

        // Avoid divide by zero
        if (Mathf.Approximately(minTemperature, maxTemperature))
        {
            maxTemperature = minTemperature + 0.0001f;
        }

        // Pass 2: Normalize to [0,1]
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float normalizedTemperature =
                    Mathf.InverseLerp(
                        minTemperature,
                        maxTemperature,
                        temperatures[x, y]);

                grid.SetTemperatureCell(x, y, normalizedTemperature);
            }
        }
    }
}