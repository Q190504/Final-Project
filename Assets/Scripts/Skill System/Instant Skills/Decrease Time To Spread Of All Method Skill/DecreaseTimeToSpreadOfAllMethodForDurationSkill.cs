using System.Collections.Generic;
using UnityEngine;

public class DecreaseTimeToSpreadOfAllMethodForDurationSkill
    : InstantSkill<DecreaseTimeToSpreadOfAllMethodForDurationSkillDataSO, DecreaseTimeToSpreadOfAllMethodForDurationSkillRuntimeData>

{
    public DecreaseTimeToSpreadOfAllMethodForDurationSkill(DecreaseTimeToSpreadOfAllMethodForDurationSkillDataSO data,
        DecreaseTimeToSpreadOfAllMethodForDurationSkillRuntimeData runtimeData) : base(data, runtimeData)
    {
    }

    public override void Execute()
    {
        // Decrease the time to spread of all methods immediately
        TimeManager.Instance.ScheduleEvent(0,
            () =>
            {
                List<SpreadMethodRuntimeData> methodRuntimeDatas = SpreadMethodManager.Instance.GetAllSpreadMethodRuntimeDatas();

                foreach (SpreadMethodRuntimeData methodRuntimeData in methodRuntimeDatas)
                {
                    methodRuntimeData.DecreaseTickToSpreadPercent(RuntimeData.skillExtraConfig.decreaseTimeToSpreadPercentForEachMethod);
                }

                UIManager.Instance.UpdateSpreadMethodCooldownUI(methodRuntimeDatas);
            }, EventPriority.SkillExecution);

        float endTime = RuntimeData.skillExtraConfig.duration;

        // Schedule an event to restore the original time to spread after the duration ends
        TimeManager.Instance.ScheduleEvent(endTime,
            () =>
            {
                List<SpreadMethodRuntimeData> methodRuntimeDatas = SpreadMethodManager.Instance.GetAllSpreadMethodRuntimeDatas();

                foreach (SpreadMethodRuntimeData methodRuntimeData in SpreadMethodManager.Instance.GetAllSpreadMethodRuntimeDatas())
                {
                    methodRuntimeData.IncreaseTickToSpreadPercent(RuntimeData.skillExtraConfig.decreaseTimeToSpreadPercentForEachMethod);
                }

                UIManager.Instance.UpdateSpreadMethodCooldownUI(methodRuntimeDatas);
            }, EventPriority.SkillExecution);

        base.Execute();
    }
}
