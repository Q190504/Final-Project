using UnityEngine;

public class WaterFactory : Structure
{
    WaterFactoryConfig extraConfig;

    public WaterFactory(WaterFactoryDataSO data)
    {
        extraConfig = data.extraConfig;
    }

    public override void ApplyEffectToCell(GridCell cell)
    {

    }

    public override void ApplyGlobalEffect()
    {

    }

    public override void DisapplyEffectToCell(GridCell cell)
    {

    }

    public override void DisapplyGlobalEffect()
    {

    }
}
