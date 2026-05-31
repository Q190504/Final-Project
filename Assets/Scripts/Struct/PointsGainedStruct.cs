using UnityEngine;

public struct PointsGainedStruct
{
    public int evolutionPoints;
    public int infectionPoints;

    public PointsGainedStruct(int evolutionPoints = 0, int infectionPoints = 0)
    {
        this.evolutionPoints = evolutionPoints;
        this.infectionPoints = infectionPoints;
    }
}
