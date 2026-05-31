using UnityEngine;

[CreateAssetMenu(fileName = "AI Context Config", menuName = "Scriptable Objects/Human AI/AI Context Config")]
public class AIContextConfigSO : ScriptableObject
{
    [Header("Action's Strategic Weight")]
    public Vector2 SterilizeStrategicWeight;
    public Vector2 LockdownStrategicWeight;
    public Vector2 BuildVaccineResearchCenterStrategicWeight;
    public Vector2 BuildHospitalStrategicWeight;
    public Vector2 DevelopVaccineStrategicWeight;
}