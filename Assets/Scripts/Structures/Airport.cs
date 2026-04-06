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
        cell.Stats.UpdateIncreaseCarrierSpreadChance(extraConfig.increaseCarrierSpawningPercent);
    }

    public override void DisapplyEffectToCell(GridCell cell)
    {
        cell.Stats.UpdateIncreaseCarrierSpreadChance(-extraConfig.increaseCarrierSpawningPercent);
    }

    public override void ApplyGlobalEffect()
    {

    }

    public override void DisapplyGlobalEffect()
    {

    }
}
