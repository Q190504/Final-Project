using System.Collections.Generic;
using UnityEngine;

public interface IMultiTargetSkill
{
    bool IsValidTargets(List<GridCell> targets);

    List<GridCell> GetTargets(GridCell centerCell);

    void Execute(List<GridCell> targets);

    MultiTargetSkillRuntimeData GetMultiTargetSkillRuntimeData();
}

public abstract class MultiTargetSkill<TData, TRuntime> : TargetSkill<TData, TRuntime>, IMultiTargetSkill
    where TData : MultiTargetSkillDataSO
    where TRuntime : MultiTargetSkillRuntimeData
{
    protected MultiTargetSkill(TData data, TRuntime runtimeData) : base(data, runtimeData)
    {
    }

    public abstract bool IsValidTargets(List<GridCell> targets);

    public abstract List<GridCell> GetTargets(GridCell centerCell);

    public MultiTargetSkillRuntimeData GetMultiTargetSkillRuntimeData()
    {
        return RuntimeData;
    }
}