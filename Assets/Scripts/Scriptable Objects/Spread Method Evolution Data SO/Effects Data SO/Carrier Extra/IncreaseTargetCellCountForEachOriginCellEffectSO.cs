using UnityEngine;

[CreateAssetMenu(fileName = "Increase Target Cell Count For Each Origin Cell Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Carrier/Increase Target Cell Count For Each Origin Cell")]
public class IncreaseTargetCellCountForEachOriginCellEffectSO : UpgradeEffectSO
{
    public int increaseTargetCellCountForEachOriginCellDelta;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        if (increaseTargetCellCountForEachOriginCellDelta < 0)
            Debug.LogError($"increaseTargetCellCountForEachOriginCellDelta can't < 0, " +
                $"current: {increaseTargetCellCountForEachOriginCellDelta}");

        if (runtime.methodType == SpreadMethodType.Carrier)
        {
            CarrierRuntimeData carrierRuntime = runtime as CarrierRuntimeData;
            carrierRuntime.extraConfig.targetCellCountForEachOriginCell += increaseTargetCellCountForEachOriginCellDelta;
        }
        else
            Debug.LogError($"Current method {runtime.methodType} is not Carrier, can't execute upgrade!");
    }
}