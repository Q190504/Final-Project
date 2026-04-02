using UnityEngine;

[CreateAssetMenu(fileName = "New Tempurature Data", menuName = "Scriptable Objects/Cell Property/Tempurature Data")]
public class TempuratureData : ScriptableObject
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

