using UnityEngine;

[CreateAssetMenu(fileName = "Set Target's Infection Gain Percent For Critical Origin Cell Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Surface/Set Target's Infection Gain Percent For Critical Origin Cell")]
public class SetTargetInfectionGainPercentForCriticalOriginCellEffectSO : UpgradeEffectSO
{
    [Range(0f, 1f)]
    public float targetInfectionGainPercentForCriticalOriginCell;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        if (targetInfectionGainPercentForCriticalOriginCell < 0)
            Debug.LogError($"targetInfectionGainPercentForCriticalOriginCell can't < 0, " +
                $"current: {targetInfectionGainPercentForCriticalOriginCell}");

        if (runtime.methodType == SpreadMethodType.Surface)
        {
            SurfaceRuntimeData surfaceRuntime = runtime as SurfaceRuntimeData;
            surfaceRuntime.extraConfig.targetInfectionGainPercentForCriticalOriginCell = targetInfectionGainPercentForCriticalOriginCell;
        }
        else
            Debug.LogError($"Current method {runtime.methodType} is not Surface, can't execute upgrade!");
    }
}