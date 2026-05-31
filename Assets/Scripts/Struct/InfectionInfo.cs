using UnityEngine;

public struct InfectionInfo
{
    public GridCell cell;

    public int infectionDelta;

    public int disinfectionImmunityTicks;

    #region Surface's Upgrade

    public int minTickToBonusSterilizationResistance;
    public SterilizationResistanceModifier sterilizationResistanceModifierIfInfectedForALongTime;

    #endregion

    #region Water's Upgrade

    public SterilizationResistanceModifier sterilizationResistanceModifierForWaterCell;

    #endregion

    public InfectionInfo(GridCell cell, int infectionDelta)
    {
        this.cell = cell;
        this.infectionDelta = infectionDelta;

        this.disinfectionImmunityTicks = 0;
        this.minTickToBonusSterilizationResistance = 0;
        this.sterilizationResistanceModifierIfInfectedForALongTime = null;

        this.sterilizationResistanceModifierForWaterCell = null;
    }

    public InfectionInfo(GridCell cell, int infectionDelta, int disinfectionImmunityTicks,
        int minTickToBonusSterilizationResistance,
        SterilizationResistanceModifier sterilizationResistanceModifierIfInfectedForALongTime)
    {
        this.cell = cell;
        this.infectionDelta = infectionDelta;
        this.disinfectionImmunityTicks = disinfectionImmunityTicks;

        this.minTickToBonusSterilizationResistance = minTickToBonusSterilizationResistance;
        this.sterilizationResistanceModifierIfInfectedForALongTime = sterilizationResistanceModifierIfInfectedForALongTime;

        this.sterilizationResistanceModifierForWaterCell = null;
    }

    public InfectionInfo(GridCell cell, int infectionDelta, SterilizationResistanceModifier sterilizationResistanceModifierForWaterCell)
    {
        this.cell = cell;
        this.infectionDelta = infectionDelta;
        this.sterilizationResistanceModifierForWaterCell = sterilizationResistanceModifierForWaterCell;

        this.disinfectionImmunityTicks = 0;
        this.minTickToBonusSterilizationResistance = 0;
        this.sterilizationResistanceModifierIfInfectedForALongTime = null;
    }
}