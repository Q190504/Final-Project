using UnityEngine;

[CreateAssetMenu(fileName = "Increase Chance Spread To Cells Have Structure Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Carrier/Increase Chance Spread To Cells Have Structure")]
public class IncreaseChanceSpreadToCellsHaveStructureEffectSO : UpgradeEffectSO
{
    public float increaseChanceSpreadToCellsHaveStructureDelta;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        if (increaseChanceSpreadToCellsHaveStructureDelta < 0)
            Debug.LogError($"increaseChanceSpreadToCellsHaveStructureDelta can't < 0, " +
                $"current: {increaseChanceSpreadToCellsHaveStructureDelta}");

        if (runtime.methodType == SpreadMethodType.Carrier)
        {
            CarrierRuntimeData carrierRuntime = runtime as CarrierRuntimeData;
            carrierRuntime.extraConfig.structurePriority += increaseChanceSpreadToCellsHaveStructureDelta;
        }
        else
            Debug.LogError($"Current method {runtime.methodType} is not Carrier, can't execute upgrade!");
    }
}