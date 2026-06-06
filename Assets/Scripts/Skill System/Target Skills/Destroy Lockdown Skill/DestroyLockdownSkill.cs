using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestroyLockdownSkill
    : MultiTargetSkill<DestroyLockdownSkillDataSO, DestroyLockdownSkillRuntimeData>
{
    public DestroyLockdownSkill(DestroyLockdownSkillDataSO data,
        DestroyLockdownSkillRuntimeData runtimeData)
        : base(data, runtimeData)
    {
    }

    public override void Execute(List<GridCell> targets)
    {
        foreach (GridCell cell in targets)
        {
            if (cell.Stats.isLockdown)
            {
                cell.Stats.SetIsLockDown(false);
            }
        }

        base.Execute(targets);
    }

    public override List<GridCell> GetTargets(GridCell centerCell)
    {
        if (centerCell == null) return null;

        MultiTargetSkillExtraConfig targetSkillExtraConfig = RuntimeData.multiTargetSkillExtraConfig;

        Grid<GridCell> grid = MapManager.Instance.GetGrid();
        int targetRadius = targetSkillExtraConfig.targetRadius;

        List<GridCell> targets = grid.GetNeighborsInRange(centerCell, targetRadius);
        targets.Add(centerCell);

        return targets;
    }

    public override bool IsValidTargets(List<GridCell> targets)
    {
        return targets.Any(c => c.Stats.isLockdown);
    }
}
