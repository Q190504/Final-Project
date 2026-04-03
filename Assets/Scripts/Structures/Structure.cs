using UnityEngine;

public abstract class Structure
{
    public abstract void ApplyEffectToCell(GridCell cell);
    public abstract void DisapplyEffectToCell(GridCell cell);

    public abstract void ApplyGlobalEffect();
    public abstract void DisapplyGlobalEffect();
}
