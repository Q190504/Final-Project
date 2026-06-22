using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Spread Method Target Visualization Database", menuName = "Scriptable Objects/UI/Spread Method/Spread Method Target Visualization Database")]
public class SpreadMethodTargetVisualizationDatabase : ScriptableObject
{
    [SerializeField]
    private List<Entry> entries;

    public SpreadMethodTargetVisualizationPreset Get(SpreadMethodType type)
    {
        return entries
            .First(x => x.Type == type)
            .Preset;
    }

    [Serializable]
    private class Entry
    {
        public SpreadMethodType Type;
        public SpreadMethodTargetVisualizationPreset Preset;
    }
}
