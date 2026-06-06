using UnityEngine;

[System.Serializable]
public struct DisableStructureSkillExtraConfig
{
    public int disableTickCount;
}

[CreateAssetMenu(fileName = "Disable Structure Skill Data", menuName = "Scriptable Objects/Skill/Disable Structure")]
public class DisableStructureSkillDataSO : SingleTargetSkillDataSO
{
    public DisableStructureSkillExtraConfig skillExtraConfig;

    public override IBaseSkill CreateSkill(SkillRuntimeData runtimeData)
    {
        return new DisableStructureSkill(this, (DisableStructureSkillRuntimeData)runtimeData);
    }

    public override SkillRuntimeData CreateRuntimeData()
    {
        return new DisableStructureSkillRuntimeData(this);
    }
}
