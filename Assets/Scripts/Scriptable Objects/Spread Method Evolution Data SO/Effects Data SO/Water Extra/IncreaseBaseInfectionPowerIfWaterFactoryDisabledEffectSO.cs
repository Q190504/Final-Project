using UnityEngine;

[CreateAssetMenu(fileName = "Increase Base Infection Power If Water Factory Disabled Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Water/Increase Base Infection Power If Water Factory Disabled")]
public class IncreaseBaseInfectionPowerIfWaterFactoryDisabledEffectSO : UpgradeEffectSO
{
    [Range(0f, 1f)]
    public float bonusBaseInfectionPowerPercentIfAWaterFactoryDisabled;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        if (bonusBaseInfectionPowerPercentIfAWaterFactoryDisabled < 0)
            Debug.LogError($"targetInfectionGainPercentForCriticalOriginCell can't < 0, " +
                $"current: {bonusBaseInfectionPowerPercentIfAWaterFactoryDisabled}");

        if (runtime.methodType == SpreadMethodType.Water)
        {
            WaterRuntimeData surfaceRuntime = runtime as WaterRuntimeData;
            surfaceRuntime.extraConfig.increaseBaseInfectionPowerIfWaterFactoryDisabled = true;
            surfaceRuntime.extraConfig.bonusBaseInfectionPowerPercentIfAWaterFactoryDisabled = bonusBaseInfectionPowerPercentIfAWaterFactoryDisabled;
        }
        else
            Debug.LogError($"Current method {runtime.methodType} is not Water, can't execute upgrade!");
    }
}