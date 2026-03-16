using System;

public interface IMapGeneratorStep
{
    void Execute(Grid<GridCell> grid);
}
