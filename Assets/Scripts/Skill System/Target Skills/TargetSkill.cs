using System.Collections.Generic;
using UnityEngine;

public interface ITargetSkill
{
    void Execute(List<GridCell> targets);
    void Execute(GridCell target);

    TargetSkillRuntimeData GetTargetSkillRuntimeData();
}

public abstract class TargetSkill<TData, TRuntime> : BaseSkill<TData, TRuntime>
    where TData : TargetSkillDataSO
    where TRuntime : TargetSkillRuntimeData
{
    protected TargetSkill(TData data, TRuntime runtimeData) : base(data, runtimeData)
    {
    }

    public virtual void Execute(List<GridCell> targets)
    {
        PointsManager.Instance.SpendInfectionPoints(Data.infectionPointCost);
        RuntimeData.skillUseCount++;
        StartCooldown();
    }

    public virtual void Execute(GridCell target)
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
