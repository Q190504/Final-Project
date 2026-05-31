using UnityEngine;

public class Airport : Structure
{
    AirportExtraConfig extraConfig;

    public Airport(AirportDataSO data)
    {
        extraConfig = data.extraConfig;
    }

    public override void ApplyEffectToCellWhenEnabled(GridCell cell)
    {

    }

    public override void DisapplyEffectToCellWhenDisabled(GridCell cell)
    {

    }

    public override void ApplyGlobalEffectWhenEnabled()
    {

    }

    public override void DisapplyGlobalEffectWhenDisabled()
    {

    }

    public override void ApplyTickGlobalEffect()
    {

    }

    public override void ApplyEffectToCellWhenDisabled(GridCell cell)
    {
        cell.Stats.UpdateIncreaseCarrierSpreadChance(extraConfig.increaseCarrierSpawningPercent);
    }

    public override void DisapplyEffectToCellWhenEnabled(GridCell cell)
    {
        cell.Stats.UpdateIncreaseCarrierSpreadChance(-extraConfig.increaseCarrierSpawningPercent);
    }

    public override void ApplyTickEffectToCell(GridCell cell)
    {

    }

    public override void ApplyGlobalEffectWhenDisabled()
    {

    }

    public override void DisapplyGlobalEffectWhenEnabled()
    {

    }
}
