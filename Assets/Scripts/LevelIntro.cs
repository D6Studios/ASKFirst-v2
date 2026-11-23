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

    [Header("Reveal")]
    [SerializeField] float titleFadeDuration = 0.4f;
    [Tooltip("Seconds the title sits alone in the centre before moving up.")]
    [SerializeField] float titleHold = 0.8f;
    [SerializeField] float titleMoveDuration = 0.6f;
    [SerializeField] float previewFadeDuration = 0.5f;
    [SerializeField] float typewriterCharsPerSecond = 45f;
    [Tooltip("Minimum seconds between typing tap sounds.")]
    [SerializeField] float typingTapInterval = 0.07f;
    [SerializeField] float buttonFadeDuration = 0.3f;

    [Header("Camera preview")]
    [Tooltip("Shows the panning intro camera (rendered to a texture). Its parent is the frame around it. On START it zooms to fill the screen, then the game view takes over.")]
    [SerializeField] RawImage cameraPreview;

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
        if (skipButton != null) skipButton.onClick.AddListener(() =>
        {
            skipRequested = true;
            SoundManager.Instance.PlaySound(Resources.Load<AudioClip>("click"));
        });
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
        if (levelDescriptionText != null)
        {
            levelDescriptionText.text = levelDescription;
            levelDescriptionText.maxVisibleCharacters = 0; // laid out in full but hidden, then revealed letter by letter
        }
        if (objectiveCardText != null) objectiveCardText.text = BuildObjectiveText();
        if (objectiveCard != null) objectiveCard.gameObject.SetActive(false); // shown in-game after the hand-off
        // Reveal order: title alone in the centre, title moves up, preview fades in, description types in, START appears.
        if (skipButton != null) skipButton.gameObject.SetActive(false);
        CanvasGroup previewGroup = cameraPreview != null ? GroupOf(cameraPreview.transform.parent) : null;
        if (previewGroup != null) previewGroup.alpha = 0f;
        CanvasGroup titleGroup = levelNameText != null ? GroupOf(levelNameText.transform) : null;
        if (titleGroup != null) titleGroup.alpha = 0f;
        if (introCanvas != null) introCanvas.enabled = true;

        CursorLockMode previousLockState = Cursor.lockState;
        bool previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        HidePlayerBody();

        // The game is paused (timeScale 0) during the intro. The brain must ignore time scale or
        // its blend to the intro camera never advances and the view stays on the player camera.
        Camera mainCam = Camera.main;
        CinemachineBrain brain = mainCam != null ? mainCam.GetComponent<CinemachineBrain>() : null;
        bool previousIgnoreTimeScale = brain != null && brain.IgnoreTimeScale;
        if (brain != null) brain.IgnoreTimeScale = true;

        // The game camera renders into the preview; a camera that draws nothing keeps the screen (and this UI) rendering.
        RenderTexture previewTexture = null;
        Camera screenCam = null;
        if (cameraPreview != null && mainCam != null)
        {
            previewTexture = new RenderTexture(Screen.width, Screen.height, 24);
            mainCam.targetTexture = previewTexture;
            cameraPreview.texture = previewTexture;
            cameraPreview.gameObject.SetActive(true);
            FitPreviewToScreenAspect();
            screenCam = new GameObject("IntroScreenCamera").AddComponent<Camera>();
            screenCam.cullingMask = 0;
            screenCam.clearFlags = CameraClearFlags.SolidColor;
            screenCam.backgroundColor = Color.black;
        }

        int previousPriority = 0;
        if (introCamera != null)
        {
            previousPriority = introCamera.Priority;
            introCamera.Priority = introCameraPriority;
        }

        bool canPan = introCamera != null && panWaypoints != null && panWaypoints.Length > 1;
        if (canPan) introCamera.transform.SetPositionAndRotation(panWaypoints[0].position, panWaypoints[0].rotation);

        yield return RevealTitle(titleGroup);

        // The pan starts as the preview fades in, and runs while the description types out.
        bool panDone = false;
        StartCoroutine(RunPan(canPan, () => panDone = true));
        StartCoroutine(Fade(previewGroup, 1f, previewFadeDuration));
        yield return WaitUnscaledFixed(previewFadeDuration * 0.5f);
        yield return TypeDescription();

        if (skipButton != null)
        {
            CanvasGroup buttonGroup = GroupOf(skipButton.transform);
            buttonGroup.alpha = 0f;
            skipButton.gameObject.SetActive(true);
            yield return Fade(buttonGroup, 1f, buttonFadeDuration);
        }

        if (autoStartAfterPan)
        {
            while (!panDone && !skipRequested) yield return null;
            yield return WaitUnscaled(holdAfterPan);
        }
        else
            while (!skipRequested) yield return null;

        if (skipButton != null) skipButton.interactable = false;

        // START: the menu fades, the preview zooms to fill the screen, and meanwhile the camera
        // glides back to the player, so when the zoom lands the texture already shows the game view.
        StartCoroutine(FadeIntroUI());
        CinemachineBlendDefinition previousBlend = default;
        if (brain != null)
        {
            previousBlend = brain.DefaultBlend;
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, cameraReturnDuration);
        }
        if (introCamera != null) introCamera.Priority = previousPriority;
        SoundManager.Instance.PlaySound(Resources.Load<AudioClip>("SFX/swoosh"));
        yield return null; // let the brain start its blend before checking IsBlending
        yield return ZoomPreviewToFullScreen();
        while (brain != null && brain.IsBlending) yield return null;

        // Swap: the full-screen texture and the live camera show the same frame, so this is invisible.
        if (previewTexture != null)
        {
            mainCam.targetTexture = null;
            cameraPreview.gameObject.SetActive(false);
            cameraPreview.texture = null;
            Destroy(screenCam.gameObject);
            previewTexture.Release();
            Destroy(previewTexture);
        }
        if (brain != null)
        {
            brain.IgnoreTimeScale = previousIgnoreTimeScale;
            brain.DefaultBlend = previousBlend;
        }
        Time.timeScale = previousTimeScale;
        Cursor.lockState = previousLockState;
        Cursor.visible = previousCursorVisible;

        onComplete?.Invoke(); // shows GameUI + resets objectives
        SetObjectiveBarAlpha(0f); // the card stands in for it until it lands

        // Objective pops up in the middle of the game view, holds, then swipes up into the bar.
        yield return PopObjectiveInCentre();
        yield return WaitUnscaledFixed(objectiveCentreHold);
        yield return FlyObjectiveToBar();

        if (introCanvas != null) introCanvas.enabled = false;
    }

    static CanvasGroup GroupOf(Transform t)
    {
        CanvasGroup g = t.GetComponent<CanvasGroup>();
        return g != null ? g : t.gameObject.AddComponent<CanvasGroup>();
    }

    static IEnumerator Fade(CanvasGroup group, float to, float duration)
    {
        if (group == null) yield break;
        float from = group.alpha;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
    }

    // Title fades in by itself in the middle of the screen, holds, then glides up to its place in the layout.
    IEnumerator RevealTitle(CanvasGroup titleGroup)
    {
        if (levelNameText == null || introCanvas == null) yield break;
        RectTransform title = levelNameText.rectTransform;
        RectTransform canvasRect = introCanvas.transform as RectTransform;
        Canvas.ForceUpdateCanvases();
        Vector3 restPos = title.position;
        Vector3 centre = canvasRect.TransformPoint(canvasRect.rect.center);
        title.position = centre;

        yield return Fade(titleGroup, 1f, titleFadeDuration);
        yield return WaitUnscaledFixed(titleHold);
        for (float t = 0f; t < titleMoveDuration; t += Time.unscaledDeltaTime)
        {
            title.position = Vector3.LerpUnclamped(centre, restPos, EaseInOutCubic(t / titleMoveDuration));
            yield return null;
        }
        title.position = restPos;
    }

    // Reveals the already laid-out description one letter at a time; a tap anywhere shows it all at once.
    IEnumerator TypeDescription()
    {
        if (levelDescriptionText == null) yield break;
        levelDescriptionText.ForceMeshUpdate();
        int total = levelDescriptionText.textInfo.characterCount;
        AudioClip[] taps = Resources.LoadAll<AudioClip>("SFX/Typing");
        float shown = 0f, nextTapTime = 0f;
        while (shown < total)
        {
            if (TappedThisFrame()) break;
            int before = levelDescriptionText.maxVisibleCharacters;
            shown += typewriterCharsPerSecond * Time.unscaledDeltaTime;
            levelDescriptionText.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(shown));
            // One tap per new letter, capped to a believable typing speed and silent on spaces.
            int last = levelDescriptionText.maxVisibleCharacters - 1;
            if (last >= before && Time.unscaledTime >= nextTapTime && !char.IsWhiteSpace(levelDescriptionText.textInfo.characterInfo[last].character))
            {
                SoundManager.Instance.PlayRandom(taps, 0.05f);
                nextTapTime = Time.unscaledTime + typingTapInterval;
            }
            yield return null;
        }
        levelDescriptionText.maxVisibleCharacters = int.MaxValue;
        yield return null; // don't let the skipping tap also land on START, which appears next
    }

    static bool TappedThisFrame()
    {
        return UnityEngine.InputSystem.Pointer.current != null && UnityEngine.InputSystem.Pointer.current.press.wasPressedThisFrame;
    }

    IEnumerator RunPan(bool canPan, Action done)
    {
        if (canPan) yield return PanCamera();
        else yield return WaitUnscaled(panDuration);
        done();
    }

    // First person never needs the player's body; hiding it keeps it out of the intro, the zoom and NPC close-ups.
    static void HidePlayerBody()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;
        foreach (Renderer r in player.transform.root.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
    }

    // The preview matches the screen's aspect so the zoomed texture lines up exactly with the real view.
    void FitPreviewToScreenAspect()
    {
        RectTransform frame = cameraPreview.rectTransform.parent as RectTransform;
        RectTransform preview = cameraPreview.rectTransform;
        float border = frame.rect.width - preview.rect.width;
        frame.sizeDelta = new Vector2(frame.sizeDelta.x, preview.rect.width * Screen.height / Screen.width + border);
    }

    IEnumerator ZoomPreviewToFullScreen()
    {
        if (cameraPreview == null || introCanvas == null) yield break;
        RectTransform preview = cameraPreview.rectTransform;
        RectTransform canvasRect = introCanvas.transform as RectTransform;
        Canvas.ForceUpdateCanvases();
        Vector3 startPos = preview.position;
        Vector3 endPos = canvasRect.TransformPoint(canvasRect.rect.center);
        Vector3 startScale = preview.localScale;
        Vector3 endScale = startScale * (canvasRect.rect.width * canvasRect.lossyScale.x) / (preview.rect.width * preview.lossyScale.x);

        float elapsed = 0f;
        while (elapsed < cameraReturnDuration)
        {
            float t = EaseInOutCubic(Mathf.Clamp01(elapsed / cameraReturnDuration));
            preview.position = Vector3.LerpUnclamped(startPos, endPos, t);
            preview.localScale = Vector3.LerpUnclamped(startScale, endScale, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        preview.position = endPos;
        preview.localScale = endScale;
    }

    IEnumerator PopObjectiveInCentre()
    {
        if (objectiveCard == null || introCanvas == null) yield break;
        RectTransform canvasRect = introCanvas.transform as RectTransform;
        objectiveCard.gameObject.SetActive(true);
        objectiveCard.position = canvasRect.TransformPoint(canvasRect.rect.center);
        Vector3 endScale = objectiveCard.localScale * objectiveCentreScale;

        float elapsed = 0f;
        while (elapsed < objectiveCentreMoveDuration)
        {
            float t = Mathf.Clamp01(elapsed / objectiveCentreMoveDuration);
            objectiveCard.localScale = Vector3.LerpUnclamped(Vector3.zero, endScale, EaseOutBack(t));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        objectiveCard.localScale = endScale;
    }

    static void SetObjectiveBarAlpha(float alpha)
    {
        GameObject objectiveTextGO = GameObject.FindGameObjectWithTag("Objective");
        if (objectiveTextGO == null) return;
        GameObject bar = objectiveTextGO.transform.parent.gameObject;
        CanvasGroup group = bar.GetComponent<CanvasGroup>();
        if (group == null) group = bar.AddComponent<CanvasGroup>();
        group.alpha = alpha;
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
            if (g == cameraPreview) continue; // it zooms instead
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
