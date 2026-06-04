using UnityEngine;

public abstract class SkillDataSO : ScriptableObject
{
    public SkillType type;
    public string displayName;
    [TextArea]
    public string description;
    public Sprite mainIcon;
    public Sprite secondaryIcon;
    public Color secondaryIconColor = Color.white;

    public int infectionPointCost;
    public int cooldownTicks;

    public SkillCastType castType;

    public abstract IBaseSkill CreateSkill(SkillRuntimeData runtimeData);
    public abstract SkillRuntimeData CreateRuntimeData();
}