using UnityEngine;

[System.Serializable]
public struct BuildVaccineResearchCenterActionExtraConfig
{
    [Range(0f, 1f), Tooltip("Percentage of the base size used to calculate the detect safe radius when building vaccine research center.")]
    public float detectSafeRadiusPercent;

    [Range(0f, 1f), Tooltip("Weight applied to undetected cells.")]
    public float undetectedCellWeight;
}

[CreateAssetMenu(fileName = "Build Vaccine Research Center Action", 
    menuName = "Scriptable Objects/Human AI/Human Action/Build Vaccine Research Center Action")]
public class BuildVaccineResearchCenterActionSO : HumanActionSO
{
    public BuildVaccineResearchCenterActionExtraConfig extraConfig;

    public override HumanAction CreateLogic()
    {
        var logic = new BuildVaccineResearchCenterAction(this, extraConfig);

        return logic;
    }

    //private void OnValidate()
    //{

    //}
}
