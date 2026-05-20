using UnityEngine;

public class CellPopulation
{
    public PopulationType type = PopulationType.None;

    [Header("Base Gameplay Values")]
    public float weight = 0f;

    public float priorityToHuman = 0f;
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
        PopulationData populationData = PropertyDataManager.Instance.GetPopulationData(populationType);
        if (populationData != null)
        {
            SetData(populationData);
            return;
        }
    }

    public void SetPopulation(EnvironmentData environmentData)
    {
        if (environmentData != null)
        {
            PopulationData populationData = PropertyDataManager.Instance.GetPopulationData(environmentData.populationType);
            if (populationData != null)
            {
                SetData(populationData);
                return;
            }
        }
    }

    public void SetPopulation(float value)
    {
        foreach (PopulationData populationData in PropertyDataManager.Instance.GetPopulationDatas())
        {
            if (populationData != null && populationData.minPopulationValue <= value
                && value <= populationData.maxPopulationValue)
            {
                SetData(populationData);
                return;
            }
        }

        SetData(PropertyDataManager.Instance.GetPopulationData(PopulationType.None));
    }

    private void SetData(PopulationData populationData)
    {
        if (populationData != null)
        {
            float previousWeight = weight;

            type = populationData.type;
            weight = populationData.weight;
            priorityToHuman = populationData.priorityToHuman;
            populationData.basePriorityToMethods = priorityToMethods;

            float deltaWeight = weight - previousWeight;

            MapManager.Instance.UpdateTotalPopulation(deltaWeight);
        }
    }
}
