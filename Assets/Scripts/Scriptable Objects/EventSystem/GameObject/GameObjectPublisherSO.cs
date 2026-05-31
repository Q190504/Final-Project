using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Game Object Pulisher", menuName = "Scriptable Objects/Events/Game Object Publisher")]
public class GameObjectPublisherSO : ScriptableObject
{
    public UnityAction<GameObject> OnEventRaised;

    public void RaiseEvent(GameObject obj)
    {
        OnEventRaised?.Invoke(obj);
    }
}