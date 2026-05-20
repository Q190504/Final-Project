using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Threat Tier Data", menuName = "Scriptable Objects/Human AI/Threat Tier")]
public class ThreatTierSO : ScriptableObject
{
    public ThreatTier tierType;
    public string tierName;

    [Header("Threat Level Range")]
    public ThreatLevelRange threatRange;

    [Header("Behavior")]
    public int maxActionsPerTick;
    public List<HumanActionType> availableActions;
}