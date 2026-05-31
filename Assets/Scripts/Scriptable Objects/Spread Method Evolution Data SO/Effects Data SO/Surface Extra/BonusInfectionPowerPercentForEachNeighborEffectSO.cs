using UnityEngine;

[CreateAssetMenu(fileName = "Bonus Infection Power Percent For Each Neighbor Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Surface/Bonus Infection Power Percent For Each Neighbor")]
public class BonusInfectionPowerPercentForEachNeighborEffectSO : UpgradeEffectSO
{
    [Range(0f, 1f)]
    public float infectionPowerPercentBonusEachNeighbor;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        if (infectionPowerPercentBonusEachNeighbor < 0)
            Debug.LogError($"infectionPowerPercentBonusEachNeighbor can't < 0, " +
                $"current: {infectionPowerPercentBonusEachNeighbor}");

        if (runtime.methodType == SpreadMethodType.Surface)
        {
            SurfaceRuntimeData surfaceRuntime = runtime as SurfaceRuntimeData;
            surfaceRuntime.extraConfig.infectionPowerPercentBonusEachNeighbor = infectionPowerPercentBonusEachNeighbor;
        }
        else
            Debug.LogError($"Current method {runtime.methodType} is not Surface, can't execute upgrade!");
    }
}