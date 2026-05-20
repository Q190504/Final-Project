using UnityEngine;

[System.Serializable]
public struct DisinfectActionExtraConfig
{
    [Range(0, 100)]
    public int disinfectAmount;

    [Range(0, 1f), Tooltip("Weight applied to undetected cells when calculating utility.")]
    public float undetectedCellWeight;
}

[CreateAssetMenu(fileName = "Disinfect Action", menuName = "Scriptable Objects/Human AI/Human Action/Disinfect Action")]
public class DisinfectActionSO : HumanActionSO
{
    public DisinfectActionExtraConfig extraConfig;

    public override HumanAction CreateLogic()
    {
        var logic = new DisinfectAction(this, extraConfig);
        return logic;
    }

    private void OnValidate()
    {
        if (maxTargetPerExecution<= 0)
            maxTargetPerExecution = 1;
    }
}