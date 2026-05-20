[System.Serializable]
public struct ThreatLevelRange
{
    public float min;
    public float max;

    public bool Contains(float value)
    {
        return value >= min && value < max;
    }
}