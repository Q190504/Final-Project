using System.Collections.Generic;
using UnityEngine;

public class ActionInstance
{
    public HumanAction Action;
    public List<GridCell> Cells;
    public float RawUtility;
    public float NormalizedUtility;
    public float FinalScore;

    public bool IsValid = true;

    public ActionInstance(HumanAction action)
    {
        Action = action;
    }

    public ActionInstance(HumanAction action, List<GridCell> cells, float rawUtility, float normalizedUtility)
    {
        Action = action;
        Cells = cells;
        RawUtility = rawUtility;
        NormalizedUtility = normalizedUtility;
        IsValid = true;
    }
}