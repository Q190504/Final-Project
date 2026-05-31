using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Float Pulisher", menuName = "Scriptable Objects/Events/Float Publisher")]
public class FloatPublisherSO : ScriptableObject
{
    public UnityAction<float> OnEventRaised;
    
    public void RaiseEvent(float value)
    {
        OnEventRaised?.Invoke(value);
    }
}