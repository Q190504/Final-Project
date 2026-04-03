using UnityEngine;

[System.Serializable]
public struct HospitalExtraConfig
{
    public int inreasedInfectionResistance;
}

[CreateAssetMenu(fileName = "Hospital Data", menuName = "Scriptable Objects/Cell Property/Structure Data")]
public class HospitalDataSO : StructureDataSO
{
    public HospitalExtraConfig extraConfig;

    public override Structure CreateLogic()
    {
        return new Hospital(this);
    }
}
