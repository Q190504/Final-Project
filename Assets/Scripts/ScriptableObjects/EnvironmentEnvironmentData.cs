using UnityEngine;

[CreateAssetMenu(fileName = "New Environment Data", menuName = "Scriptable Objects/Cell Property/Environment Data")]
public class EnvironmentData : ScriptableObject
{
    public EnvironmentType type;
    public Sprite sprite;

    [Header("Base Gameplay Values")]
    public PopulationType populationType;
    public TempuratureType tempuratureType;
    [Range(0, 1)]
    public float maxSpawnRate;
    public bool isBlocked;
    public bool canBeLockeddown;
    public bool canHaveStructure;
    public bool canSpawnCarrier;
    public float priorityToHuman;
    public PriorityToMethods basePriorityToMethods;
}
