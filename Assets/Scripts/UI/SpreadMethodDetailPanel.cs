using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpreadMethodDetailPanel : MonoBehaviour
{
    [SerializeField] private SpreadMethodInfoEntryUI spreadMethodInfoUIEntryPrefab;
    [SerializeField] private Transform container;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private float scrollSpeed = 0.1f;
    [SerializeField] private float delayScrollTime = 0.8f;

    [Header("Entries")]
    [SerializeField] private SpreadMethodInfoEntryUI tickToSpreadEntry;
    [SerializeField] private SpreadMethodInfoEntryUI infectionPowerEntry;
    [SerializeField] private SpreadMethodInfoEntryUI sterilizationResistanceEntry;
    [SerializeField] private SpreadMethodInfoEntryUI spreadChanceWhenBlockedEntry;
    [SerializeField] private SpreadMethodInfoEntryUI minDistanceEntry;
    [SerializeField] private SpreadMethodInfoEntryUI maxDistanceEntry;
    [SerializeField] private SpreadMethodInfoEntryUI InfectionModifierHotTemperatureEntry;
    [SerializeField] private SpreadMethodInfoEntryUI InfectionModifierNormalTemperatureEntry;
    [SerializeField] private SpreadMethodInfoEntryUI InfectionModifierColdTemperatureEntry;
    [SerializeField] private SpreadMethodInfoEntryUI InfectionModifierHighPopulationEntry;
    [SerializeField] private SpreadMethodInfoEntryUI InfectionModifierMediumPopulationEntry;
    [SerializeField] private SpreadMethodInfoEntryUI InfectionModifierLowPopulationEntry;

    private float delayRemains = 0;

    private void Update()
    {
        if (delayRemains > 0)
            delayRemains -= Time.deltaTime;
        else
        {
            scrollRect.verticalNormalizedPosition -= scrollSpeed * Time.deltaTime;

            // When reaching the bottom, return to the top
            if (scrollRect.verticalNormalizedPosition <= 0f)
            {
                StartCoroutine(ReturnToTheTop());
            }
        }
    }

    private IEnumerator ReturnToTheTop()
    {
        yield return new WaitForSeconds(delayScrollTime);
        scrollRect.verticalNormalizedPosition = 1f;
        yield return new WaitForSeconds(delayScrollTime);
    }

    public void SetInfo(SpreadMethodRuntimeData spreadMethodRuntimeData = null)
    {
        if (spreadMethodRuntimeData != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
            delayRemains = delayScrollTime;

            // ===== BASIC =====
            SetupEntry(tickToSpreadEntry, $"Days To Spread: {spreadMethodRuntimeData.GetFinalTickToSpread()}.");
            SetupEntry(infectionPowerEntry, $"Infection Power: {spreadMethodRuntimeData.GetBaseInfectionPower()}.");
            SetupEntry(sterilizationResistanceEntry, $"Sterilization Resistance: {spreadMethodRuntimeData.GetFinalSterilizationResistance()}.");
            SetupEntry(minDistanceEntry, $"Min Distance: {spreadMethodRuntimeData.GetFinalMinDistance()}.");
            SetupEntry(maxDistanceEntry, $"Max Distance: {spreadMethodRuntimeData.GetFinalMaxDistance()}.");

            // ===== POPULATION =====
            foreach (PopulationModifierEntry populationModifierEntry in spreadMethodRuntimeData.GetPopulationModifiers())
            {
                switch (populationModifierEntry.population)
                {
                    case PopulationType.High:
                        SetupEntry(InfectionModifierHighPopulationEntry, $"Infection Modifier (High Population): {populationModifierEntry.multiplier}");
                        break;
                    case PopulationType.Medium:
                        SetupEntry(InfectionModifierMediumPopulationEntry, $"Infection Modifier (Medium Population): {populationModifierEntry.multiplier}");
                        break;
                    case PopulationType.Low:
                        SetupEntry(InfectionModifierLowPopulationEntry, $"Infection Modifier (Low Population): {populationModifierEntry.multiplier}");
                        break;
                    default:
                        break;
                }
            }

            // ===== TEMPERATURE =====
            foreach (TemperatureModifierEntry temperatureModifierEntry in spreadMethodRuntimeData.GetTemperatureModifiers())
            {
                switch (temperatureModifierEntry.temperature)
                {
                    case TemperatureType.Hot:
                        SetupEntry(InfectionModifierHotTemperatureEntry, $"Infection Modifier (Hot Temperature): {temperatureModifierEntry.multiplier}");
                        break;
                    case TemperatureType.Normal:
                        SetupEntry(InfectionModifierNormalTemperatureEntry, $"Infection Modifier (Normal Temperature): {temperatureModifierEntry.multiplier}");
                        break;
                    case TemperatureType.Cold:
                        SetupEntry(InfectionModifierColdTemperatureEntry, $"Infection Modifier (Cold Temperature): {temperatureModifierEntry.multiplier}");
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void SetupEntry(SpreadMethodInfoEntryUI entry, string text)
    {
        if (entry == null) return;

        entry.gameObject.SetActive(true);
        entry.Setup(text);
    }

    private void SetActiveSafe(SpreadMethodInfoEntryUI entry, bool state)
    {
        if (entry == null) return;

        entry.gameObject.SetActive(state);
    }

    private string Safe(string value)
    {
        return string.IsNullOrEmpty(value) ? "Null" : value;
    }
}
