using UnityEngine;

[CreateAssetMenu(fileName = "New Environment Data", menuName = "Scriptable Objects/Cell Property/Environment Data")]
public class EnvironmentData : ScriptableObject
{
    public EnvironmentType type;
    public string displayName;
    public Sprite sprite;

    [Header("Base Gameplay Values")]
    public PopulationType populationType;
    public TemperatureType tempuratureType;
    public bool isBlocked;
    public bool canBeLockeddown;
    public bool canHaveStructure;
    public bool canSpawnCarrier;
    public float priorityToHuman;
    public PriorityToMethods basePriorityToMethods;
}
