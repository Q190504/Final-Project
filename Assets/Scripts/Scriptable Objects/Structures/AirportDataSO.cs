using UnityEngine;

[System.Serializable]
public struct AirportExtraConfig
{
    [Range(0, 1)]
    public float increaseCarrierSpawningPercent;
}

[CreateAssetMenu(fileName = "Airport Data", menuName = "Scriptable Objects/Cell Property/Structure Data/Airport Data")]
public class AirportDataSO : StructureDataSO
{
    public AirportExtraConfig extraConfig;

    public override Structure CreateLogic()
    {
        return new Airport(this);
    }
}
