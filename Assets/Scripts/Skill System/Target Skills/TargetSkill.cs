using UnityEngine;

public interface ITargetSkill
{
    bool IsValidTarget(GridCell centerCell);

    void Execute(GridCell centerCell);

    TargetSkillRuntimeData GetTargetSkillRuntimeData();
}

public abstract class TargetSkill<TData, TRuntime> : BaseSkill<TData, TRuntime>, ITargetSkill
    where TData : TargetSkillDataSO
    where TRuntime : TargetSkillRuntimeData
{
    protected TargetSkill(TData data, TRuntime runtimeData) : base(data, runtimeData)
    {
    }

    public abstract bool IsValidTarget(GridCell centerCell);

    public virtual void Execute(GridCell centerCell)
    {
        PointsManager.Instance.SpendInfectionPoints(Data.infectionPointCost);
        RuntimeData.skillUseCount++;
        StartCooldown();
    }

    public TargetSkillRuntimeData GetTargetSkillRuntimeData()
    {
        return RuntimeData;
    }
}