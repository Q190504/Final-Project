using UnityEngine;

public class MultiTargetSkillRuntimeData : TargetSkillRuntimeData
{
    public MultiTargetSkillExtraConfig multiTargetSkillExtraConfig;

    public MultiTargetSkillRuntimeData(MultiTargetSkillExtraConfig multiTargetSkillExtraConfig, 
        TargetSkillExtraConfig targetSkillExtraConfig) : base(targetSkillExtraConfig)
    {
        this.multiTargetSkillExtraConfig = multiTargetSkillExtraConfig;
    }
}