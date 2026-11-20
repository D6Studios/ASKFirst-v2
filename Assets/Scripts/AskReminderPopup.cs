using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AskReminderPopup : MonoBehaviour
{
    public static AskReminderPopup Instance { get; private set; }

    [SerializeField] Canvas reminderCanvas;
    [SerializeField] TextMeshProUGUI bodyText;
    [SerializeField] TextMeshProUGUI badgeText;   // optional: big letter in the circle ("S" / "K")
    [SerializeField] TextMeshProUGUI headingText; // optional: heading next to the badge ("Safety First")
    [SerializeField] Button closeButton;

    [Header("Fade")]
    [Tooltip("Full-screen backdrop behind the popup. Found by name (\"Dim\") if left empty.")]
    [SerializeField] Image dimBackdrop;
    [Range(0f, 1f)][SerializeField] float dimAlpha = 0.5f;
    [Tooltip("The popup panel; it scales up slightly as it fades in. Found by name (\"Panel\") if left empty.")]
    [SerializeField] RectTransform panel;
    [SerializeField] float fadeInDuration = 0.3f;
    [SerializeField] float fadeOutDuration = 0.2f;

    CanvasGroup group;
    Coroutine fadeRoutine;

    public bool IsOpen { get; private set; }

    // One of these is shown at random each time the popup appears.
    // The first line of each message is rendered bold as a heading.
    [TextArea(3, 8)]
    [SerializeField]
    string[] reminderMessages =
    {
        "S – Safety First\nMaintain a safe distance when interacting with suspicious individuals. Do not attempt any physical contact or confrontation.",
        "K – Keep Close\nMonitor discreetly by performing routine duties.",
    };

    void Awake()
    {
        Instance = this;
        reminderCanvas.enabled = false;
        closeButton.onClick.AddListener(Close);

        group = reminderCanvas.GetComponent<CanvasGroup>();
        if (group == null) group = reminderCanvas.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;

        if (dimBackdrop == null)
        {
            Transform t = reminderCanvas.transform.Find("Dim");
            if (t != null) dimBackdrop = t.GetComponent<Image>();
        }
        if (dimBackdrop != null)
        {
            Color c = dimBackdrop.color;
            c.a = dimAlpha;
            dimBackdrop.color = c;
        }
        if (panel == null) panel = reminderCanvas.transform.Find("Panel") as RectTransform;
    }

    public void Show()
    {
        string message = reminderMessages[Random.Range(0, reminderMessages.Length)];
        int split = message.IndexOf('\n');
        string heading = split < 0 ? "" : message.Substring(0, split).Trim();
        string body = split < 0 ? message : message.Substring(split + 1).Trim();

        if (headingText != null && badgeText != null && heading.Length > 0)
        {
            // "S – Safety First" -> badge "S", heading "Safety First"
            int dash = heading.IndexOfAny(new[] { '–', '-' });
            badgeText.text = heading.Substring(0, 1);
            headingText.text = dash >= 0 ? heading.Substring(dash + 1).Trim() : heading;
            bodyText.text = body;
        }
        else
        {
            bodyText.text = heading.Length > 0
                ? "<b><size=120%>" + heading + "</size></b>\n" + body
                : body;
        }
        reminderCanvas.enabled = true;
        IsOpen = true;
        Time.timeScale = 0f;
        closeButton.interactable = true;
        StartFade(FadeIn());
    }

    void Close()
    {
        closeButton.interactable = false;
        IsOpen = false;
        Time.timeScale = 1f;
        StartFade(FadeOut());
    }

    void StartFade(IEnumerator routine)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(routine);
    }

    // Uses unscaled time because the game is paused (timeScale 0) while the popup is open.
    IEnumerator FadeIn()
    {
        float startAlpha = group.alpha;
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            float t = elapsed / fadeInDuration;
            float e = 1f - (1f - t) * (1f - t); // ease out
            group.alpha = Mathf.Lerp(startAlpha, 1f, e);
            if (panel != null) panel.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, e);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        group.alpha = 1f;
        if (panel != null) panel.localScale = Vector3.one;
        fadeRoutine = null;
    }

    IEnumerator FadeOut()
    {
        float startAlpha = group.alpha;
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            group.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        group.alpha = 0f;
        reminderCanvas.enabled = false;
        if (panel != null) panel.localScale = Vector3.one;
        fadeRoutine = null;
    }
}
