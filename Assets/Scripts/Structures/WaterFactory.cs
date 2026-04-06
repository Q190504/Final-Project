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
        int increaseInfectionLevel = Mathf.RoundToInt(cell.Stats.infectionLevel * extraConfig.increasedInfectionLevelPercentWhenTakenDown);

        cell.Stats.UpdateInfectionLevel(increaseInfectionLevel);
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
