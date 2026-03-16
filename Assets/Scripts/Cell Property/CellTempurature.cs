using UnityEngine;

public class CellTempurature
{
    public TempuratureType type;

    [Header("Base Gameplay Values")]
    public float priorityToHuman;
    public PriorityToMethods priorityToMethods;

    public CellTempurature()
    {
        type = TempuratureType.None;
        priorityToHuman = 0f;
        priorityToMethods.surfacePriority = 0f;
        priorityToMethods.airPriority = 0f;
        priorityToMethods.waterPriority = 0f;
        priorityToMethods.carrierPriority = 0f;
    }

    public void SetTempurature(TempuratureType tempuratureType)
    {
        type = tempuratureType;

        TempuratureData tempuratureData = CellPropertyManager.Instance.GetTempuratureData(tempuratureType);
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
            foreach (TempuratureData tempuratureData in CellPropertyManager.Instance.GetTempuratureDatas())
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
        foreach (TempuratureData tempuratureData in CellPropertyManager.Instance.GetTempuratureDatas())
        {
            if (tempuratureData != null && tempuratureData.minTempuratureValue <= value
                && value <= tempuratureData.maxTempuratureValue)
            {
                type = tempuratureData.type;
                return;
            }
        }

        type = TempuratureType.None;
    }
}
