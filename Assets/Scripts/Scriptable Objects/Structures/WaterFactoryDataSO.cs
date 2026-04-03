using UnityEngine;

[System.Serializable]
public struct WaterFactoryConfig
{

}

[CreateAssetMenu(fileName = "Water Factory Data", menuName = "Scriptable Objects/Cell Property/Structure Data")]
public class WaterFactoryDataSO : StructureDataSO
{
    public WaterFactoryConfig extraConfig;

    public override Structure CreateLogic()
    {
        return new WaterFactory(this);
    }
}
