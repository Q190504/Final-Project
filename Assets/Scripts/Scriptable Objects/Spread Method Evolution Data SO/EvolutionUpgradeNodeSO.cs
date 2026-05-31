using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Evolution Upgrade Node SO", menuName = "Scriptable Objects/Virus/Evolution/Evolution Upgrade Node SO")]

public class EvolutionUpgradeNodeSO : ScriptableObject
{
    public string upgradeName;

    [TextArea]
    public string description;

    public Sprite icon;

    public List<UpgradeEffectSO> effects;
}