using System;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class LevelIntro : MonoBehaviour
{
    [SerializeField] string levelName;
    [TextArea][SerializeField] string levelDescription;

    [SerializeField] Canvas introCanvas;
    [SerializeField] TextMeshProUGUI levelNameText;
    [SerializeField] TextMeshProUGUI levelDescriptionText;
    [Tooltip("The START button. Pressing it ends the intro and plays the objective transition.")]
    [SerializeField] Button skipButton;

    [Header("Objective")]
    [Tooltip("Card on the intro screen showing the level objective. It flies into the in-game objective bar when START is pressed.")]
    [SerializeField] RectTransform objectiveCard;
    [SerializeField] TextMeshProUGUI objectiveCardText;
    [Tooltip("Leave empty to use \"Talk to N customers\" (same as the in-game bar).")]
    [SerializeField] string objectiveOverride;
    [SerializeField] float objectiveFlyDuration = 0.8f;
    [Tooltip("Time for the objective card to glide to the centre of the screen after START.")]
    [SerializeField] float objectiveCentreMoveDuration = 0.5f;
    [Tooltip("How long the objective stays in the centre before flying up to the bar.")]
    [SerializeField] float objectiveCentreHold = 1.2f;
    [Tooltip("Scale of the objective card while it's held in the centre.")]
    [SerializeField] float objectiveCentreScale = 1.15f;

    [Header("Camera pan")]
    [SerializeField] CinemachineCamera introCamera;
    [SerializeField] int introCameraPriority = 20;
    [SerializeField] Transform[] panWaypoints;
    [SerializeField] float panDuration = 6f;
    [SerializeField] float holdAfterPan = 1f;
    [Tooltip("If off, the intro waits on the last waypoint until START is pressed.")]
    [SerializeField] bool autoStartAfterPan = false;
    [SerializeField] float uiFadeDuration = 0.3f;
    [Tooltip("Seconds for the camera to glide back to the player from wherever the intro camera is (including when START is pressed mid-pan).")]
    [SerializeField] float cameraReturnDuration = 1.5f;

    bool skipRequested;

    void Awake()
    {
        if (introCanvas != null) introCanvas.enabled = false;
        if (skipButton != null) skipButton.onClick.AddListener(() => skipRequested = true);
    }

    public void PlayIntro(Action onComplete)
    {
        StartCoroutine(RunIntro(onComplete));
    }

    IEnumerator RunIntro(Action onComplete)
    {
        skipRequested = false;
        float previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (levelNameText != null) levelNameText.text = levelName;
        if (levelDescriptionText != null) levelDescriptionText.text = levelDescription;
        if (objectiveCardText != null) objectiveCardText.text = BuildObjectiveText();
        if (introCanvas != null) introCanvas.enabled = true;

        CursorLockMode previousLockState = Cursor.lockState;
        bool previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Renderer[] playerRenderers = null;
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerRenderers = player.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in playerRenderers) r.enabled = false;
        }

        // The game is paused (timeScale 0) during the intro. The brain must ignore time scale or
        // its blend to the intro camera never advances and the view stays on the player camera.
        CinemachineBrain brain = Camera.main != null ? Camera.main.GetComponent<CinemachineBrain>() : null;
        bool previousIgnoreTimeScale = brain != null && brain.IgnoreTimeScale;
        if (brain != null) brain.IgnoreTimeScale = true;

        int previousPriority = 0;
        if (introCamera != null)
        {
            previousPriority = introCamera.Priority;
            introCamera.Priority = introCameraPriority;
        }

        if (introCamera != null && panWaypoints != null && panWaypoints.Length > 1)
        {
            yield return PanCamera();
        }
        else
        {
            yield return WaitUnscaled(panDuration);
        }

        if (autoStartAfterPan)
            yield return WaitUnscaled(holdAfterPan);
        else
            while (!skipRequested) yield return null;

        if (skipButton != null) skipButton.interactable = false;

        // Remember the card's resting spot so it can be put back after the animation.
        Vector3 cardRestPos = objectiveCard != null ? objectiveCard.position : Vector3.zero;
        Vector3 cardRestScale = objectiveCard != null ? objectiveCard.localScale : Vector3.one;

        // Fade out the menu while the objective card glides to the centre of the screen, then hold.
        StartCoroutine(FadeIntroUI());
        yield return MoveObjectiveToCentre();
        yield return WaitUnscaledFixed(objectiveCentreHold);

        // Hand control back to the game: camera blends back to the player while the card flies.
        // Use a longer, eased blend for the hand-back so the camera glides from its current spot
        // (wherever the pan was when START was pressed) to the player instead of snapping.
        CinemachineBlendDefinition previousBlend = default;
        if (brain != null)
        {
            previousBlend = brain.DefaultBlend;
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, cameraReturnDuration);
        }
        if (introCamera != null) introCamera.Priority = previousPriority;
        Time.timeScale = previousTimeScale;
        if (playerRenderers != null)
        {
            foreach (Renderer r in playerRenderers)
            {
                if (r != null) r.enabled = true;
            }
        }
        Cursor.lockState = previousLockState;
        Cursor.visible = previousCursorVisible;

        onComplete?.Invoke(); // shows GameUI + resets objectives

        yield return FlyObjectiveToBar();

        // Let the return blend finish before restoring the brain's normal settings.
        while (brain != null && brain.IsBlending) yield return null;
        if (brain != null)
        {
            brain.IgnoreTimeScale = previousIgnoreTimeScale;
            brain.DefaultBlend = previousBlend;
        }
        if (introCanvas != null) introCanvas.enabled = false;
        if (objectiveCard != null)
        {
            objectiveCard.position = cardRestPos;
            objectiveCard.localScale = cardRestScale;
        }
    }

    IEnumerator MoveObjectiveToCentre()
    {
        if (objectiveCard == null || introCanvas == null) yield break;
        RectTransform canvasRect = introCanvas.transform as RectTransform;
        Vector3 startPos = objectiveCard.position;
        Vector3 endPos = canvasRect.TransformPoint(canvasRect.rect.center);
        Vector3 startScale = objectiveCard.localScale;
        Vector3 endScale = startScale * objectiveCentreScale;

        float elapsed = 0f;
        while (elapsed < objectiveCentreMoveDuration)
        {
            float t = Mathf.Clamp01(elapsed / objectiveCentreMoveDuration);
            objectiveCard.position = Vector3.LerpUnclamped(startPos, endPos, EaseInOutCubic(t));
            objectiveCard.localScale = Vector3.LerpUnclamped(startScale, endScale, EaseOutBack(t));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        objectiveCard.position = endPos;
        objectiveCard.localScale = endScale;
    }

    static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    string BuildObjectiveText()
    {
        if (!string.IsNullOrEmpty(objectiveOverride)) return objectiveOverride;
        int count = 0;
        foreach (GameObject npc in GameObject.FindGameObjectsWithTag("NPC"))
        {
            NPCBehavior b = npc.GetComponent<NPCBehavior>();
            if (b != null && b.isShoplifter) count++;
        }
        return "Talk to " + count + " customers";
    }

    IEnumerator FadeIntroUI()
    {
        if (introCanvas == null) yield break;
        foreach (Graphic g in introCanvas.GetComponentsInChildren<Graphic>())
        {
            if (objectiveCard != null && g.transform.IsChildOf(objectiveCard)) continue;
            g.CrossFadeAlpha(0f, uiFadeDuration, true);
        }
        yield return WaitUnscaledFixed(uiFadeDuration);
    }

    IEnumerator FlyObjectiveToBar()
    {
        GameObject objectiveTextGO = GameObject.FindGameObjectWithTag("Objective");
        if (objectiveCard == null || objectiveTextGO == null) yield break;

        RectTransform bar = objectiveTextGO.transform.parent as RectTransform;
        CanvasGroup barGroup = bar.GetComponent<CanvasGroup>();
        if (barGroup == null) barGroup = bar.gameObject.AddComponent<CanvasGroup>();
        barGroup.alpha = 0f;

        CanvasGroup cardGroup = objectiveCard.GetComponent<CanvasGroup>();
        if (cardGroup == null) cardGroup = objectiveCard.gameObject.AddComponent<CanvasGroup>();

        // Let the GameUI layout settle for a frame before reading the bar's position.
        yield return null;

        Vector3 startPos = objectiveCard.position;
        Vector3 startScale = objectiveCard.localScale;
        Vector3 endPos = bar.TransformPoint(bar.rect.center);
        float cardHeight = objectiveCard.rect.height * objectiveCard.lossyScale.y;
        float barHeight = bar.rect.height * bar.lossyScale.y;
        Vector3 endScale = startScale * (cardHeight > 0f ? barHeight / cardHeight : 0.5f);
        // Arc slightly upward-then-in rather than a straight line.
        Vector3 control = Vector3.Lerp(startPos, endPos, 0.5f) + new Vector3(0f, (endPos.y - startPos.y) * 0.25f, 0f);

        // The card's contents are centred for the intro; slide each child back to where the
        // matching child sits in the gameplay bar (matched by name) so the hand-off is seamless.
        int childCount = objectiveCard.childCount;
        RectTransform[] cardChildren = new RectTransform[childCount];
        Vector2[] childStart = new Vector2[childCount];
        Vector2[] childEnd = new Vector2[childCount];
        for (int i = 0; i < childCount; i++)
        {
            cardChildren[i] = objectiveCard.GetChild(i) as RectTransform;
            if (cardChildren[i] == null) continue;
            childStart[i] = cardChildren[i].anchoredPosition;
            RectTransform match = bar.Find(cardChildren[i].name) as RectTransform;
            childEnd[i] = match != null ? match.anchoredPosition : childStart[i];
        }

        float elapsed = 0f;
        while (elapsed < objectiveFlyDuration)
        {
            float t = Mathf.Clamp01(elapsed / objectiveFlyDuration);
            float e = EaseInOutCubic(t);
            objectiveCard.position = QuadBezier(startPos, control, endPos, e);
            objectiveCard.localScale = Vector3.LerpUnclamped(startScale, endScale, e);
            for (int i = 0; i < childCount; i++)
                if (cardChildren[i] != null) cardChildren[i].anchoredPosition = Vector2.LerpUnclamped(childStart[i], childEnd[i], e);

            // Cross-fade into the real bar over the last 35% of the flight.
            float fade = Mathf.InverseLerp(0.65f, 1f, t);
            cardGroup.alpha = 1f - fade;
            barGroup.alpha = fade;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        cardGroup.alpha = 0f;
        barGroup.alpha = 1f;

        // Small "landing" pop on the bar.
        Vector3 barScale = bar.localScale;
        elapsed = 0f;
        const float popDuration = 0.25f;
        while (elapsed < popDuration)
        {
            float t = elapsed / popDuration;
            bar.localScale = barScale * (1f + 0.08f * Mathf.Sin(t * Mathf.PI));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        bar.localScale = barScale;

        objectiveCard.position = startPos;
        objectiveCard.localScale = startScale;
        for (int i = 0; i < childCount; i++)
            if (cardChildren[i] != null) cardChildren[i].anchoredPosition = childStart[i];
    }

    static Vector3 QuadBezier(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        float u = 1f - t;
        return u * u * a + 2f * u * t * b + t * t * c;
    }

    static float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }

    IEnumerator WaitUnscaled(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration && !skipRequested)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    IEnumerator WaitUnscaledFixed(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    IEnumerator PanCamera()
    {
        Transform camTransform = introCamera.transform;
        float segmentDuration = panDuration / (panWaypoints.Length - 1);
        camTransform.SetPositionAndRotation(panWaypoints[0].position, panWaypoints[0].rotation);

        for (int i = 0; i < panWaypoints.Length - 1 && !skipRequested; i++)
        {
            Vector3 startPos = panWaypoints[i].position;
            Quaternion startRot = panWaypoints[i].rotation;
            Vector3 endPos = panWaypoints[i + 1].position;
            Quaternion endRot = panWaypoints[i + 1].rotation;

            float elapsed = 0f;
            while (elapsed < segmentDuration && !skipRequested)
            {
                float t = elapsed / segmentDuration;
                camTransform.SetPositionAndRotation(Vector3.Lerp(startPos, endPos, t), Quaternion.Slerp(startRot, endRot, t));
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            // Only snap to the segment end if it actually finished; if START was pressed
            // mid-segment, leave the camera where it is so the return blend starts from there.
            if (!skipRequested) camTransform.SetPositionAndRotation(endPos, endRot);
        }
    }
}
