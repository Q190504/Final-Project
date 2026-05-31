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

    public void SetModifier(TemperatureType temperature, float multiplier)
    {
        if (lookup.ContainsKey(temperature))
        {
            lookup[temperature] = multiplier;
            int index = entries.FindIndex(e => e.temperature == temperature);
            if (index != -1)
            {
                entries[index] = new TemperatureModifierEntry(temperature, multiplier);
            }
        }
        else
        {
            lookup[temperature] = multiplier;
            entries.Add(new TemperatureModifierEntry(temperature, multiplier));
        }
    }

    public List<TemperatureModifierEntry> GetEntries()
    {
        return entries;
    }

    public TemperatureModifierTable Clone()
    {
        TemperatureModifierTable clone = new();
        clone.entries = new List<TemperatureModifierEntry>(entries);
        clone.initialized = false;
        return clone;
    }
}