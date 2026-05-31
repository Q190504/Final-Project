using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Evolution Tree SO", menuName = "Scriptable Objects/Virus/Evolution/Evolution Tree SO")]
public class EvolutionTreeSO : ScriptableObject
{
    public SpreadMethodType spreadType;

    public List<EvolutionTierData> tiers;
}