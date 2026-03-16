using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Structure Data", menuName = "Scriptable Objects/Cell Property/Structure Data")]
public class StructureData : ScriptableObject
{
    public StructureType type;
    public Sprite sprite;

    [Header("Base Gameplay Values")]
    public List<SpawnEnvironmentAndRate> spawnEnvironmentAndRateList;
    [Range(0, 1)]
    public float spawnRate;
    public int maxAmountInAMatch;
    public float effectRangePercent;
    public float priorityToHuman;
    public PriorityToMethods basePriorityToMethods;
}

[System.Serializable]
public struct SpawnEnvironmentAndRate
{
    public EnvironmentType environmentTypeCanSpawn;
    [Range(0, 1)]
    public float spawnRateOnEnvironment;
}
