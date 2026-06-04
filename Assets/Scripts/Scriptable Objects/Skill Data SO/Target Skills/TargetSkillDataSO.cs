using UnityEngine;

[System.Serializable]
public struct TargetSkillExtraConfig
{
    public int targetRadius;
}

public abstract class TargetSkillDataSO : SkillDataSO
{
    public TargetSkillExtraConfig targetSkillExtraConfig;
} 