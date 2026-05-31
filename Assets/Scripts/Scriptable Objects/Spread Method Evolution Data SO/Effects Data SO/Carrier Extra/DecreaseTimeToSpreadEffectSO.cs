using UnityEngine;

[CreateAssetMenu(fileName = "Decrease Time To Spread Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Carrier/Decrease Time To Spread")]
public class DecreaseTimeToSpreadEffectSO : UpgradeEffectSO
{
    public float decreaseSpreadSpeedDelta;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        if (decreaseSpreadSpeedDelta < 0)
            Debug.LogError($"decreaseSpreadSpeedDelta can't < 0, " +
                $"current: {decreaseSpreadSpeedDelta}");

        if (runtime.methodType == SpreadMethodType.Carrier)
        {
            CarrierRuntimeData carrierRuntime = runtime as CarrierRuntimeData;
            carrierRuntime.baseTickToSpread -= decreaseSpreadSpeedDelta;
            carrierRuntime.baseTickToSpread = Mathf.Max(1, carrierRuntime.baseTickToSpread);
        }
        else
            Debug.LogError($"Current method {runtime.methodType} is not Carrier, can't execute upgrade!");
    }
}