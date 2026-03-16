using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Double Pulisher", menuName = "Scriptable Objects/Events/Double Publisher")]
public class DoublePublisherSO : ScriptableObject
{
    public UnityAction<double> OnEventRaised;

    public void RaiseEvent(double value)
    {
        OnEventRaised?.Invoke(value);
    }
}
