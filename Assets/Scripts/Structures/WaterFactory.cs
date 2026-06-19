using UnityEngine;

public class WaterFactory : Structure
{
    WaterFactoryConfig extraConfig;

    private SpreadMethodManager spreadMethodManager;

    public WaterFactory(WaterFactoryDataSO data)
    {
        extraConfig = data.extraConfig;
        spreadMethodManager = SpreadMethodManager.Instance;
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
        int increaseInfectionLevel = Mathf.FloorToInt(cell.Stats.infectionLevel * extraConfig.increasedInfectionLevelPercentWhenTakenDown);

        cell.Stats.UpdateInfectionLevel(increaseInfectionLevel, true);
    }

    public override void DisapplyEffectToCellWhenEnabled(GridCell cell)
    {

    }

    public override void ApplyTickEffectToCell(GridCell cell)
    {

    }

    public override void ApplyGlobalEffectWhenDisabled()
    {
        WaterRuntimeData waterRuntimeData = spreadMethodManager.GetRuntimeData(SpreadMethodType.Water) as WaterRuntimeData;
        waterRuntimeData.extraConfig.totalDisabledWaterFactory++;
    }

    public override void DisapplyGlobalEffectWhenEnabled()
    {
        WaterRuntimeData waterRuntimeData = spreadMethodManager.GetRuntimeData(SpreadMethodType.Water) as WaterRuntimeData;
        waterRuntimeData.extraConfig.totalDisabledWaterFactory--;

        if (waterRuntimeData.extraConfig.totalDisabledWaterFactory < 0)
            waterRuntimeData.extraConfig.totalDisabledWaterFactory = 0;
    }
}
