using UnityEngine;

public class VaccineResearchCenter : Structure
{
    VaccineResearchCenterExtraConfig extraConfig;

    public VaccineResearchCenter(VaccineResearchCenterDataSO data)
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
        VaccineSystem.Instance.UpdateProgress(extraConfig.vaccineProgressIncrementPercentPerTick);
    }

    public override void ApplyEffectToCellWhenDisabled(GridCell cell)
    {

    }

    public override void DisapplyEffectToCellWhenEnabled(GridCell cell)
    {

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
