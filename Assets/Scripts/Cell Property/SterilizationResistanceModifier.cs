using System;

public class SterilizationResistanceModifier
{
    public string Id; // unique
    public float Value;
    public SterilizationResistanceAdditiveSourceType SourceType;
    public ModifierType ModifierType;

    public SterilizationResistanceModifier(float value, SterilizationResistanceAdditiveSourceType sourceType, ModifierType modifierType)
    {
        Id = Guid.NewGuid().ToString();
        Value = value;
        SourceType = sourceType;
        ModifierType = modifierType;
    }
}

public enum SterilizationResistanceAdditiveSourceType
{
    Unkown,
    Surface,
    SurfaceUpgrade_HavingInfectedNeighbors,
    SurfaceUpgrade_InfectedForALongTime,
    Air,
    Water,
    SurfaceUpgrade_HasWater,
    Carrier,
    Skill,
    Event,
}
