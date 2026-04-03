using UnityEngine;

public class Airport : Structure
{
    AirportExtraConfig extraConfig;

    public Airport(AirportDataSO data)
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
