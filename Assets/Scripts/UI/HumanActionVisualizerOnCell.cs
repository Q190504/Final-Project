using System.Collections;
using UnityEngine;

public class HumanActionVisualizerOnCell : MonoBehaviour
{
    [SerializeField] private SpriteRenderer pulseRenderer;

    private Coroutine currentRoutine;

    private void Start()
    {
        pulseRenderer.enabled = false;
    }

    public void PlayVisual(HumanActionVisualPreset preset)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(PlayRoutine(preset));
    }

    private IEnumerator PlayRoutine(HumanActionVisualPreset preset)
    {
        pulseRenderer.enabled = true;
        pulseRenderer.color = preset.Color;

        float time = 0f;

        while (time < preset.Duration)
        {
            time += Time.deltaTime;

            float t = time / preset.Duration;

            float scale = Mathf.Lerp(
                    preset.StartScale,
                    preset.EndScale,
                    t);

            float alpha = preset.FadeCurve.Evaluate(t);

            pulseRenderer.transform.localScale = Vector3.one * scale;

            Color c = preset.Color;
            c.a *= alpha;

            pulseRenderer.color = c;

            yield return null;
        }

        pulseRenderer.enabled = false;
    }
}