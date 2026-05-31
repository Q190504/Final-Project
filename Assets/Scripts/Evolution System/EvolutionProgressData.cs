using System.Collections.Generic;
using UnityEngine;

public class EvolutionProgressData
{
    [Tooltip("Whether the player has chosen a specialization path (if false, they are still in the generalist path)")]
    public bool specializationChosen = false;

    public SpreadMethodType selectedSpreadType;

    public int completedTierIndex = -1;

    public Queue<int> pendingTierQueue = new();

    public List<EvolutionUpgradeNodeSO> selectedUpgrades = new();
}