using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IncreaseInfectionLevelToCellsSkill
    : TargetSkill<IncreaseInfectionLevelToCellsSkillDataSO, IncreaseInfectionLevelToCellsSkillRuntimeData>
{
    public IncreaseInfectionLevelToCellsSkill(IncreaseInfectionLevelToCellsSkillDataSO data,
        IncreaseInfectionLevelToCellsSkillRuntimeData runtimeData)
        : base(data, runtimeData)
    {
    }

    public override void Execute(GridCell centerCell)
    {
        TargetSkillExtraConfig targetSkillExtraConfig = RuntimeData.targetSkillExtraConfig;
        IncreaseInfectionLevelToCellsSkillExtraConfig skillExtraConfig = RuntimeData.skillExtraConfig;

        float percent = skillExtraConfig.increaseInfectionLevelPercentForEachCell;
        Grid<GridCell> grid = MapManager.Instance.GetGrid();
        int targetRadius = targetSkillExtraConfig.targetRadius;

        List<GridCell> targets = grid.GetNeighborsInRange(centerCell, targetRadius);
        targets.Add(centerCell);

        foreach (GridCell cell in targets)
        {
            if (IsValidStage(cell.Stats.stage.type))
            {
                int currentInfectionLevel = cell.Stats.infectionLevel;
                int increaseInfectionLevel = Mathf.RoundToInt(currentInfectionLevel * percent);
                cell.Stats.UpdateInfectionLevel(increaseInfectionLevel);
            }
        }

        base.Execute(centerCell);
    }

    public override bool IsValidTarget(GridCell centerCell)
    {
        if (centerCell == null) return false;

        TargetSkillExtraConfig targetSkillExtraConfig = RuntimeData.targetSkillExtraConfig;
        Grid<GridCell> grid = MapManager.Instance.GetGrid();
        int targetRadius = targetSkillExtraConfig.targetRadius;

        List<GridCell> targets = grid.GetNeighborsInRange(centerCell, targetRadius);

        return targets.Any(c => IsValidStage(c.Stats.stage.type));
    }

    private bool IsValidStage(CellStageType stage)
    {
        return stage == CellStageType.Exposed
            || stage == CellStageType.Infected;
    }
}
