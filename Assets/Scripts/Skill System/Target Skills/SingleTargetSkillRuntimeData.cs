using UnityEngine;

public class SingleTargetSkillRuntimeData : TargetSkillRuntimeData
{
    public SingleTargetSkillExtraConfig singleTargetSkillExtraConfig;

    public SingleTargetSkillRuntimeData(SingleTargetSkillExtraConfig singleTargetSkillExtraConfig, 
        TargetSkillExtraConfig targetSkillExtraConfig) 
        : base(targetSkillExtraConfig)
    {
        this.singleTargetSkillExtraConfig = singleTargetSkillExtraConfig;
    }
}
