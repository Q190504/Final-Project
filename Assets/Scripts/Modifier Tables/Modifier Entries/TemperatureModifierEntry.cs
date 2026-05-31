using System;

[Serializable]
public struct TemperatureModifierEntry
{
    public TemperatureType temperature;
    public float multiplier;

    public TemperatureModifierEntry(TemperatureType temperature, float multiplier)
    {
        this.temperature = temperature;
        this.multiplier = multiplier;
    }
}