using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class PointTextContainer : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text diffPointText;
    [SerializeField] private TMP_Text currentPointText;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SetPoints(int currentPoints, int diffPoints)
    {
        currentPointText.SetText(currentPoints.ToString());

        if (diffPoints > 0)
        {
            diffPointText.color = Color.green;
            diffPointText.SetText($"+{diffPoints}");

            animator.SetTrigger("ShowPointDiff");
        }
        else if (diffPoints < 0)
        {
            diffPointText.color = Color.red;
            diffPointText.SetText($"-{Mathf.Abs(diffPoints)}");

            animator.SetTrigger("ShowPointDiff");
        }
    }
}
