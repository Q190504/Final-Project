using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class Notification : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private Button closeButton;

    private bool isClickable;
    private Animator animator;
    private Coroutine currentRoutine;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ShowNotification(string text, Color color, float duration)
    {
        notificationText.SetText(text);
        notificationText.color = color;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(NotificationRoutine(duration));
    }

    private IEnumerator NotificationRoutine(float duration)
    {
        isClickable = false;

        // Show animation
        ShowPanel();

        // Wait display duration
        yield return new WaitForSeconds(duration);

        // Hide animation
        StartHideAnimations();
    }

    public IEnumerator DelayClickRoutine()
    {
        if (isClickable) yield break;
        yield return new WaitForSeconds(0.5f);
        isClickable = true;
    }

    public void CloseNotification()
    {
        if (!isClickable)
            return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);
        
        StartHideAnimations();
    }

    public void ShowUIElements()
    {
        SetUIElementsVisibility(true);
    }

    private void SetUIElementsVisibility(bool isVisible)
    {
        backgroundImage.gameObject.SetActive(isVisible);
        notificationText.gameObject.SetActive(isVisible);
        closeButton.gameObject.SetActive(isVisible);
        closeButton.interactable = isVisible;
    }

    private void ShowPanel()
    {
        animator.SetTrigger("show");
    }

    private void StartHideAnimations()
    {
        animator.SetTrigger("hide");
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
        SetUIElementsVisibility(false);
    }
}