using UnityEngine;
using UnityEngine.Events;

public class IntEventListener : MonoBehaviour
{
    [SerializeField] private UnityEvent<int> EventResponse;
    [SerializeField] private IntPublisherSO publisher;

    private void OnEnable()
    {
        publisher.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        publisher.OnEventRaised -= Respond;
    }

    private void Respond(int value)
    {
        EventResponse?.Invoke(value);
    }
}