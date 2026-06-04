using UnityEngine;

/// <summary>
/// Controls the idle summon circle animation and fade transitions.
/// Shown when no summon result is active.
/// </summary>
public class SummonIdleState : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform ring;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float rotateSpeed = 30f;

    private bool isShowing = true;

    private void Awake()
    {
        if (group == null) group = GetComponent<CanvasGroup>();
        if (group != null) group.alpha = isShowing ? 1f : 0f;
    }

    private void Update()
    {
        if (ring != null)
            ring.Rotate(0f, 0f, -rotateSpeed * Time.deltaTime);
    }

    public void Show()
    {
        isShowing = true;
        StopAllCoroutines();
        StartCoroutine(FadeTo(1f, 0.3f));
    }

    public void Hide()
    {
        isShowing = false;
        StopAllCoroutines();
        StartCoroutine(FadeTo(0f, 0.3f));
    }

    private System.Collections.IEnumerator FadeTo(float target, float duration)
    {
        if (group == null) yield break;
        float start = group.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            group.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }
        group.alpha = target;
    }
}