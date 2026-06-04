using UnityEngine;

[CreateAssetMenu(fileName = "Modify Infection Power Multiplier By Population Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Infection Power Multiplier By Population/Modify Infection Power Multiplier By Population")]
public class ModifyInfectionPowerMultiplierByPopulationEffectSO : UpgradeEffectSO
{
    public PopulationType populationType;
    public float multiplierDelta;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    { 
        float oldValue = runtime.populationModifiers.GetModifier(populationType);
        float newValue = oldValue * (1 + multiplierDelta);

        runtime.populationModifiers.SetModifier(populationType, newValue);
    }
}
