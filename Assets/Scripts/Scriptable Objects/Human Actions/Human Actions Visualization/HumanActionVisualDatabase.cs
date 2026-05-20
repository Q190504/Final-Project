using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Human Action Visual Database", menuName = "Scriptable Objects/UI/Human Action/Human Action Visual Database")]
public class HumanActionVisualDatabase : ScriptableObject
{
    [SerializeField]
    private List<Entry> entries;

    public HumanActionVisualPreset Get(HumanActionType type)
    {
        return entries
            .First(x => x.Type == type)
            .Preset;
    }

    [Serializable]
    private class Entry
    {
        public HumanActionType Type;
        public HumanActionVisualPreset Preset;
    }
}