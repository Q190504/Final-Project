using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IncreaseInfectionLevelToCellsSkill
    : MultiTargetSkill<IncreaseInfectionLevelToCellsSkillDataSO, IncreaseInfectionLevelToCellsSkillRuntimeData>
{
    public IncreaseInfectionLevelToCellsSkill(IncreaseInfectionLevelToCellsSkillDataSO data,
        IncreaseInfectionLevelToCellsSkillRuntimeData runtimeData)
        : base(data, runtimeData)
    {
    }

    public override void Execute(List<GridCell> targets)
    {
        IncreaseInfectionLevelToCellsSkillExtraConfig skillExtraConfig = RuntimeData.skillExtraConfig;

        float percent = skillExtraConfig.increaseInfectionLevelPercentForEachCell;

        foreach (GridCell cell in targets)
        {
            if (IsValidStage(cell.Stats.stage.type))
            {
                int currentInfectionLevel = cell.Stats.infectionLevel;
                int increaseInfectionLevel = Mathf.RoundToInt(currentInfectionLevel * percent);
                cell.Stats.UpdateInfectionLevel(increaseInfectionLevel);
            }
        }

        base.Execute(targets);
    }

    public override List<GridCell> GetTargets(GridCell centerCell)
    {
        if (centerCell == null) return null;

        MultiTargetSkillExtraConfig multiTargetSkillExtraConfig = RuntimeData.multiTargetSkillExtraConfig;
        Grid<GridCell> grid = MapManager.Instance.GetGrid();
        int targetRadius = multiTargetSkillExtraConfig.targetRadius;

        List<GridCell> targets = new() { centerCell };
        targets.AddRange(grid.GetNeighborsInRange(centerCell, targetRadius));

        return targets;
    }

    public override bool IsValidTargets(List<GridCell> targets)
    {
        return targets.Any(c => IsValidStage(c.Stats.stage.type));
    }

    private bool IsValidStage(CellStageType stage)
    {
        return stage == CellStageType.Exposed
            || stage == CellStageType.Infected;
    }
}
