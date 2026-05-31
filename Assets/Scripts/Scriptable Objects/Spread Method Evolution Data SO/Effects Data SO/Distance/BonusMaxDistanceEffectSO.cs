using UnityEngine;

[CreateAssetMenu(fileName = "Bonus Max Distance Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Distance/Bonus Max Distance")]
public class BonusMaxDistanceEffectSO : UpgradeEffectSO
{
    public int amount;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        runtime.bonusMaxDistance += amount;
    }
}
