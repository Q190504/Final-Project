using UnityEngine;

public class EnvironmentDerivationStep : IMapGeneratorStep
{
    public void Execute(Grid<GridCell> grid)
    {
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                GridCell cell = grid.GetCell(x, y);

                cell.Stats.hasWater = grid.GetWaterGrid()[x, y];

                if (cell.Stats.hasWater)
                {
                    EnvironmentData environmentData = CellPropertyManager.Instance
                 .GetEnvironmentData(EnvironmentType.Water);

                    cell.Stats.SetStats(environmentData.populationType, environmentData.tempuratureType, EnvironmentType.Water, cell);

                    continue;
                }

                if (grid.GetMountainGrid()[x, y])
                {
                    EnvironmentData environmentData = CellPropertyManager.Instance
                    .GetEnvironmentData(EnvironmentType.Mountain);

                    cell.Stats.SetStats(environmentData.populationType, environmentData.tempuratureType, EnvironmentType.Mountain, cell);

                    continue;
                }

                cell.Stats.SetStats(grid.GetPopulationGrid()[x, y], grid.GetTemperatureGrid()[x, y], cell);
            }
        }
    }
}
