using UnityEngine;
using UnityEngine.UI;

public class UIHoverShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeIntensity = 2f;
    [SerializeField] private float shakeSpeed = 10f;
    [SerializeField] private bool enableVerticalShake = true;
    [SerializeField] private bool enableHorizontalShake = true;

    [Header("Floating Settings")]
    [SerializeField] private bool enableFloating = true;
    [SerializeField] private float floatAmplitude = 5f;
    [SerializeField] private float floatSpeed = 2f;

    [Header("Auto Start")]
    [SerializeField] private bool startOnAwake = true;

    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private bool isShaking = false;

    private float shakeTimer = 0f;
    private float floatTimer = 0f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;

        if (startOnAwake)
        {
            StartHoverEffect();
        }
    }

    void Update()
    {
        if (isShaking)
        {
            ApplyHoverEffect();
        }
    }

    private void ApplyHoverEffect()
    {
        Vector3 newPosition = originalPosition;

        // Apply floating effect (smooth up and down motion)
        if (enableFloating)
        {
            floatTimer += Time.deltaTime * floatSpeed;
            newPosition.y += Mathf.Sin(floatTimer) * floatAmplitude;
        }

        // Apply shake effect (random small movements)
        shakeTimer += Time.deltaTime * shakeSpeed;

        if (enableHorizontalShake)
        {
            newPosition.x += Mathf.Sin(shakeTimer * 1.3f) * shakeIntensity;
            newPosition.x += Mathf.Cos(shakeTimer * 0.7f) * shakeIntensity * 0.5f;
        }

        if (enableVerticalShake)
        {
            newPosition.y += Mathf.Cos(shakeTimer * 1.1f) * shakeIntensity * 0.8f;
            newPosition.y += Mathf.Sin(shakeTimer * 0.9f) * shakeIntensity * 0.3f;
        }

        rectTransform.anchoredPosition = newPosition;
    }

    /// <summary>
    /// Start the hover shake effect
    /// </summary>
    public void StartHoverEffect()
    {
        isShaking = true;
        shakeTimer = 0f;
        floatTimer = 0f;
    }

    /// <summary>
    /// Stop the hover shake effect and return to original position
    /// </summary>
    public void StopHoverEffect()
    {
        isShaking = false;
        rectTransform.anchoredPosition = originalPosition;
    }

    /// <summary>
    /// Toggle the hover effect on/off
    /// </summary>
    public void ToggleHoverEffect()
    {
        if (isShaking)
            StopHoverEffect();
        else
            StartHoverEffect();
    }

    /// <summary>
    /// Set the shake intensity at runtime
    /// </summary>
    /// <param name="intensity">New shake intensity value</param>
    public void SetShakeIntensity(float intensity)
    {
        shakeIntensity = intensity;
    }

    /// <summary>
    /// Set the shake speed at runtime
    /// </summary>
    /// <param name="speed">New shake speed value</param>
    public void SetShakeSpeed(float speed)
    {
        shakeSpeed = speed;
    }

    /// <summary>
    /// Reset to original position (useful for repositioning the UI element)
    /// </summary>
    public void ResetOriginalPosition()
    {
        originalPosition = rectTransform.anchoredPosition;
    }

    void OnDisable()
    {
        // Return to original position when disabled
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }
}