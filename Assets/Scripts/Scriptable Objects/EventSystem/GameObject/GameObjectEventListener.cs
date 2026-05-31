using UnityEngine;
using UnityEngine.Events;

public class GameObjectEventListener : MonoBehaviour
{
    [SerializeField] private UnityEvent<GameObject> EventResponse;
    [SerializeField] private GameObjectPublisherSO publisher;

    private void OnEnable()
    {
        publisher.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        publisher.OnEventRaised -= Respond;
    }

    private void Respond(GameObject obj)
    {
        EventResponse?.Invoke(obj);
    }
}