using UnityEngine;

[CreateAssetMenu(fileName = "New Temperature Data", menuName = "Scriptable Objects/Cell Property/Temperature Data")]
public class TemperatureData : ScriptableObject
{
    public TemperatureType type;
    public string displayName;
    public Sprite sprite;

    [Header("Base Gameplay Values")]
    public float minTempuratureValue;
    public float maxTempuratureValue;

    public float priorityToHuman;
    public PriorityToMethods basePriorityToMethods;
}

