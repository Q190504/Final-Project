using UnityEngine;

public class CellTemperature
{
    public TemperatureType type;

    [Header("Base Gameplay Values")]
    public float priorityToHuman;
    public PriorityToMethods priorityToMethods;

    public CellTemperature()
    {
        type = TemperatureType.None;
        priorityToHuman = 0f;
        priorityToMethods.surfacePriority = 0f;
        priorityToMethods.airPriority = 0f;
        priorityToMethods.waterPriority = 0f;
        priorityToMethods.carrierPriority = 0f;
    }

    public void SetTempurature(TemperatureType tempuratureType)
    {
        type = tempuratureType;

        TemperatureData tempuratureData = PropertyDataManager.Instance.GetTempuratureData(tempuratureType);
        if (tempuratureData != null)
        {
            priorityToHuman = tempuratureData.priorityToHuman;
            priorityToMethods = tempuratureData.basePriorityToMethods;
            return;
        }
    }

    public void SetTempurature(EnvironmentData environmentData)
    {
        if (environmentData != null)
        {
            foreach (TemperatureData tempuratureData in PropertyDataManager.Instance.GetTempuratureDatas())
            {
                if (tempuratureData != null && tempuratureData.type == environmentData.tempuratureType)
                {
                    priorityToHuman = tempuratureData.priorityToHuman;
                    priorityToMethods = tempuratureData.basePriorityToMethods;
                    return;
                }
            }
        }
    }

    public void SetTempurature(float value)
    {
        foreach (TemperatureData tempuratureData in PropertyDataManager.Instance.GetTempuratureDatas())
        {
            if (tempuratureData != null && tempuratureData.minTempuratureValue <= value
                && value <= tempuratureData.maxTempuratureValue)
            {
                type = tempuratureData.type;
                return;
            }
        }

        type = TemperatureType.None;
    }
}
