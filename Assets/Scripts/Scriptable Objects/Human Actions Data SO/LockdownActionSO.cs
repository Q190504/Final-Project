using UnityEngine;

[System.Serializable]
public struct LockdownActionExtraConfig
{
    [Header("Evaluation Target Weights")]
    [Range(0f, 1f), Tooltip("Weight for undetected cells when estimating infection.")]
    public float UndetectedCellWeight;
    [Range(0f, 1f), Tooltip("Weight for infection pressure score when evaluating cell score. This and SafeNeighborsPopulationWeight should sum to 1.")]
    public float InfectionPressureWeight;
    [Range(0f, 1f), Tooltip("Weight for frontier score when evaluating cell score. This and SafeNeighborsPopulationWeight should sum to 1.")]
    public float FrontierCellWeight;
    [Range(0f, 1f), Tooltip("Weight for safe neighbor population score when evaluating cell score. This and InfectionPressureWeight/FrontierWeight should sum to 1.")]
    public float SafeNeighborsPopulationWeight;
    [Tooltip("Penalty for high-infected cells.")]
    public float CurrentCellInfectionPenaltyWeight;

    [Header("Build Chain Stats")]
    [Tooltip("Detect radius when building chain.")]
    public int DetectRadius;
    [Tooltip("Weight for cell score when building fronline chain.")]
    public float cellScoreWeight;
    [Tooltip("Threshold for determining frontline cells based on infection pressure.")]
    public float FrontlinePressureThreshold;
    [Tooltip("Bonus for sealing a chain when building chain.")]
    public float SealBonus;
    [Tooltip("Bonus for cells adjacent to existing lockdowns when building chain.")]
    public float AdjacentToExistingLockdownBonus;
    [Tooltip("Bonus for cells make progress to anchor (mountain cells / map's boundary cell) when building frontline chain.")]
    public float AnchorProgressBonus;
    [Tooltip("Bonus for cells near high-populaion cells when building frontline chain.")]
    public float NearHighPopulationCellBonus;
    [Tooltip("Penalty for cells have to many adjacent to existing lockdowns when building frontline chain.")]
    public float ClusterPenalty;

    [Header("Calculate Raw Utility Stats")]
    [Tooltip("Weight for region completion when building chain.")]
    public float RegionCompletionWeight;
    [Tooltip("Weight for region protection score when calculating raw utility.")]
    public float RegionProtectionWeight;
    [Tooltip("Weight for targets' score when calculating raw utility.")]
    public float TargetsScoreWeight;

    [Header("Execution's Stats")]
    [Range(0, 100), Tooltip("Amount by which infection resistance is increased after a lockdown.")]
    public int InfectionResistanceIncreasedAfterLockdown;
    [Tooltip("Duration of the lockdown in ticks.")]
    public float LockdownDuration;
}

[CreateAssetMenu(fileName = "Lockdown Action", menuName = "Scriptable Objects/Human AI/Human Action/Lockdown Action")]
public class LockdownActionSO : HumanActionSO
{
    public LockdownActionExtraConfig extraConfig;

    public override HumanAction CreateLogic()
    {
        var logic = new LockdownAction(this, extraConfig);
        return logic;
    }

    private void OnValidate()
    {
        float evaluationTargetForRegionWeightSum = extraConfig.InfectionPressureWeight + extraConfig.SafeNeighborsPopulationWeight;
        if (evaluationTargetForRegionWeightSum > 1.01f || evaluationTargetForRegionWeightSum < 0.99f)
            Debug.LogWarning($"Evaluation target weights of LockdownActionSO should sum to 1. Current sum = {evaluationTargetForRegionWeightSum}");
        
        float evaluationTargetForFrontierWeightSum = extraConfig.FrontierCellWeight + extraConfig.SafeNeighborsPopulationWeight;
        if (evaluationTargetForFrontierWeightSum > 1.01f || evaluationTargetForFrontierWeightSum < 0.99f)
            Debug.LogWarning($"Evaluation target weights of LockdownActionSO should sum to 1. Current sum = {evaluationTargetForFrontierWeightSum}");
    }
}