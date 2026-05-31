using UnityEngine;

[CreateAssetMenu(fileName = "Bonus Min Distance Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Distance/Bonus Min Distance")]
public class BonusMinDistanceEffectSO : UpgradeEffectSO
{
    public int amount;
    
    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        runtime.bonusMinDistance += amount;
    }
}
