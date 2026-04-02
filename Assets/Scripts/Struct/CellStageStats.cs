using UnityEngine;

[System.Serializable]
public struct CellStageStats 
{
    public int minInfectionValue;
    public int maxInfectionValue;
    public int infectionResistance;
    [Range(0, 1)]
    public float targetInfectionIncreasePercent;

    [Range(0, 1)]
    public float detectionPercent;

    public bool isContagious;
    public bool isBlocked;

    public bool canSwitchToDead;
    public float tickToDeadCount;

    public float priorityToHuman;
    public PriorityToMethods basePriorityToMethods;
}
