using UnityEngine;

public class ConfirmExitGameButton : BaseButton
{
    [SerializeField] private VoidPublisherSO exitGameSO;

    public void OnButtonPressed()
    {
        PlayClickSound();
        exitGameSO.RaiseEvent();
    }
}
