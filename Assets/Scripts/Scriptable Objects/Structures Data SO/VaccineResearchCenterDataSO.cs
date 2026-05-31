using UnityEngine;

[System.Serializable]
public struct VaccineResearchCenterExtraConfig
{
    [Range(0f, 100f)]
    public float vaccineProgressIncrementPercentPerTick;
}

[CreateAssetMenu(fileName = "Vaccine Research Center Data", menuName = "Scriptable Objects/Cell Property/Structure Data/Vaccine Research Center Data")]
public class VaccineResearchCenterDataSO : StructureDataSO
{
    public VaccineResearchCenterExtraConfig extraConfig;

    public override Structure CreateLogic()
    {
        return new VaccineResearchCenter(this);
    }
}
