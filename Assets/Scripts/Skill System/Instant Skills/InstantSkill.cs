using UnityEngine;

public interface IInstantSkill
{
    void Execute();
}

public abstract class InstantSkill<TData, TRuntime> : BaseSkill<TData, TRuntime>, IInstantSkill
    where TData : InstantSkillDataSO
    where TRuntime : InstantSkillRuntimeData
{
    protected InstantSkill(TData data, TRuntime runtimeData) : base(data, runtimeData)
    {
    }

    public virtual void Execute()
    {
        PointsManager.Instance.SpendInfectionPoints(Data.infectionPointCost);
        RuntimeData.skillUseCount++;
        StartCooldown();
    }
}
