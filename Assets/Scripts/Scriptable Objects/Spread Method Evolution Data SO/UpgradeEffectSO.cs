using UnityEngine;

public abstract class UpgradeEffectSO : ScriptableObject
{
    public abstract void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context);
}