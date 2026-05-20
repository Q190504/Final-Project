using System;

public class CellDetectionModifier
{
    public string Id; // unique
    public float Value;
    public DetectionAdditiveSourceType SourceType;

    public CellDetectionModifier(float value, DetectionAdditiveSourceType sourceType)
    {
        Id = Guid.NewGuid().ToString();
        Value = value;
        SourceType = sourceType;
    }
}

public enum DetectionAdditiveSourceType
{
    Lockdown,
    Skill,
    Event,
}