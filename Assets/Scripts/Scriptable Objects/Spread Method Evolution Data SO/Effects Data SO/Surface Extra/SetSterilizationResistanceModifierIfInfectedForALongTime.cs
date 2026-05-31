using UnityEngine;

[CreateAssetMenu(fileName = "Set Sterilization Resistance Bonus Percent If Cell Infected For A Long Time Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Surface/Set Sterilization Resistance Bonus Percent If Cell Infected For A Long Time")]
public class SetSterilizationResistanceBonusPercentIfInfectedForALongTimeEffect : UpgradeEffectSO
{
    public int minTickToBonusSterilizationResistancePercent = 1;
    [Range(0f, 1f)]
    public float sterilizationResistanceBonusPercentIfInfectedForALongTime;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        if (minTickToBonusSterilizationResistancePercent <= 0)
            Debug.LogError($"minTickToBonusSterilizationResistancePercent can't <= 0, " +
                $"current: {minTickToBonusSterilizationResistancePercent}");

        if (sterilizationResistanceBonusPercentIfInfectedForALongTime < 0)
            Debug.LogError($"sterilizationResistanceBonusPercentIfInfectedForALongTime can't < 0, " +
                $"current: {sterilizationResistanceBonusPercentIfInfectedForALongTime}");

        if (runtime.methodType == SpreadMethodType.Surface)
        {
            SurfaceRuntimeData surfaceRuntime = runtime as SurfaceRuntimeData;
            surfaceRuntime.extraConfig.minTickToBonusSterilizationResistancePercent = minTickToBonusSterilizationResistancePercent;
            surfaceRuntime.extraConfig.sterilizationResistanceModifierIfInfectedForALongTime = new(sterilizationResistanceBonusPercentIfInfectedForALongTime,
            SterilizationResistanceAdditiveSourceType.SurfaceUpgrade_InfectedForALongTime, ModifierType.Multiplicative);
        }
        else
            Debug.LogError($"Current method {runtime.methodType} is not Surface, can't execute upgrade!");
    }
}