using UnityEngine;

[CreateAssetMenu(fileName = "Additive Sterilization Resistance Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Sterilization Resistance/Additive Sterilization Resistance")]
public class AdditiveSterilizationResistanceEffectSO : UpgradeEffectSO
{
    public int amount;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        runtime.additiveSterilizationResistance += amount;
    }
}
