using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DisableStructureSkill
    : SingleTargetSkill<DisableStructureSkillDataSO, DisableStructureSkillRuntimeData>
{
    public DisableStructureSkill(DisableStructureSkillDataSO data,
        DisableStructureSkillRuntimeData runtimeData)
        : base(data, runtimeData)
    {
    }

    public override void Execute(GridCell centerCell)
    {
        CellStructure structure = centerCell.Stats.structure;

        int disableTick = RuntimeData.skillExtraConfig.disableTickCount;

        if (IsValidTarget(centerCell))
            structure.DisableStructure(null, true, disableTick);

        base.Execute(centerCell);
    }

    public override bool IsValidTarget(GridCell centerCell)
    {
        if (centerCell == null) return false;
        CellStructure structure = centerCell.Stats.structure;
        return structure.type != StructureType.None && structure.isActive;
    }
}

