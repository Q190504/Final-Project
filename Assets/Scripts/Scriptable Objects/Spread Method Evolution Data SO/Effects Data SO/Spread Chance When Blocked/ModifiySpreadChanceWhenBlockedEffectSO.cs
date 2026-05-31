using UnityEngine;

[CreateAssetMenu(fileName = "Increase Spread Chance When Spread To Blocked Cells Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Spread Chance When Blocked/Increase Spread Chance When Spread To Blocked Cells")]
public class IncreaseSpreadChanceWhenSpreadToBlockedCellsEffectSO : UpgradeEffectSO
{
    public float increaseSpreadChanceWhenSpreadToBlockedCells;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        runtime.spreadChanceWhenBlockedModifier += increaseSpreadChanceWhenSpreadToBlockedCells;
        runtime.spreadChanceWhenBlockedModifier = Mathf.Min(runtime.spreadChanceWhenBlockedModifier, 1f);
    }
}
 