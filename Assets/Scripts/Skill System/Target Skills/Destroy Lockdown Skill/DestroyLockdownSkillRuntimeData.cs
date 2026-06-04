using UnityEngine;

public class DestroyLockdownSkillRuntimeData : TargetSkillRuntimeData
{
    public DestroyLockdownSkillExtraConfig skillExtraConfig;

    public DestroyLockdownSkillRuntimeData(DestroyLockdownSkillDataSO dataSO) : base(dataSO.targetSkillExtraConfig)
    {
        skillExtraConfig = dataSO.skillExtraConfig;
    }
}