using UnityEngine;

public class CellEnvironment
{
    public EnvironmentType currentEnvironmentType;
    public EnvironmentType originalEnvironmentType;

    [Header("Base Gameplay Values")]
    public bool isBlocked;
    public bool canBeLockeddown;
    public bool canHaveStructure;
    public bool canSpawnCarrier;

    public float priorityToHuman;
    public PriorityToMethods priorityToMethods;

    public CellEnvironment()
    {
        currentEnvironmentType = originalEnvironmentType = EnvironmentType.None;
        isBlocked = false;
        canBeLockeddown = false;
        canHaveStructure = false;
        canSpawnCarrier = false;

        priorityToHuman = 0f;
        priorityToMethods.surfacePriority = 0f;
        priorityToMethods.airPriority = 0f;
        priorityToMethods.waterPriority = 0f;
        priorityToMethods.carrierPriority = 0f;
    }

    public CellEnvironment(EnvironmentType environmentType)
    {
        currentEnvironmentType = originalEnvironmentType = environmentType;

        EnvironmentData envData = PropertyDataManager.Instance.GetEnvironmentData(currentEnvironmentType);
        if (envData != null)
        {
            isBlocked = envData.isBlocked;
            canBeLockeddown = envData.canBeLockeddown;
            canHaveStructure = envData.canHaveStructure;
            canSpawnCarrier = envData.canSpawnCarrier;

            priorityToHuman = envData.priorityToHuman;
            priorityToMethods = envData.basePriorityToMethods;
            return;
        }
    }

    public CellEnvironment(PopulationType populationType, TemperatureType tempuratureType)
    {
        foreach (EnvironmentData envData in PropertyDataManager.Instance.GetEnvironmentDatas())
        {
            if (envData != null && envData.populationType == populationType && envData.tempuratureType == tempuratureType)
            {
                currentEnvironmentType = originalEnvironmentType = envData.type;
            }
        }

        currentEnvironmentType = originalEnvironmentType = EnvironmentType.None;
    }

    public EnvironmentType SetEnvironmentType(PopulationType populationType, TemperatureType tempuratureType)
    {
        foreach (EnvironmentData envData in PropertyDataManager.Instance.GetEnvironmentDatas())
        {
            if (envData != null && envData.populationType == populationType && envData.tempuratureType == tempuratureType)
            {
                currentEnvironmentType = originalEnvironmentType = envData.type;
                return currentEnvironmentType;
            }
        }

        return EnvironmentType.None;
    }

    public void SetEnvironmentType(EnvironmentType environmentType)
    {
        originalEnvironmentType = currentEnvironmentType;
        currentEnvironmentType = environmentType;
    }

    public EnvironmentType ReturnToOriginalEnvironment()
    {
        currentEnvironmentType = originalEnvironmentType;
        return currentEnvironmentType;
    }
}
