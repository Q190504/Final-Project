using UnityEngine;

[System.Serializable]
public struct DecreaseTimeToSpreadOfAllMethodForDurationSkillExtraConfig
{
    [Range(0f, 1f)]
    public float decreaseTimeToSpreadPercentForEachMethod;

    public int duration;
}

[CreateAssetMenu(fileName = "Decrease Time To Spread Of All Method For Duration Skill Data", menuName = "Scriptable Objects/Skill/Decrease Time To Spread Of All Method For Duration")]
public class DecreaseTimeToSpreadOfAllMethodForDurationSkillDataSO : InstantSkillDataSO
{
    public DecreaseTimeToSpreadOfAllMethodForDurationSkillExtraConfig skillExtraConfig;

    public override IBaseSkill CreateSkill(SkillRuntimeData runtimeData)
    {
        return new DecreaseTimeToSpreadOfAllMethodForDurationSkill(this,
            (DecreaseTimeToSpreadOfAllMethodForDurationSkillRuntimeData)runtimeData);
    }

    public override SkillRuntimeData CreateRuntimeData()
    {
        return new DecreaseTimeToSpreadOfAllMethodForDurationSkillRuntimeData(this);
    }

    private void OnValidate()
    {
        if (skillExtraConfig.duration <= 0)
        {
            Debug.LogError($"Duration must >= 0, duration: {skillExtraConfig.duration}");
        }

        if(cooldownTicks < skillExtraConfig.duration)
        {
            Debug.LogError($"Cooldown must be >= duration, cooldownTicks: {cooldownTicks}, duration: {skillExtraConfig.duration}");
        }
    }
}