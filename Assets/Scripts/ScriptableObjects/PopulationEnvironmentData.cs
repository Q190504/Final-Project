using UnityEngine;

[CreateAssetMenu(fileName = "New Population Data", menuName = "Scriptable Objects/Cell Property/Population Data")]
public class PopulationData : ScriptableObject
{
    public PopulationType type;
    public Sprite sprite;

    [Header("Base Gameplay Values")]
    public float minPopulationValue;
    public float maxPopulationValue;

    public float weight;

    public float priorityToHuman;
    public PriorityToMethods basePriorityToMethods;
}
