using System;

public class InfectionResistanceModifier
{
    public string Id; // unique
    public int Value;
    public InfectionResistanceAdditiveSourceType SourceType;

    public InfectionResistanceModifier(int value, InfectionResistanceAdditiveSourceType sourceType)
    {
        Id = Guid.NewGuid().ToString();
        Value = value;
        SourceType = sourceType;
    }
}

public enum InfectionResistanceAdditiveSourceType
{
    CellStage,
    Lockdown,
    Vaccine,
    Hospital,
    Skill,
    Event,
}