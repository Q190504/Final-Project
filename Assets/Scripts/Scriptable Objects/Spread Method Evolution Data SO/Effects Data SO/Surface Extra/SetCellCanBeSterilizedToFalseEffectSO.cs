using UnityEngine;

[CreateAssetMenu(fileName = "Set Cell Can Be Sterilized To False Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Surface/Set Cell Can Be Sterilized To False")]
public class SetCellCanBeSterilizedToFalseEffectSO : UpgradeEffectSO
{
    public int sterilizationImmunityTicks;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        if (sterilizationImmunityTicks <= 0)
            Debug.LogError($"sterilizationImmunityTicks can't <= 0, current: {sterilizationImmunityTicks}");

        if (runtime.methodType == SpreadMethodType.Surface)
        {
            SurfaceRuntimeData surfaceRuntime = runtime as SurfaceRuntimeData;
            surfaceRuntime.extraConfig.sterilizationImmunityTicks = sterilizationImmunityTicks;
        }
        else
            Debug.LogError($"Current method {runtime.methodType} is not Surface, can't execute upgrade!");
    }
}