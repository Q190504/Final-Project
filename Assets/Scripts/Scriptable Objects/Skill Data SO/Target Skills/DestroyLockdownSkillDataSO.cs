using UnityEngine;

[System.Serializable]
public struct DestroyLockdownSkillExtraConfig
{

}

[CreateAssetMenu(fileName = "Destroy Lockdown Skill Data", menuName = "Scriptable Objects/Skill/Destroy Lockdown")]
public class DestroyLockdownSkillDataSO : MultiTargetSkillDataSO
{
    public DestroyLockdownSkillExtraConfig skillExtraConfig;

    public override IBaseSkill CreateSkill(SkillRuntimeData runtimeData)
    {
        return new DestroyLockdownSkill(this, (DestroyLockdownSkillRuntimeData)runtimeData);
    }

    public override SkillRuntimeData CreateRuntimeData()
    {
        return new DestroyLockdownSkillRuntimeData(this);
    }
}
