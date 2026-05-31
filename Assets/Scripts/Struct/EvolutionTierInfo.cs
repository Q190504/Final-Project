using UnityEngine;

[System.Serializable]
public class EvolutionTierInfo
{
    public int tier;
    public float infectedRate;
    public int cost;

    [HideInInspector]
    public bool hasBeenChosen;
    [HideInInspector]
    public bool hasBeenSetInfo;
    [HideInInspector]
    public bool hasShownNoti;
    [HideInInspector]
    public EvolutionUpgradeNodeSO selectedNode;
}
