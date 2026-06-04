using UnityEngine;

public class TargetSkillRuntimeData : SkillRuntimeData
{
    public TargetSkillExtraConfig targetSkillExtraConfig;

    public TargetSkillRuntimeData(TargetSkillExtraConfig targetSkillExtraConfig)
    {
        this.targetSkillExtraConfig = targetSkillExtraConfig;
    }
}