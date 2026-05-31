using UnityEngine;

[CreateAssetMenu(fileName = "Increase Base Carrier Spawn Chance Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Carrier/Increase Base Carrier Spawn Chance")]
public class IncreaseBaseCarrierSpawnChanceEffectSO : UpgradeEffectSO
{
    [Range(0f, 1f)]
    public float increaseBaseCarrierSpawnChanceDelta;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        if (increaseBaseCarrierSpawnChanceDelta < 0)
            Debug.LogError($"increaseBaseCarrierSpawnChanceDelta can't < 0, " +
                $"current: {increaseBaseCarrierSpawnChanceDelta}");

        if (runtime.methodType == SpreadMethodType.Carrier)
        {
            CarrierRuntimeData carrierRuntime = runtime as CarrierRuntimeData;
            carrierRuntime.extraConfig.baseCarrierSpawnChance += increaseBaseCarrierSpawnChanceDelta;
            carrierRuntime.extraConfig.baseCarrierSpawnChance = Mathf.Clamp01(carrierRuntime.extraConfig.baseCarrierSpawnChance);
        }
        else
            Debug.LogError($"Current method {runtime.methodType} is not Carrier, can't execute upgrade!");
    }
}
