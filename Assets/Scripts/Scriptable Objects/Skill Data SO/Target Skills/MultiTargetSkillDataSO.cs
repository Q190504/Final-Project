using UnityEngine;

[System.Serializable]
public struct MultiTargetSkillExtraConfig
{
    public int targetRadius;
}

public abstract class MultiTargetSkillDataSO : TargetSkillDataSO
{
    public MultiTargetSkillExtraConfig multiTargetSkillExtraConfig;
} 