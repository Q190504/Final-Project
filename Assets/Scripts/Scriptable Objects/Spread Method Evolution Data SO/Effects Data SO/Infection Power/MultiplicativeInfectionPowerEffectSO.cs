using UnityEngine;

[CreateAssetMenu(fileName = "Multiplicative Infection Power Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Infection Power/Multiplicative Infection Power")]
public class MultiplicativeInfectionPowerEffectSO : UpgradeEffectSO
{
    public float percentageDelta = 0f;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        runtime.multiplicativeInfectionPower *= 1 + percentageDelta;
    }
}