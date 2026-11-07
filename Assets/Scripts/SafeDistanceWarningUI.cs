using System.Collections;
using TMPro;
using UnityEngine;

public class SafeDistanceWarningUI : MonoBehaviour
{
    public static SafeDistanceWarningUI Instance { get; private set; }

    [SerializeField] CanvasGroup warningGroup;
    [SerializeField] TextMeshProUGUI warningText;
    [SerializeField] string message = "Keep a safe distance!";
    [SerializeField] float showDuration = 1.5f;
    [SerializeField] float fadeDuration = 0.25f;

    Coroutine activeRoutine;

    void Awake()
    {
        Instance = this;
        warningGroup.alpha = 0f;
    }

    public void ShowWarning()
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        warningText.text = message;
        yield return Fade(warningGroup.alpha, 1f, fadeDuration);
        yield return new WaitForSeconds(showDuration);
        yield return Fade(warningGroup.alpha, 0f, fadeDuration);
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            warningGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        warningGroup.alpha = to;
    }
}
