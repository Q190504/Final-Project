using UnityEngine;

public class Hospital : Structure
{
    HospitalExtraConfig extraConfig;

    public Hospital(HospitalDataSO data)
    {
        extraConfig = data.extraConfig;
    }

    public override void ApplyEffectToCell(GridCell cell)
    {
        cell.Stats.UpdateCurrentlInfectionResistance(extraConfig.inreasedInfectionResistance);
    }

    public override void DisapplyEffectToCell(GridCell cell)
    {
        cell.Stats.UpdateCurrentlInfectionResistance(-extraConfig.inreasedInfectionResistance);
    }

    public override void ApplyGlobalEffect()
    {

    }

    public override void DisapplyGlobalEffect()
    {

    }
}
