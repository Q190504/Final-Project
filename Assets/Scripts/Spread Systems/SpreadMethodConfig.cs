using UnityEngine;

[System.Serializable]
public class SpreadMethodConfig
{
    [Header("Base Stats")]
    public SpreadMethodType methodType;
    [Range(0, 100)]
    public int baseInfectionPower = 1;
    public int methodOrder;         // logic order between methods
    public int eventPriority;       // logic order between events
    public float tickToSpread = 1f;
    public float spreadChanceWhenBlocked = 0f;
    public int spreadSpeed = 1;
    public SpreadDirection spreadDirection;
    public int minDistance = 1;
    public int maxDistance = 1;
    [Range(0f, 1f)]
    public float detectionRate;

    [Header("Modifier Tables")]
    public EnvironmentModifierTable environmentModifiers;
    public TemperatureModifierTable temperatureModifiers;
    public PopulationModifierTable populationModifiers;
}
