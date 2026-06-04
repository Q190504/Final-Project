using UnityEngine;

public class IncreaseInfectionLevelToCellsSkillRuntimeData : TargetSkillRuntimeData
{
    public IncreaseInfectionLevelToCellsSkillExtraConfig skillExtraConfig;

    public IncreaseInfectionLevelToCellsSkillRuntimeData(IncreaseInfectionLevelToCellsSkillDataSO dataSO) : base(dataSO.targetSkillExtraConfig)
    {
        skillExtraConfig = dataSO.skillExtraConfig;
    }
}