using System;

[Serializable]
public struct PopulationModifierEntry
{
    public PopulationType population;
    public float multiplier;

    public PopulationModifierEntry(PopulationType population, float multiplier)
    {
        this.population = population;
        this.multiplier = multiplier;
    }
}