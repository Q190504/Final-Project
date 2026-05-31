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
        if (cell.Stats.canBeSterilized)
            cell.Stats.UpdateInfectionLevel(-extraConfig.sterilizeAmountWhenPlaced);

        hospitalInfectionResistanceModifier = new(extraConfig.additionalInfectionResistance, InfectionResistanceAdditiveSourceType.Hospital, ModifierType.Additive);
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

    public override void ApplyTickGlobalEffect()
    {

    }

    public override void ApplyEffectToCellWhenDisabled(GridCell cell)
    {

    }

    public override void DisapplyEffectToCellWhenEnabled(GridCell cell)
    {

    }

    public override void ApplyTickEffectToCell(GridCell cell)
    {
        if (cell.Stats.canBeSterilized)
            cell.Stats.UpdateInfectionLevel(-extraConfig.sterilizeAmountEachTick);
    }

    public override void ApplyGlobalEffectWhenDisabled()
    {

    }

    public override void DisapplyGlobalEffectWhenEnabled()
    {

    }
}
