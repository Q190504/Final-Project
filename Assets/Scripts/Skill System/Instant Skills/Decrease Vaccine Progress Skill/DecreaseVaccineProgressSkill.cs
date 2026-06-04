using UnityEngine;

public class DecreaseVaccineProgressSkill
    : InstantSkill<DecreaseVaccineProgressSkillDataSO, DecreaseVaccineProgressSkillRuntimeData>

{
    public DecreaseVaccineProgressSkill(DecreaseVaccineProgressSkillDataSO data,
        DecreaseVaccineProgressSkillRuntimeData runtimeData) : base(data, runtimeData)
    {
    }

    public override bool CanCast()
    {
        if (base.CanCast())
        {
            if (VaccineSystem.Instance.Stage == VaccineDevelopmentStage.NotStarted)
            {
                UIManager.Instance.ShowNotification("Vaccine development has not started yet!", 2, Color.red);
                return false;
            }
            else if (VaccineSystem.Instance.Stage == VaccineDevelopmentStage.Developed)
            {
                UIManager.Instance.ShowNotification("Vaccine has already been developed!", 2, Color.red);
                return false;
            }

            return true;
        }

        return false;
    }

    public override void Execute()
    {
        VaccineSystem.Instance.UpdateProgress(-RuntimeData.skillExtraConfig.decreaseVaccineProgressPercent);

        base.Execute();
    }
}

