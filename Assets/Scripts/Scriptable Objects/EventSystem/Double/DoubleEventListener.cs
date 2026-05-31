using UnityEngine;
using UnityEngine.Events;

public class DoubleEventListener : MonoBehaviour
{
    [SerializeField] private UnityEvent<double> EventResponse;
    [SerializeField] private DoublePublisherSO publisher;

    private void OnEnable()
    {
        publisher.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        publisher.OnEventRaised -= Respond;
    }

    private void Respond(double value)
    {
        EventResponse?.Invoke(value);
    }
}
