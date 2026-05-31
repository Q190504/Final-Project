using UnityEngine;

public class PointsManager : MonoBehaviour
{
    public static PointsManager Instance;

    private int evolutionPoints;
    private int infectionPoints;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    #region Evolution Points

    public bool HaveEnoughEvolutionPoints(int cost)
    {
        return evolutionPoints >= cost;
    }

    public void SpendEvolutionPoints(int cost)
    {
        evolutionPoints -= cost;
        UIManager.Instance.SetEvolutionPointText(evolutionPoints, cost);
    }

    public void AddEvolutionPoints(int amount)
    {
        evolutionPoints += amount;
        UIManager.Instance.SetEvolutionPointText(evolutionPoints, amount);
    }

    #endregion

    #region Infection Points

    public bool HaveEnoughInfectionPoints(int cost)
    {
        return infectionPoints >= cost;
    }

    public void SpendInfectionPoints(int cost)
    {
        int previousPoints = infectionPoints;
        infectionPoints -= cost;
        int diffPoints = infectionPoints - previousPoints;
        UIManager.Instance.SetInfectionPointText(infectionPoints, diffPoints);
    }

    public void AddInfectionPoints(int amount)
    {
        int previousPoints = infectionPoints;
        infectionPoints += amount;
        int diffPoints = infectionPoints - previousPoints;
        UIManager.Instance.SetInfectionPointText(infectionPoints, diffPoints);
    }

    #endregion
}
