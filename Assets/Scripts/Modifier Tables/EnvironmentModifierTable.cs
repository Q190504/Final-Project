using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnvironmentModifierTable
{
    [SerializeField]
    private List<EnvironmentModifierEntry> entries = new();

    private Dictionary<EnvironmentType, float> lookup = new();

    private bool initialized = false;

    public void Initialize()
    {
        if (initialized || entries.Count == 0) return;

        foreach (var entry in entries)
        {
            lookup[entry.environment] = entry.multiplier;
        }

        initialized = true;
    }

    public float GetModifier(EnvironmentType environment)
    {
        if (lookup.TryGetValue(environment, out float value))
            return value;

        return 1f;
    }

    public void SetModifier(EnvironmentType environment, float multiplier)
    {
        if (lookup.ContainsKey(environment))
        {
            lookup[environment] = multiplier;
            int index = entries.FindIndex(e => e.environment == environment);
            if (index != -1)
            {
                entries[index] = new EnvironmentModifierEntry { environment = environment, multiplier = multiplier };
            }
        }
        else
        {
            lookup[environment] = multiplier;
            entries.Add(new EnvironmentModifierEntry { environment = environment, multiplier = multiplier });
        }
    }

    public List<EnvironmentModifierEntry> GetEntries()
    {
        return entries;
    }

    public EnvironmentModifierTable Clone()
    {
        EnvironmentModifierTable clone = new()
        {
            entries = new List<EnvironmentModifierEntry>(entries),
            initialized = false
        };
        return clone;
    }
}