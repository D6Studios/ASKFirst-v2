using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class UIHoverGrow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale Settings")]
    [Tooltip("Scale multiplier when hovered (1.0 = no change, 1.2 = 20% bigger)")]
    [Range(1.0f, 2.0f)]
    public float hoverScale = 1.2f;

    [Tooltip("How fast the scaling animation happens")]
    [Range(0.1f, 1.0f)]
    public float animationSpeed = 0.3f;

    [Header("Animation Settings")]
    [Tooltip("Use smooth easing for the animation")]
    public bool useSmoothEasing = true;

    [Tooltip("Animation curve for custom easing (only used if useSmoothEasing is false)")]
    public AnimationCurve customCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Coroutine scaleCoroutine;
    private bool isHovered = false;

    void Start()
    {
        // Store the original scale
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isHovered)
        {
            isHovered = true;
            targetScale = originalScale * hoverScale;
            StartScaleAnimation();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isHovered)
        {
            isHovered = false;
            targetScale = originalScale;
            StartScaleAnimation();
        }
    }

    void StartScaleAnimation()
    {
        // Stop any existing animation
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }

        // Start new animation
        scaleCoroutine = StartCoroutine(ScaleAnimation());
    }

    IEnumerator ScaleAnimation()
    {
        Vector3 startScale = transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < animationSpeed)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / animationSpeed;

            // Apply easing
            if (useSmoothEasing)
            {
                // Smooth step easing
                progress = progress * progress * (3.0f - 2.0f * progress);
            }
            else
            {
                // Custom curve easing
                progress = customCurve.Evaluate(progress);
            }

            // Lerp between start and target scale
            transform.localScale = Vector3.Lerp(startScale, targetScale, progress);

            yield return null;
        }

        // Ensure we end exactly at target scale
        transform.localScale = targetScale;
        scaleCoroutine = null;
    }

    // Public methods for manual control
    public void ForceHover()
    {
        if (!isHovered)
        {
            OnPointerEnter(null);
        }
    }

    public void ForceUnhover()
    {
        if (isHovered)
        {
            OnPointerExit(null);
        }
    }

    // Reset to original scale (useful if you change the object's scale elsewhere)
    public void ResetOriginalScale()
    {
        originalScale = transform.localScale;
        if (!isHovered)
        {
            targetScale = originalScale;
        }
    }

    // Change hover scale at runtime
    public void SetHoverScale(float newScale)
    {
        hoverScale = newScale;
        if (isHovered)
        {
            targetScale = originalScale * hoverScale;
            StartScaleAnimation();
        }
    }

    void OnDisable()
    {
        // Reset scale when disabled
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }
        transform.localScale = originalScale;
        isHovered = false;
    }
}