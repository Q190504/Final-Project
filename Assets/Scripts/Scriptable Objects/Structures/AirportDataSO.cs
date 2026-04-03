using UnityEngine;

[System.Serializable]
public struct AirportExtraConfig
{

}

[CreateAssetMenu(fileName = "Airport Data", menuName = "Scriptable Objects/Cell Property/Structure Data")]
public class AirportDataSO : StructureDataSO
{
    public AirportExtraConfig extraConfig;

    public override Structure CreateLogic()
    {
        return new Airport(this); 
    }
}
