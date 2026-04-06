using UnityEngine;

[System.Serializable]
public struct WaterFactoryConfig
{
    [Range (0,1)]
    public float increasedInfectionLevelPercentWhenTakenDown;
}

[CreateAssetMenu(fileName = "Water Factory Data", menuName = "Scriptable Objects/Cell Property/Structure Data/Water Factory Data")]
public class WaterFactoryDataSO : StructureDataSO
{
    public WaterFactoryConfig extraConfig;

    public override Structure CreateLogic()
    {
        return new WaterFactory(this);
    }
}
