using UnityEngine;

[System.Serializable]
public struct CellStageStats 
{
    public int minInfectionValue;
    public int maxInfectionValue;
    public int infectionResistance;
    [Range(0, 1)]
    public float bonusTargetInfectionGainPercent;

    [Range(0, 1)]
    public float detectionPercent;

    public bool isContagious;
    public bool isBlocked;
    public bool canBeSterilized;
    public bool canHasCarrier;
    public bool canBuildStructure;

    public bool canSwitchToDead;
    public float tickToDeadCount;

    public int evolutionPointsGained;
    public int infectionPointsGained;

    public float priorityToHuman;
    public PriorityToMethods basePriorityToMethods;
}
