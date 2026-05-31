using UnityEngine;

[System.Serializable]
public struct SterilizeActionExtraConfig
{
    [Range(0, 100)]
    public int minSterilizedAmount;

    [Range(0, 100)]
    public int maxSterilizedAmount;

    [Range(0, 1f), Tooltip("Weight applied to undetected cells when calculating cell's score & action's utility.")]
    public float undetectedCellWeight;

    [Tooltip("Penalty applied to dead cells when calculating cell's score.")]
    public float deadCellPenalty;
}

[CreateAssetMenu(fileName = "Sterilized Action", menuName = "Scriptable Objects/Human AI/Human Action/Sterilized Action")]
public class SterilizedActionSO : HumanActionSO
{
    public SterilizeActionExtraConfig extraConfig;

    public override HumanAction CreateLogic()
    {
        var logic = new SterilizeAction(this, extraConfig);
        return logic;
    }

    private void OnValidate()
    {
        if (maxTargetPerExecution <= 0)
            maxTargetPerExecution = 1;
    }
}