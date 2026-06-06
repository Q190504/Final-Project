using UnityEngine;

public class DestroyLockdownSkillRuntimeData : MultiTargetSkillRuntimeData
{
    public DestroyLockdownSkillExtraConfig skillExtraConfig;

    public DestroyLockdownSkillRuntimeData(DestroyLockdownSkillDataSO dataSO) 
        : base(dataSO.multiTargetSkillExtraConfig, dataSO.targetSkillExtraConfig)
    {
        skillExtraConfig = dataSO.skillExtraConfig;
    }
}