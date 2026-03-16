using UnityEngine;

/// <summary>
/// Normalizes the population grid so that the maximum value becomes 1.
/// Keeps relative distribution but scales everything into [0.1, 1].
/// </summary>
public class PopulationNormalizer : IMapGeneratorStep
{
    public void Execute(Grid<GridCell> grid)
    {
        float[,] map = grid.GetPopulationGrid();
        int w = grid.GetWidth();
        int h = grid.GetHeight();
        float max = 0f;

        // Find maximum value in grid
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (map[x, y] > max)
                    max = map[x, y];
            }
        }

        // Avoid division by zero
        if (max <= 0f) return;

        // Normalize and clamp
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                float normalized = map[x, y] / max;
                map[x, y] = Mathf.Clamp(normalized, 0.1f, 1f);
            }
        }
    }
}