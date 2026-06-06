using UnityEngine;

public class DisableStructureSkillRuntimeData : SingleTargetSkillRuntimeData
{
    public DisableStructureSkillExtraConfig skillExtraConfig;

    public DisableStructureSkillRuntimeData(DisableStructureSkillDataSO dataSO) :
        base (dataSO.singleTargetSkillExtraConfig, dataSO.targetSkillExtraConfig)
    {
        skillExtraConfig = dataSO.skillExtraConfig;
    }
}
