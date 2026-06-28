using UnityEngine;

public class CellPresenter
{
    private GridCell cell;
    private GridCellVisual view;
    private SpreadMethodTargetVisualizerOnCell spreadMethodTargetVisualizer;
    private HumanActionVisualizerOnCell humanActionVisualizer;

    public CellPresenter(GridCell cell, GridCellVisual view)
    {
        this.cell = cell;
        this.view = view;
        this.spreadMethodTargetVisualizer = view.GetComponent<SpreadMethodTargetVisualizerOnCell>();
        this.humanActionVisualizer = view.GetComponent<HumanActionVisualizerOnCell>();
    }

    public void Refresh()
    {
        view.UpdateVisual(cell);
    }

    public void SetInspectingVFXVisibility(bool state)
    {
        view.SetInspectingVFXVisibility(state);
    }

    public void SetStartingCellVFXVisibility(bool state)
    {
        view.SetStartingCellFocusVFXVisibility(state);
    }

    public void SetCellTargetedBySkillOverlayVisibility(bool state)
    {
        view.SetAffectedBySkillOverlayVisibility(state);
    }

    public void SetCellAffectedByStructureOverlayColor(bool state)
    {
        view.SetAffectedByStructureOverlayVisibility(state);
    }

    public void PlayHumanActionVisual(HumanActionVisualPreset preset)
    {
        humanActionVisualizer.PlayVisual(preset);
    }

    public void PlaySpreadMethodTargetVisual(SpreadMethodTargetVisualizationPreset preset)
    {
        spreadMethodTargetVisualizer.PlayVisual(preset);
    }
}

