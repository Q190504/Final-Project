using UnityEngine;

public abstract class Structure
{
    public abstract void ApplyEffectToCellWhenEnabled(GridCell cell);
    public abstract void DisapplyEffectToCellWhenEnabled(GridCell cell);
    public abstract void ApplyEffectToCellWhenDisabled(GridCell cell);
    public abstract void DisapplyEffectToCellWhenDisabled(GridCell cell);

    public abstract void ApplyGlobalEffectWhenEnabled();
    public abstract void DisapplyGlobalEffectWhenDisabled();

    public abstract void ApplyTickEffect();
}
