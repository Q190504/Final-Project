using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New String Pulisher", menuName = "Scriptable Objects/Events/String Publisher")]
public class StringPublisherSO : ScriptableObject
{
    public UnityAction<string> OnEventRaised;

    public void RaiseEvent(string value)
    {
        OnEventRaised?.Invoke(value);
    }
}