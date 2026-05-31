using UnityEngine;

[CreateAssetMenu(fileName = "Set Infection Gain Percent For Water Cell Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Water/Set Infection Gain Percent For Water Cell")]
public class SetInfectionGainPercentForTargetCellHasWaterEffectSO : UpgradeEffectSO
{
    [Range(0f, 1f)]
    public float infectionGainPercentForWaterCell;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        if (infectionGainPercentForWaterCell < 0)
            Debug.LogError($"infectionGainPercentForWaterCell can't < 0, " +
                $"current: {infectionGainPercentForWaterCell}");

        if (runtime.methodType == SpreadMethodType.Water)
        {
            WaterRuntimeData surfaceRuntime = runtime as WaterRuntimeData;
            surfaceRuntime.extraConfig.infectionGainPercentForWaterCell = infectionGainPercentForWaterCell;
        }
        else
            Debug.LogError($"Current method {runtime.methodType} is not Water, can't execute upgrade!");
    }
}