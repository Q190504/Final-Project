using UnityEngine;

[System.Serializable]
public struct DevelopVaccineActionExtraConfig
{
    [Range(0, 100)]
    public float VaccineProgressPerExecution;
    [Range(0f, 1f)]
    public float UndetectedCellWeight;
}

[CreateAssetMenu(fileName = "Develop Vaccine Action", menuName = "Scriptable Objects/Human AI/Human Action/Develop Vaccine Action")]
public class DevelopVaccineActionSO : HumanActionSO
{
    public DevelopVaccineActionExtraConfig extraConfig;

    public override HumanAction CreateLogic()
    {
        var logic = new DevelopVaccineAction(this, extraConfig);
        return logic;
    }
}