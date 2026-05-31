using UnityEngine;

[CreateAssetMenu(fileName = "Additive Infection Power Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Infection Power/Additive Infection Power")]
public class AdditiveInfectionPowerEffectSO : UpgradeEffectSO
{
    public int amount;
    
    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        runtime.additiveInfectionPower += amount;
    }
}