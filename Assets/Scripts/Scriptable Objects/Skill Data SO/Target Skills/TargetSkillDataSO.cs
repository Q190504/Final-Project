using UnityEngine;

[System.Serializable]
public struct TargetSkillExtraConfig
{

}

public abstract class TargetSkillDataSO : SkillDataSO
{
    public TargetSkillExtraConfig targetSkillExtraConfig;
}