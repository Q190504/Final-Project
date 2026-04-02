using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TemperatureModifierTable
{
    [SerializeField]
    private List<TemperatureModifierEntry> entries = new();

    private Dictionary<TemperatureType, float> lookup = new();

    private bool initialized = false;

    public void Initialize()
    {
        if (initialized || entries.Count == 0) return;

        foreach (var entry in entries)
        {
            lookup[entry.temperature] = entry.multiplier;
        }

        initialized = true;
    }

    public float GetModifier(TemperatureType temperature)
    {
        if (lookup.TryGetValue(temperature, out float value))
            return value;

        return 1f;
    }

    public List<TemperatureModifierEntry> GetEntries()
    {
        return entries;
    }
}