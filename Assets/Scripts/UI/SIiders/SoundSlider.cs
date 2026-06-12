using UnityEngine;
using UnityEngine.UI;

public enum SoundSliderType
{
    SFX,
    BGM,
}

public class SoundSlider : MonoBehaviour
{
    [SerializeField] SoundSliderType type;
    [SerializeField] Slider slider;
    [SerializeField] FloatPublisherSO onSliderValueChangedSO;

    private void Start()
    {
        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            if (type == SoundSliderType.SFX)
                SetSliderValue(audioManager.GetSFXValueForSlider());
            else
                SetSliderValue(audioManager.GetBGMValueForSlider());
        }
    }

    public void SetSliderValue(float value)
    {
        slider.value = value;
    }

    public void OnSliderValueChanged()
    {
        onSliderValueChangedSO.RaiseEvent(slider.value);
    }
}
