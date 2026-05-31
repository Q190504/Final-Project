using UnityEngine;

[System.Serializable]
public struct AirExtraConfig
{
    [Range(0f, 1f), Tooltip("Factor by which infection power decays with distance.")]
    public float distanceDecayFactor;
}

[CreateAssetMenu(fileName = "Air Spread Data", menuName = "Scriptable Objects/Spread Method/Air")]
public class AirSpreadDataSO : SpreadMethodDataSO
{
    public AirExtraConfig extraConfig;

    public override ISpreadMethod CreateMethod(SpreadMethodContext context)
    {
        return new AirSpreadMethod(context, this);
    }

    public override SpreadMethodRuntimeData CreateRuntimeData()
    {
        return new AirRuntimeData(this);
    }
}