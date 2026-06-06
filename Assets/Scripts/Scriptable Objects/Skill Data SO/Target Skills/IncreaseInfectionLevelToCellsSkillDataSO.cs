using UnityEngine;

[System.Serializable]
public struct IncreaseInfectionLevelToCellsSkillExtraConfig
{
    public float increaseInfectionLevelPercentForEachCell;
}

[CreateAssetMenu(fileName = "Increase Infection Level To Cells Skill Data", menuName = "Scriptable Objects/Skill/Increase Infection Level To Cells")]
public class IncreaseInfectionLevelToCellsSkillDataSO : MultiTargetSkillDataSO
{
    public IncreaseInfectionLevelToCellsSkillExtraConfig skillExtraConfig;

    public override IBaseSkill CreateSkill(SkillRuntimeData runtimeData)
    {
        return new IncreaseInfectionLevelToCellsSkill(this, (IncreaseInfectionLevelToCellsSkillRuntimeData)runtimeData);
    }

    public override SkillRuntimeData CreateRuntimeData()
    {
        return new IncreaseInfectionLevelToCellsSkillRuntimeData(this);
    }
}