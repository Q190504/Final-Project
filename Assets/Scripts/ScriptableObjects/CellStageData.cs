using UnityEngine;

[CreateAssetMenu(fileName = "New Cell Stage Data", menuName = "Scriptable Objects/Cell Property/Cell Stage Data")]
public class CellStageData : ScriptableObject
{
    public CellStageType type;
    public Sprite sprite;

    [Header("Base Gameplay Values")]
    public CellStageStats cellStageStats;
}
