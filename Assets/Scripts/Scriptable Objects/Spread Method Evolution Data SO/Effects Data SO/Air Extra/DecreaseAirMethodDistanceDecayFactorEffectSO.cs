using UnityEngine;

[CreateAssetMenu(fileName = "Decrease Air Method Distance Decay Factor Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Air/Decrease Air Method's Distance Decay Factor")]
public class DecreaseAirMethodDistanceDecayFactorEffectSO : UpgradeEffectSO
{
    public float decreaseDistanceDecayFactorDelta;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        if (decreaseDistanceDecayFactorDelta < 0)
            Debug.LogError($"decreaseDistanceDecayFactorDelta can't < 0, " +
                $"current: {decreaseDistanceDecayFactorDelta}");

        if (runtime.methodType == SpreadMethodType.Air)
        {
            AirRuntimeData airRuntime = runtime as AirRuntimeData;
            airRuntime.extraConfig.distanceDecayFactor -= decreaseDistanceDecayFactorDelta;
            airRuntime.extraConfig.distanceDecayFactor = Mathf.Clamp01(airRuntime.extraConfig.distanceDecayFactor);
        }
        else
            Debug.LogError($"Current method {runtime.methodType} is not Air, can't execute upgrade!");
    }
}