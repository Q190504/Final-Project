using UnityEngine;

public class CloseConfirmExitGamePanelButton : BaseButton
{
    [SerializeField] private BoolPublisherSO closeConfirmExitGamePanel;

    public void OnButtonPressed()
    {
        PlayClickSound();
        closeConfirmExitGamePanel.RaiseEvent(false);
    }
}
