using System.Collections.Generic;
using UnityEngine;

public abstract class HumanActionSO : ScriptableObject
{
    public HumanActionType ActionType;
    public string actionName;
    public Sprite actionIcon;

    [Header("Config")]
    public HumanActionExecutionType executionType;
    public HumanActionPriority priority;
    public int minTargetPerExecution = 1;
    public int maxTargetPerExecution = 1;
    [HideInInspector]
    public float minUtility;
    [HideInInspector]
    public float maxUtility;
    public int cooldownTicks;

    public abstract HumanAction CreateLogic();
}
