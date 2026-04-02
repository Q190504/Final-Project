using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PopulationModifierTable
{
    [SerializeField]
    private List<PopulationModifierEntry> entries = new();

    private Dictionary<PopulationType, float> lookup = new();

    private bool initialized = false;

    public void Initialize()
    {
        if (initialized || entries.Count == 0) return;

        foreach (var entry in entries)
        {
            lookup[entry.population] = entry.multiplier;
        }

        initialized = true;
    }

    public float GetModifier(PopulationType population)
    {
        if (lookup.TryGetValue(population, out float value))
            return value;

        return 1f;
    }

    public List<PopulationModifierEntry> GetEntries()
    {
        return entries;
    }
}