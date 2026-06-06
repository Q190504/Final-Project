using UnityEngine;

public class IncreaseInfectionLevelToCellsSkillRuntimeData : MultiTargetSkillRuntimeData
{
    public IncreaseInfectionLevelToCellsSkillExtraConfig skillExtraConfig;

    public IncreaseInfectionLevelToCellsSkillRuntimeData(IncreaseInfectionLevelToCellsSkillDataSO dataSO) 
        : base(dataSO.multiTargetSkillExtraConfig, dataSO.targetSkillExtraConfig)
    {
        skillExtraConfig = dataSO.skillExtraConfig;
    }
}