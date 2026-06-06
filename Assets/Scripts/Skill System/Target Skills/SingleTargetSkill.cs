using UnityEngine;

public interface ISingleTargetSkill
{
    bool IsValidTarget(GridCell centerCell);

    void Execute(GridCell centerCell);

    SingleTargetSkillRuntimeData GetSingleTargetSkillRuntimeData();
}

public abstract class SingleTargetSkill<TData, TRuntime> : TargetSkill<TData, TRuntime>, ISingleTargetSkill
    where TData : SingleTargetSkillDataSO
    where TRuntime : SingleTargetSkillRuntimeData
{
    protected SingleTargetSkill(TData data, TRuntime runtimeData) : base(data, runtimeData)
    {
    }

    public abstract bool IsValidTarget(GridCell centerCell);

    public SingleTargetSkillRuntimeData GetSingleTargetSkillRuntimeData()
    {
        return RuntimeData;
    }
}
