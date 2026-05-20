using UnityEngine;

public class CellPresenter
{
    private GridCell cell;
    private GridCellVisual view;
    private HumanActionVisualizerOnCell humanActionVisualizer;

    public CellPresenter(GridCell cell, GridCellVisual view)
    {
        this.cell = cell;
        this.view = view;
        this.humanActionVisualizer = view.GetComponent<HumanActionVisualizerOnCell>();
    }

    public void Refresh()
    {
        view.UpdateVisual(cell);
    }

    public void SetCellFocusVFXVisibility(bool state)
    {
        view.SetCellFocusVFXVisibility(state);
    }

    public void PlayHumanActionVisual(HumanActionVisualPreset preset)
    {
        humanActionVisualizer.PlayVisual(preset);
    }
}

