using UnityEngine;

[CreateAssetMenu(fileName = "Spread Method Target Visualization Preset", menuName = "Scriptable Objects/UI/Spread Method/Spread Method Target Visualization Preset")]
public class SpreadMethodTargetVisualizationPreset : ScriptableObject
{
    public Color Color;

    public float Duration = 0.5f;

    public float StartScale = 1.4f;
    public float EndScale = 1f;

    public AnimationCurve FadeCurve;

    public Sprite OverlaySprite;
}