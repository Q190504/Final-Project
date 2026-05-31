using System.Collections.Generic;
using UnityEngine;

public abstract class StructureDataSO : ScriptableObject
{
    public StructureType type;
    public StructureLogicType logicType;
    public string displayName;
    public Sprite sprite;

    [Header("Base Gameplay Values")]
    public List<SpawnEnvironmentAndRate> spawnEnvironmentAndRateList;
    [Range(0, 1)]
    public float spawnRate;
    public int maxAmountInAMatch;
    public float effectRangePercent;
    public float priorityToHuman;
    public PriorityToMethods basePriorityToMethods;

    public int infectionPointWhenDestroyed;
    public int evolutionPointWhenDestroyed;
    //public int mutationPointWhenDestroyed;

    public abstract Structure CreateLogic();
}

[System.Serializable]
public struct SpawnEnvironmentAndRate
{
    public EnvironmentType environmentTypeCanSpawn;
    [Range(0, 1)]
    public float spawnRateOnEnvironment;
}
