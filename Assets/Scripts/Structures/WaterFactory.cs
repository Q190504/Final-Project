using UnityEngine;

public class WaterFactory : Structure
{
    WaterFactoryConfig extraConfig;

    public WaterFactory(WaterFactoryDataSO data)
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

    public override void ApplyTickEffect()
    {

    }

    public override void ApplyEffectToCellWhenDisabled(GridCell cell)
    {
        int increaseInfectionLevel = Mathf.RoundToInt(cell.Stats.infectionLevel * extraConfig.increasedInfectionLevelPercentWhenTakenDown);

        cell.Stats.UpdateInfectionLevel(increaseInfectionLevel);
    }

    public override void DisapplyEffectToCellWhenEnabled(GridCell cell)
    {

    }
}
