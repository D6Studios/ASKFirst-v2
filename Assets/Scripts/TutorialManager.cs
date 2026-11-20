using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
public class TutorialManager : MonoBehaviour
{
    public GameObject currentPopup;
    [SerializeField] GameObject tutorialPopupPrefab;
    GameObject tutorialCanvas;
    Image tutorialBackground;
    Vector2 deviceSize;
    [SerializeField] Sprite[] tutorialImages;
    int currentTutorialIndex = 0;
    bool continueTutorial = false;
    Animator objectiveAnimator;
    TextMeshProUGUI objective1Text;
    TextMeshProUGUI objective2Text;
    [SerializeField]
    GameObject interactButton;
    [Tooltip("Name of the tutorial NPC in the scene. Falls back to the first shoplifter NPC if not found.")]
    [SerializeField] string tutorialNPCName = "Tutorial";

    [Header("Controls practice")]
    [Tooltip("How far (metres) the player must actually walk before the move step counts.")]
    [SerializeField] float requiredMoveDistance = 1.5f;
    [Tooltip("How much (degrees, total) the player must look around before the look step counts.")]
    [SerializeField] float requiredLookAngle = 45f;
    [Tooltip("Text on each popup's close button.")]
    [SerializeField] string popupButtonText = "OK, got it";

    TutorialHints hints;
    VisualElement moveJoystick;

    // Only one time-scale / background fade may run at a time, otherwise an opening popup and a
    // closing popup fight over Time.timeScale and the game can end up frozen (or running under a popup).
    Coroutine speedRoutine;
    Coroutine bgRoutine;
    Coroutine tutorialRoutine;
    float currentBgOpacity = 0f;

    static readonly Color DarkBg = new Color(0f / 255f, 4f / 255f, 71f / 255f, 0f);
    static readonly Color Done = new Color(0f, 1f, 0f, 1f);
    const float StepDelay = 0.5f;

    void Start()
    {
        tutorialCanvas = GameObject.Find("TutorialCanvas");
        if (tutorialCanvas != null)
        {
            Transform bg = tutorialCanvas.transform.Find("Background");
            if (bg != null) tutorialBackground = bg.GetComponent<Image>();
        }
        GameObject objectives = GameObject.Find("TutorialObjectives");
        if (objectives != null) objectiveAnimator = objectives.GetComponent<Animator>();
        GameObject o1 = GameObject.Find("Objective1");
        GameObject o2 = GameObject.Find("Objective2");
        if (o1 != null) objective1Text = o1.GetComponent<TextMeshProUGUI>();
        if (o2 != null) objective2Text = o2.GetComponent<TextMeshProUGUI>();
        if (tutorialCanvas != null) hints = TutorialHints.Create(tutorialCanvas);
        GameObject mobileControls = GameObject.FindGameObjectWithTag("MobileControls");
        UIDocument doc = mobileControls != null ? mobileControls.GetComponent<UIDocument>() : null;
        if (doc != null && doc.rootVisualElement != null)
        {
            moveJoystick = doc.rootVisualElement.Q<VisualElement>("JoystickBackground") ?? doc.rootVisualElement.Q<VisualElement>("JoystickMove");
        }
        FindDeviceSize();
    }

    // Where the move joystick is on screen; falls back to the lower-left if it can't be measured.
    Vector2 JoystickScreenPos()
    {
        Vector2? p = TutorialHints.ScreenCenterOf(moveJoystick);
        return p ?? new Vector2(Screen.width * 0.18f, Screen.height * 0.25f);
    }
    Vector2 LookAreaScreenPos()
    {
        return new Vector2(Screen.width * 0.72f, Screen.height * 0.4f);
    }

