using UnityEngine;

public class DecreaseVaccineProgressSkillRuntimeData : InstantSkillRuntimeData
{
    public DecreaseVaccineProgressSkillExtraConfig skillExtraConfig;

    public DecreaseVaccineProgressSkillRuntimeData(DecreaseVaccineProgressSkillDataSO dataSO) : base()
    {
        skillExtraConfig = dataSO.skillExtraConfig;
    }
}

