using UnityEngine;

[System.Serializable]
public struct LockdownActionExtraConfig
{
    [Header("Evaluation Target Weights")]
    [Range(0f, 1f), Tooltip("Weight for undetected cells when estimating infection.")]
    public float UndetectedCellWeight;
    [Range(0f, 1f), Tooltip("Weight for spread potential score when evaluating cell score. This and SafeNeighborsPopulationWeight should sum to 1.")]
    public float SpreadPotentialWeight;
    [Range(0f, 1f), Tooltip("Weight for safe neighbor population score when evaluating cell score. This and SpreadPotentialWeight should sum to 1.")]
    public float SafeNeighborsPopulationWeight;

    [Header("Build Chain Stats")]
    [Tooltip("Range for calculating infection pressure when building chain.")]
    public int InfectionPressureRadius;

    [Tooltip("Threshold for determining frontline cells based on infection pressure.")]
    public float FrontlinePressureThreshold;
    [Tooltip("Bonus for sealing a region when building chain.")]
    public float SealBonus;
    [Tooltip("Bonus for cells adjacent to existing lockdowns when building chain.")]
    public float AdjacentToExistingLockdownBonus;

    [Header("Calculate Raw Utility Stats")]
    [Tooltip("Weight for region completion when building chain.")]
    public float RegionCompletionWeight;
    [Tooltip("Weight for infection pressure when calculating raw utility.")]
    public float InfectionPressureWeight;
    [Tooltip("Weight for region protection score when calculating raw utility.")]
    public float RegionProtectionWeight;
    [Tooltip("Weight for targets' score when calculating raw utility.")]
    public float TargetsScoreWeight;

    [Header("Execution's Stats")]
    [Range(0, 100), Tooltip("Amount by which infection resistance is increased after a lockdown.")]
    public int InfectionResistanceIncreasedAfterLockdown;
    [Tooltip("Duration of the lockdown in ticks.")]
    public float lockdownDuration;
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
        float evaluationTargetWeightSum = extraConfig.SpreadPotentialWeight + extraConfig.SafeNeighborsPopulationWeight;
        if (evaluationTargetWeightSum > 1.01f || evaluationTargetWeightSum < 0.99f)
            Debug.LogWarning($"Evaluation target weights of LockdownActionSO should sum to 1. Current sum = {evaluationTargetWeightSum}");
        else
            Debug.Log($"Evaluation target weights of LockdownActionSO sum to 1. Current sum = {evaluationTargetWeightSum}");
    }
}