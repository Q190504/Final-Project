using UnityEngine;

public class VaccineResearchCenter : Structure
{
    VaccineResearchCenterExtraConfig extraConfig;

    public VaccineResearchCenter(VaccineResearchCenterDataSO data)
    {
        extraConfig = data.extraConfig;
    }

    public override void ApplyEffectToCell(GridCell cell)
    {

    }

    public override void DisapplyEffectToCell(GridCell cell)
    {

    }

    public override void ApplyGlobalEffect()
    {

    }

    public override void DisapplyGlobalEffect()
    {

    }
}
