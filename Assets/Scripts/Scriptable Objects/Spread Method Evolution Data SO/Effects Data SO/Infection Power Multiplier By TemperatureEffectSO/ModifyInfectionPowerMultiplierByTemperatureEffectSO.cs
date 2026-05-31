using UnityEngine;

[CreateAssetMenu(fileName = "Modify Infection Power Multiplier By Temperature Effect", menuName = "Scriptable Objects/Virus/Evolution/Effects/Infection Power Multiplier By Temperature/Modify Infection Power Multiplier By Temperature")]
public class ModifyInfectionPowerMultiplierByTemperatureEffectSO : UpgradeEffectSO
{
    public TemperatureType temperatureType;
    public float multiplierDelta;

    public override void Apply(SpreadMethodRuntimeData runtime, SpreadMethodContext context)
    {
        float oldValue = runtime.temperatureModifiers.GetModifier(temperatureType);
        float newValue = Mathf.Max(0f, oldValue + multiplierDelta);

        runtime.temperatureModifiers.SetModifier(temperatureType, newValue);
    }
}
