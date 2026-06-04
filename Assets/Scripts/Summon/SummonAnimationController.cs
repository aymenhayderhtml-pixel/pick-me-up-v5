using UnityEngine;
using System.Collections;

/// <summary>
/// Controls summon animation playback, skip logic, and rarity-gated confirmation.
/// Double-tap skips 1-3★ animations; 4★+ shows confirmation modal.
/// </summary>
public class SummonAnimationController : MonoBehaviour
{
    [Header("Skip Configuration")]
    [SerializeField] private int forcedAnimationRarity = 4; // 4★+ cannot skip without confirmation
    [SerializeField] private GameObject skipConfirmationModal;
    [SerializeField] private TMPro.TextMeshProUGUI skipModalText;
    [SerializeField] private TMPro.TextMeshProUGUI confirmSkipButtonLabel;
    [SerializeField] private TMPro.TextMeshProUGUI cancelSkipButtonLabel;

    [Header("Animation State")]
    [SerializeField] private bool isAnimating = false;
    [SerializeField] private int currentResultRarity = 0;

    private float tapTimer = 0f;
    private const float DOUBLE_TAP_WINDOW = 0.3f;
    private bool doubleTapDetected = false;
    private System.Action onSkipCallback;
    private System.Action onConfirmSkipCallback;

    private void Awake()
    {
        if (skipConfirmationModal != null)
            skipConfirmationModal.SetActive(false);

        if (confirmSkipButtonLabel != null)
            confirmSkipButtonLabel.text = "Skip Anyway";
        if (cancelSkipButtonLabel != null)
            cancelSkipButtonLabel.text = "Watch Celebration";
    }

    private void Update()
    {
        if (!isAnimating) return;
        if (tapTimer > 0) tapTimer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0) || 
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            if (tapTimer > 0)
            {
                // Double tap detected
                AttemptSkip();
                tapTimer = 0f;
            }
            else
            {
                tapTimer = DOUBLE_TAP_WINDOW;
            }
        }
    }

    public void StartSummonAnimation(int rarity, System.Action onSkip = null, System.Action onConfirmSkip = null)
    {
        currentResultRarity = rarity;
        isAnimating = true;
        doubleTapDetected = false;
        onSkipCallback = onSkip;
        onConfirmSkipCallback = onConfirmSkip;
    }

    private void AttemptSkip()
    {
        if (currentResultRarity >= forcedAnimationRarity && skipConfirmationModal != null)
        {
            // Rare result — show confirmation
            if (skipModalText != null)
                skipModalText.text = $"A {currentResultRarity}★ hero appeared! Watch the celebration?";
            skipConfirmationModal.SetActive(true);
        }
        else
        {
            SkipAnimation();
        }
    }

    public void SkipAnimation()
    {
        doubleTapDetected = true;
        isAnimating = false;
        if (skipConfirmationModal != null)
            skipConfirmationModal.SetActive(false);
        onSkipCallback?.Invoke();
    }

    public void ConfirmSkip()
    {
        if (skipConfirmationModal != null)
            skipConfirmationModal.SetActive(false);
        doubleTapDetected = true;
        isAnimating = false;
        onConfirmSkipCallback?.Invoke();
        SkipAnimation();
    }

    public void CancelSkip()
    {
        if (skipConfirmationModal != null)
            skipConfirmationModal.SetActive(false);
    }

    public bool IsAnimating() => isAnimating;
    public bool HasDoubleTap()
    {
        bool result = doubleTapDetected;
        doubleTapDetected = false;
        return result;
    }

    public void EndAnimation()
    {
        isAnimating = false;
        doubleTapDetected = false;
        if (skipConfirmationModal != null)
            skipConfirmationModal.SetActive(false);
    }
}