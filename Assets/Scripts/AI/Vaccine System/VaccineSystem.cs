using UnityEngine;

public enum VaccineDevelopmentStage
{
    NotStarted,
    Researching,
    Developed
}

public class VaccineSystem : MonoBehaviour
{
    public static VaccineSystem Instance;

    [Header("Vaccine Settings")]
    [SerializeField, Range(0f, 1f), Tooltip("The amount of progress required for each step of vaccine development.")]
    private float progressPerStep;
    [SerializeField, Range(0, 100), Tooltip("The amount of infection resistance gained for each step of vaccine development.")]
    private int infectionResistancePerStep;
    private InfectionResistanceModifier infectionResistanceModifier;

    public float Progress { get; private set; }
    public VaccineDevelopmentStage Stage { get; private set; }

    private MapManager mapManager;
    private UIManager uiManager;
    public VaccineSystem()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Debug.LogWarning("Multiple instances of VaccineSystem detected! Destroying duplicate.");
            Destroy(this);
        }
    }

    public void Init()
    {
        Progress = 0f;
        Stage = VaccineDevelopmentStage.NotStarted;
        mapManager = MapManager.Instance;
        uiManager = UIManager.Instance;
        infectionResistanceModifier = new InfectionResistanceModifier(0, InfectionResistanceAdditiveSourceType.Vaccine);
    }

    public void UpdateProgress(float amount)
    {
        if (Stage == VaccineDevelopmentStage.Developed)
            return;

        Progress += amount;
        Progress = Mathf.Clamp01(Progress);

        if (Progress >= 1f)
        {
            Progress = 1f;
            Stage = VaccineDevelopmentStage.Developed;
            Debug.Log("Vaccine developed!");

            return;
        }
        else if (Progress > 0f && Stage == VaccineDevelopmentStage.NotStarted)
        {
            Stage = VaccineDevelopmentStage.Researching;
            Debug.Log("Vaccine research started!");
        }
        else if (Progress == 0)
        {
            Stage = VaccineDevelopmentStage.NotStarted;
            Debug.Log("Vaccine research reset to not started.");
        }

        uiManager.SetVaccineProgress(Progress);
        int resistanceAdditive = CalculateInfectionResistanceAdditive();
        mapManager.UpdateMapInfectionResistanceByVaccine(infectionResistanceModifier, resistanceAdditive);
    }

    private int CalculateInfectionResistanceAdditive()
    {
        // Every progressPerStep, the infection resistance increases by infectionResistancePerStep
        return Mathf.FloorToInt(Progress / progressPerStep) * infectionResistancePerStep;
    }

    public int CalculateInfectionResistanceIncreasementIfExecute(float vaccineProgressIncreasement)
    {
        return Mathf.FloorToInt(Progress + vaccineProgressIncreasement / progressPerStep) * infectionResistancePerStep;
    }
}
