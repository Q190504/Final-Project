using System;

public class InfectionResistanceModifier
{
    public string Id; // unique
    public float Value;
    public InfectionResistanceAdditiveSourceType SourceType;
    public ModifierType ModifierType;

    public InfectionResistanceModifier(float value, InfectionResistanceAdditiveSourceType sourceType, ModifierType modifierType)
    {
        Id = Guid.NewGuid().ToString();
        Value = value;
        SourceType = sourceType;
        ModifierType = modifierType;
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