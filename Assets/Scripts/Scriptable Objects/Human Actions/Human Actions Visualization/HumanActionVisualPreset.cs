using UnityEngine;

[CreateAssetMenu(fileName = "New Human Action Visual Preset", menuName = "Scriptable Objects/UI/Human Action/Human Action Visual Preset")]
public class HumanActionVisualPreset : ScriptableObject
{
    public Color Color;

    public float Duration = 0.5f;

    public float StartScale = 1.4f;
    public float EndScale = 1f;

    public AnimationCurve FadeCurve;

    public Sprite OverlaySprite;
}
