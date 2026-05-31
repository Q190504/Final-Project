using UnityEngine;

[System.Serializable]
public struct HospitalExtraConfig
{
    public int additionalInfectionResistance;
    public int sterilizeAmountWhenPlaced;

    public int sterilizeAmountEachTick;
}

[CreateAssetMenu(fileName = "Hospital Data", menuName = "Scriptable Objects/Cell Property/Structure Data/Hospital Data")]
public class HospitalDataSO : StructureDataSO
{
    public HospitalExtraConfig extraConfig;

    public override Structure CreateLogic()
    {
        return new Hospital(this);
    }
}
