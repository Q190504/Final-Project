using UnityEngine;

[CreateAssetMenu(fileName = "Set Sterilization Resistance Bonus For Water Cell Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Water/Set Sterilization Resistance Bonus For Water Cell")]
public class SetSterilizationResistanceBonusForWaterCellEffectSO : UpgradeEffectSO
{
    [Range(0, 100)]
    public int sterilizationResistanceBonusIfCellHasWater;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        if (sterilizationResistanceBonusIfCellHasWater < 0)
            Debug.LogError($"sterilizationResistanceBonusIfCellHasWater can't < 0, " +
                $"current: {sterilizationResistanceBonusIfCellHasWater}");

        if (runtime.methodType == SpreadMethodType.Water)
        {
            WaterRuntimeData waterRuntime = runtime as WaterRuntimeData;
            waterRuntime.extraConfig.sterilizationResistanceModifierCellHasWater = new(sterilizationResistanceBonusIfCellHasWater,
                SterilizationResistanceAdditiveSourceType.SurfaceUpgrade_HasWater, ModifierType.Additive);
        }
        else
            Debug.LogError($"Current method {runtime.methodType} is not Water, can't execute upgrade!");
    }
}