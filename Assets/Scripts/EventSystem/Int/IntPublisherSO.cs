using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Int Pulisher", menuName = "Scriptable Objects/Events/Int Publisher")]
public class IntPublisherSO : ScriptableObject
{
    public UnityAction<int> OnEventRaised;

    public void RaiseEvent(int value)
    {
        OnEventRaised?.Invoke(value);
    }
}
