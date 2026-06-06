using UnityEngine;

[System.Serializable]
public struct SingleTargetSkillExtraConfig
{

}

public abstract class SingleTargetSkillDataSO : TargetSkillDataSO
{
    public SingleTargetSkillExtraConfig singleTargetSkillExtraConfig;
}
