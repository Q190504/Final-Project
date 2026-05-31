using UnityEngine;

[CreateAssetMenu(fileName = "Increase Chance Spread To Safe Cell Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Carrier/Increase Chance Spread To Safe Cell")]
public class IncreaseChanceSpreadToSafeCellEffectSO : UpgradeEffectSO
{
    public float increaseChanceSpreadToSafeCellDelta;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        if (increaseChanceSpreadToSafeCellDelta < 0)
            Debug.LogError($"increaseChanceSpreadToSafeCellDelta can't < 0, " +
                $"current: {increaseChanceSpreadToSafeCellDelta}");

        if (runtime.methodType == SpreadMethodType.Carrier)
        {
            CarrierRuntimeData carrierRuntime = runtime as CarrierRuntimeData;
            carrierRuntime.extraConfig.weightBonusForSafeCells += increaseChanceSpreadToSafeCellDelta;
        }
        else
            Debug.LogError($"Current method {runtime.methodType} is not Carrier, can't execute upgrade!");
    }
}