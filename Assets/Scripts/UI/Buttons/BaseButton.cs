using UnityEngine;

public class BaseButton : MonoBehaviour
{
    [SerializeField] protected VoidPublisherSO playClickSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayClickSound()
    {
        playClickSound.RaiseEvent();
    }
}
