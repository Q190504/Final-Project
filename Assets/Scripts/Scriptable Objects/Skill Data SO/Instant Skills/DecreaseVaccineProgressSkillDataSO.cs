using UnityEngine;

[System.Serializable]
public struct DecreaseVaccineProgressSkillExtraConfig
{
    [Range(0f, 1f)]
    public float decreaseVaccineProgressPercent;
}

[CreateAssetMenu(fileName = "Decrease Vaccine Progress Skill Data", menuName = "Scriptable Objects/Skill/Decrease Vaccine Progress")]
public class DecreaseVaccineProgressSkillDataSO : InstantSkillDataSO
{
    public DecreaseVaccineProgressSkillExtraConfig skillExtraConfig;

    public override IBaseSkill CreateSkill(SkillRuntimeData runtimeData)
    {
        return new DecreaseVaccineProgressSkill(this,
            (DecreaseVaccineProgressSkillRuntimeData)runtimeData);
    }

    public override SkillRuntimeData CreateRuntimeData()
    {
        return new DecreaseVaccineProgressSkillRuntimeData(this);
    }
}
