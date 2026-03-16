using UnityEngine;

public class CellPresenter
{
    private GridCell cell;
    private GridCellVisual view;

    public CellPresenter(GridCell cell, GridCellVisual view)
    {
        this.cell = cell;
        this.view = view;
    }

    public void Refresh()
    {
        view.UpdateVisual(cell);
    }
}

