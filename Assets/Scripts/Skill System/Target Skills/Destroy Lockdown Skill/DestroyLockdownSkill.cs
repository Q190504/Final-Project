using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestroyLockdownSkill
    : TargetSkill<DestroyLockdownSkillDataSO, DestroyLockdownSkillRuntimeData>
{
    public DestroyLockdownSkill(DestroyLockdownSkillDataSO data,
        DestroyLockdownSkillRuntimeData runtimeData)
        : base(data, runtimeData)
    {
    }

    public override void Execute(GridCell centerCell)
    {
        TargetSkillExtraConfig targetSkillExtraConfig = RuntimeData.targetSkillExtraConfig;

        Grid<GridCell> grid = MapManager.Instance.GetGrid();
        int targetRadius = targetSkillExtraConfig.targetRadius;

        List<GridCell> targets = grid.GetNeighborsInRange(centerCell, targetRadius);
        targets.Add(centerCell);

        foreach (GridCell cell in targets)
        {
            if (cell.Stats.isLockdown)
            {
                cell.Stats.SetIsLockDown(false);
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

        return targets.Any(c => c.Stats.isLockdown);
    }
}
