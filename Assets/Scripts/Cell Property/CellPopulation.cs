using UnityEngine;

public class CellPopulation
{
    public PopulationType type;

    [Header("Base Gameplay Values")]
    public float weight;

    public float priorityToHuman;
    public PriorityToMethods priorityToMethods;

    public CellPopulation()
    {
        type = PopulationType.None;
        weight = 0f;
        priorityToHuman = 0f;
        priorityToMethods.surfacePriority = 0f;
        priorityToMethods.airPriority = 0f;
        priorityToMethods.waterPriority = 0f;
        priorityToMethods.carrierPriority = 0f;
    }

    public void SetPopulation(PopulationType populationType)
    {
        type = populationType;

        PopulationData populationData = CellPropertyManager.Instance.GetPopulationData(type);
        if (populationData != null)
        {
            weight = 0f;
            priorityToMethods.surfacePriority = populationData.weight;
            priorityToHuman = populationData.priorityToHuman;
            populationData.basePriorityToMethods = priorityToMethods;
            return;
        }
    }

    public void SetPopulation(EnvironmentData environmentData)
    {
        if (environmentData != null)
        {
            foreach (PopulationData populationData in CellPropertyManager.Instance.GetPopulationDatas())
            {
                if (populationData != null && populationData.type == environmentData.populationType)
                {
                    weight = 0f;
                    priorityToMethods.surfacePriority = populationData.weight;
                    priorityToHuman = populationData.priorityToHuman;
                    populationData.basePriorityToMethods = priorityToMethods;
                    return;
                }
            }
        }
    }

    public void SetPopulation(float value)
    {
        foreach (PopulationData populationData in CellPropertyManager.Instance.GetPopulationDatas())
        {
            if (populationData != null && populationData.minPopulationValue <= value
                && value <= populationData.maxPopulationValue)
            {
                type = populationData.type;
                return;
            }
        }

        type = PopulationType.None;
    }
}
