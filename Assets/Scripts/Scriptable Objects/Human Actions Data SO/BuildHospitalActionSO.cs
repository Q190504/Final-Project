using UnityEngine;

[System.Serializable]
public struct BuildHospitalExtraConfig
{
    [Range(0f, 1f), Tooltip("Weight applied to the average benefit when evaluating a cell. This and total benefit weights should sum to 1.")]
    public float avgBenefitWeight;

    [Range(0f, 1f), Tooltip("Weight applied to the total benefit when evaluating a cell. This and average benefit weights should sum to 1.")]
    public float totalBenefitWeight;

    [Range(0f, 1f), Tooltip("Weight applied to undetected cells.")]
    public float undetectedCellWeight;

    [Tooltip("Weight applied to the distance from the nearest hospital. Higher values increase the penalty for being closer to a hospital.")]
    public float hospitalDistancePenaltyWeight;

    [Tooltip("Penalty for cells can't be sterilized.")]
    public float CellCantBeSterilizedPenalty;
}

[CreateAssetMenu(fileName = "Build Hospital Action", menuName = "Scriptable Objects/Human AI/Human Action/Build Hospital Action")]
public class BuildHospitalActionSO : HumanActionSO
{
    public BuildHospitalExtraConfig extraConfig;

    public override HumanAction CreateLogic()
    {
        var logic = new BuildHospitalAction(this, extraConfig);
        return logic;
    }

    private void OnValidate()
    {
        float benefitWeightSum = extraConfig.totalBenefitWeight + extraConfig.avgBenefitWeight;

        if (Mathf.Abs(benefitWeightSum - 1f) > 0.0001f)
            Debug.LogWarning($"Benefit weights of BuildHospitalActionSO should sum to 1. Current sum = {benefitWeightSum}");
    }
}