    // Makes the popup's close button obvious: bigger, clearly labelled, with room left for it under the text.
    void StylePopupButton(GameObject popup)
    {
        Transform ok = popup.transform.Find("Ok");
        if (ok == null) return;
        RectTransform okRect = (RectTransform)ok;
        okRect.sizeDelta = new Vector2(-140f, 40f);
        okRect.anchoredPosition = new Vector2(0f, 28f);
        TextMeshProUGUI label = ok.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            label.text = popupButtonText;
            label.enableAutoSizing = true;
            label.fontSizeMin = 16f;
            label.fontSizeMax = 26f;
        }
        Transform text = popup.transform.Find("TutorialText");
        if (text != null)
        {
            RectTransform textRect = (RectTransform)text;
            textRect.offsetMin = new Vector2(textRect.offsetMin.x, 52f); // keep text clear of the button
            TextMeshProUGUI body = text.GetComponent<TextMeshProUGUI>();
            if (body != null)
            {
                // Shrink only if a message would otherwise overflow the popup.
                body.fontSizeMax = body.fontSize;
                body.fontSizeMin = 14f;
                body.enableAutoSizing = true;
            }
        }
    }

    void Update()
    {
        // Keep the game frozen while a popup is up, even if something else (pause menu resume,
        // ASK reminder close) set timeScale back to 1.
        if (currentPopup != null && speedRoutine == null && Time.timeScale != 0f)
        {
            Time.timeScale = 0f;
        }
    }

    void OnDestroy()
    {
        // Never leave the next scene frozen if the tutorial is exited mid-popup.
        Time.timeScale = 1f;
    }

    public void StartTutorial()
    {
        if (tutorialRoutine != null) StopCoroutine(tutorialRoutine); // guard against double start
        tutorialRoutine = StartCoroutine(Tutorial());
    }
    public void FindDeviceSize()
    {
        deviceSize = new Vector2(Screen.width, Screen.height);
    }

    // Positions are given as a fraction of the screen so they stay correct after a resize/rotation.
    void OpenPopup(string message, Vector2 screenFraction, Vector2 size, int imageIndex, Color backgroundColor, float backgroundOpacity)
    {
        if (currentPopup != null) Destroy(currentPopup); // never stack two popups
        FindDeviceSize();
        continueTutorial = false;
        Vector2 position = new Vector2(deviceSize.x * screenFraction.x, deviceSize.y * screenFraction.y);
        currentPopup = Instantiate(tutorialPopupPrefab, position, Quaternion.identity, tutorialCanvas.transform);
        currentPopup.GetComponent<RectTransform>().sizeDelta = size + new Vector2(0f, 30f); // extra room for the bigger button
        currentPopup.transform.Find("TutorialText").GetComponent<TextMeshProUGUI>().text = message;
        StylePopupButton(currentPopup);
        if (tutorialBackground != null)
        {
            if (imageIndex >= 0 && imageIndex < tutorialImages.Length) tutorialBackground.sprite = tutorialImages[imageIndex];
            tutorialBackground.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, currentBgOpacity / 100f);
        }
        StartSpeed(0f, 0.6f);
        StartBg(backgroundOpacity, 0.2f);
    }
    public void ClosePopup()
    {
        if (currentPopup == null) return; // ignore double taps / stray clicks
        Destroy(currentPopup);
        currentPopup = null;
        continueTutorial = true;
        StartSpeed(1f, 0.6f);
        StartBg(0f, 0.2f);
        currentTutorialIndex++;
    }

    void StartSpeed(float endSpeed, float duration)
    {
        if (speedRoutine != null) StopCoroutine(speedRoutine);
        speedRoutine = StartCoroutine(LerpSpeed(Time.timeScale, endSpeed, duration));
    }
    void StartBg(float endOpacity, float duration)
    {
        if (tutorialBackground == null) return;
        if (bgRoutine != null) StopCoroutine(bgRoutine);
        bgRoutine = StartCoroutine(LerpBg(currentBgOpacity, endOpacity, duration));
    }

    IEnumerator LerpSpeed(float startSpeed, float endSpeed, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            Time.timeScale = Mathf.Lerp(startSpeed, endSpeed, elapsedTime / duration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        Time.timeScale = endSpeed;
        speedRoutine = null;
    }
    IEnumerator LerpBg(float startOpacity, float endOpacity, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            currentBgOpacity = Mathf.Lerp(startOpacity, endOpacity, elapsedTime / duration);
            SetBgAlpha(currentBgOpacity);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        currentBgOpacity = endOpacity;
        SetBgAlpha(endOpacity);
        bgRoutine = null;
    }
    void SetBgAlpha(float opacity)
    {
        Color c = tutorialBackground.color;
        tutorialBackground.color = new Color(c.r, c.g, c.b, opacity / 100f);
    }

    // Opens a popup and waits until the player closes it.
    IEnumerator Popup(string message, Vector2 screenFraction, Vector2 size, int imageIndex, Color backgroundColor, float backgroundOpacity)
    {
        // Don't open on top of the ASK reminder or while a conversation is on screen.
        yield return new WaitUntil(() => !ReminderOpen());
        OpenPopup(message, screenFraction, size, imageIndex, backgroundColor, backgroundOpacity);
        yield return new WaitUntil(() => continueTutorial);
    }

    static bool ReminderOpen()
    {
        return AskReminderPopup.Instance != null && AskReminderPopup.Instance.IsOpen;
    }

    NPCBehavior FindTutorialNPC()
    {
        GameObject named = GameObject.Find(tutorialNPCName);
        if (named != null && named.GetComponent<NPCBehavior>() != null) return named.GetComponent<NPCBehavior>();
        foreach (NPCBehavior npc in FindObjectsByType<NPCBehavior>(FindObjectsSortMode.None))
        {
            if (npc.isShoplifter) return npc;
        }
        Debug.LogWarning("TutorialManager: tutorial NPC not found.");
        return null;
    }

    Camera ButtonCamera()
    {
        Canvas c = interactButton.GetComponentInParent<Canvas>();
        return c != null && c.renderMode != RenderMode.ScreenSpaceOverlay ? c.worldCamera : null;
    }

    static bool IsTalking()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        PlayerInteraction pi = p != null ? p.GetComponent<PlayerInteraction>() : null;
        return pi != null && pi.isBusy;
    }

    void MarkDone(TextMeshProUGUI text)
    {
        if (text == null) return;
        text.fontStyle = FontStyles.Strikethrough;
        text.color = Done;
    }

    IEnumerator Tutorial()
    {
        // Realtime waits everywhere: scaled waits stall forever if timeScale is ever left at 0.
        yield return new WaitForSecondsRealtime(StepDelay);
        yield return Popup("Welcome to ASK First! Let's get you familiar with the controls and objectives!", new Vector2(0.5f, 0.5f), new Vector2(360, 150), 0, DarkBg, 50f);

        //Controls Tutorial — one skill at a time: show it, then let the player actually do it.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Transform cam = Camera.main != null ? Camera.main.transform : null;
        if (objectiveAnimator != null) objectiveAnimator.SetBool("ObjectiveIn", false);
        yield return new WaitForSecondsRealtime(StepDelay);

        // 1) Moving
        if (hints != null) hints.ShowFingerDrag(JoystickScreenPos, new[] { new Vector2(0f, 45f), new Vector2(35f, 25f), new Vector2(0f, 45f) });
        yield return Popup("Let's start by walking.\n\nPress and HOLD the joystick in the bottom left, then slide your thumb the way you want to go.\n(On a computer: W A S D keys)", new Vector2(0.3f, 0.68f), new Vector2(360, 180), 1, new Color(1f, 1f, 1f, 0.5f), 100f);
        if (objectiveAnimator != null) objectiveAnimator.SetBool("ObjectiveIn", true);
        float walked = 0f;
        Vector3 lastPos = player.transform.position;
        while (walked < requiredMoveDistance)
        {
            Vector3 delta = player.transform.position - lastPos;
            delta.y = 0f; // ignore falling / stepping
            walked += delta.magnitude;
            lastPos = player.transform.position;
            yield return null;
        }
        if (hints != null) hints.HideFinger();
        MarkDone(objective1Text);
        yield return new WaitForSecondsRealtime(0.8f); // let them see the tick before the next instruction

        // 2) Looking around
        if (hints != null) hints.ShowFingerDrag(LookAreaScreenPos, new[] { new Vector2(-80f, 0f), new Vector2(80f, 0f) });
        yield return Popup("Nice walking!\n\nNow put your finger on the RIGHT side of the screen and drag it left and right to look around.\n(On a computer: move the mouse)", new Vector2(0.3f, 0.68f), new Vector2(360, 180), 1, new Color(1f, 1f, 1f, 0.5f), 100f);
        float looked = 0f;
        Quaternion lastRot = cam != null ? cam.rotation : player.transform.rotation;
        while (looked < requiredLookAngle)
        {
            // The camera includes both turning and looking up/down.
            Quaternion rot = cam != null ? cam.rotation : player.transform.rotation;
            looked += Quaternion.Angle(lastRot, rot);
            lastRot = rot;
            yield return null;
        }
        if (hints != null) hints.HideFinger();
        MarkDone(objective2Text);
        yield return new WaitForSecondsRealtime(0.8f);

        if (objectiveAnimator != null) objectiveAnimator.SetBool("ObjectiveIn", false);
        yield return new WaitForSecondsRealtime(StepDelay);
        yield return Popup("Good Job! Now walk up to a person and look straight at them!", new Vector2(0.5f, 0.5f), new Vector2(360, 150), 0, DarkBg, 50f);

        //Teach interact button — spotlight the real button instead of describing where it is.
        NPCBehavior tutorialNPC = FindTutorialNPC();
        yield return new WaitUntil(() => interactButton.activeSelf || NPCDone(tutorialNPC));
        if (!NPCDone(tutorialNPC))
        {
            RectTransform buttonRect = interactButton.GetComponent<RectTransform>();
            Vector2 buttonScreen = RectTransformUtility.WorldToScreenPoint(ButtonCamera(), buttonRect.position);
            float popupX = buttonScreen.x > Screen.width * 0.5f ? 0.32f : 0.68f; // keep the popup away from the button
            if (hints != null) hints.ShowSpotlight(buttonRect, true);
            // Background opacity 0: the spotlight provides the dimming, with a hole over the button.
            yield return Popup("This round button appears when you're looking at someone.\n\nTap the glowing button to start talking to them!", new Vector2(popupX, 0.6f), new Vector2(360, 170), 0, DarkBg, 0f);
            // Keep a pulsing ring on the button (no dimming) until they've used it.
            if (hints != null) hints.ShowSpotlight(buttonRect, false);
        }

        //NPC Leaving
        yield return new WaitUntil(() => NPCDone(tutorialNPC) || IsTalking());
        if (hints != null) hints.HideSpotlight();
        yield return new WaitUntil(() => NPCDone(tutorialNPC));
        yield return Popup("Great! Now that you've interacted with the person, they will leave the store.", new Vector2(0.5f, 0.5f), new Vector2(360, 150), 0, DarkBg, 50f);

        //Explain level layout
        yield return Popup("Each level will have multiple potential shoplifters. Talk to 2 or 3 to complete the level!", new Vector2(0.5f, 0.5f), new Vector2(360, 150), 0, DarkBg, 50f);
        //Remind about ASK First
        yield return Popup("Remember to practice the ASK protocol!", new Vector2(0.5f, 0.5f), new Vector2(360, 150), 0, DarkBg, 50f);

        //End tutorial
        yield return new WaitForSecondsRealtime(StepDelay);
        GameManager.Instance.mistakesMade = new Mistake[3];
        for (int i = 0; i < GameManager.Instance.mistakesMade.Length; i++)
        {
            GameManager.Instance.mistakesMade[i] = new Mistake("", "", true, -1); // Initialize with default values
        }
        tutorialRoutine = null;
        GameManager.Instance.EndLevel(10, "tutorial");
    }

    // A destroyed NPC (it walked out the exit) counts as done so the tutorial can't get stuck.
    static bool NPCDone(NPCBehavior npc)
    {
        return npc == null || npc.interactedWith;
    }
}
