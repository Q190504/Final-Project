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

    [Header("Vaccine Settings")]
    [SerializeField] private BoolPublisherSO onGameEndedSO;

    public float Progress { get; private set; }
    public VaccineDevelopmentStage Stage { get; private set; }

    private InfectionResistanceModifier infectionResistanceModifier;

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
        infectionResistanceModifier = null;
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

            onGameEndedSO.RaiseEvent(false);

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

        if (Stage != VaccineDevelopmentStage.NotStarted)
        {
            uiManager.SetVaccineProgress(Progress, amount > 0);
            int resistanceAdditive = CalculateInfectionResistanceAdditive();
            if (infectionResistanceModifier == null)
            {
                infectionResistanceModifier = new InfectionResistanceModifier(resistanceAdditive,
                    InfectionResistanceAdditiveSourceType.Vaccine, ModifierType.Additive);

                mapManager.AddInfectionResistanceByVaccineToMap(infectionResistanceModifier);
            }
            else
                infectionResistanceModifier.Value = resistanceAdditive;
        }
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
