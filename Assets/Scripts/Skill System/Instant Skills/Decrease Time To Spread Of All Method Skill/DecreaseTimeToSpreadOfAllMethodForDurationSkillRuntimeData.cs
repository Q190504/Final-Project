using UnityEngine;

public class DecreaseTimeToSpreadOfAllMethodForDurationSkillRuntimeData : InstantSkillRuntimeData
{
    public DecreaseTimeToSpreadOfAllMethodForDurationSkillExtraConfig skillExtraConfig;

    public DecreaseTimeToSpreadOfAllMethodForDurationSkillRuntimeData(DecreaseTimeToSpreadOfAllMethodForDurationSkillDataSO dataSO) 
        : base()
    {
        skillExtraConfig = dataSO.skillExtraConfig;
    }
}
