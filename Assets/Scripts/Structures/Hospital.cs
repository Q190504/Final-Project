using System.Collections.Generic;
using UnityEngine;

public class Hospital : Structure
{
    HospitalExtraConfig extraConfig;

    private InfectionResistanceModifier hospitalInfectionResistanceModifier;

    public Hospital(HospitalDataSO data)
    {
        extraConfig = data.extraConfig;
    }

    public override void ApplyEffectToCellWhenEnabled(GridCell cell)
    {
        cell.Stats.SetInfectionLevel(0);

        hospitalInfectionResistanceModifier = new(extraConfig.inreasedInfectionResistance, InfectionResistanceAdditiveSourceType.Hospital);
        cell.Stats.AddInfectionResistanceModifier(hospitalInfectionResistanceModifier);
    }

    public override void DisapplyEffectToCellWhenDisabled(GridCell cell)
    {
        if (hospitalInfectionResistanceModifier != null)
            cell.Stats.RemoveInfectionResistanceModifier(hospitalInfectionResistanceModifier);
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

    }

    public override void DisapplyEffectToCellWhenEnabled(GridCell cell)
    {

    }
}
