using UnityEngine;

[CreateAssetMenu(fileName = "Multiplicative Sterilization Resistance Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Sterilization Resistance/Multiplicative Sterilization Resistance")]
public class MultiplicativeSterilizationResistanceEffectSO : UpgradeEffectSO
{
    public float percentageDelta = 0f;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        runtime.multiplicativeSterilizationResistance += percentageDelta;
        runtime.multiplicativeSterilizationResistance = Mathf.Max(runtime.multiplicativeSterilizationResistance, 0f);
    }
}
