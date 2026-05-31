using UnityEngine;

[CreateAssetMenu(fileName = "Has Chance To Spread To Non Water Cell Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Water/Has Chance To Spread To Non Water Cell")]
public class HasChanceToSpreadToNonWaterCellEffectSO : UpgradeEffectSO
{
    [Range(0f, 1f)]
    public float chanceToSpreadToNonWaterCell;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        if (chanceToSpreadToNonWaterCell < 0)
            Debug.LogError($"chanceToSpreadToNonWaterCell can't < 0, " +
                $"current: {chanceToSpreadToNonWaterCell}");

        if (runtime.methodType == SpreadMethodType.Water)
        {
            WaterRuntimeData waterRuntime = runtime as WaterRuntimeData;
            waterRuntime.extraConfig.chanceToSpreadToNonWaterCell = chanceToSpreadToNonWaterCell;
        }
        else
            Debug.LogError($"Current method {runtime.methodType} is not Water, can't execute upgrade!");
    }
}