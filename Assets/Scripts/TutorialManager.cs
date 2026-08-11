using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
public class TutorialManager : MonoBehaviour
{
    public GameObject currentPopup;
    [SerializeField] GameObject tutorialPopupPrefab;
    GameObject tutorialCanvas;
    Vector2 deviceSize;
    [SerializeField] Sprite[] tutorialImages;
    int currentTutorialIndex = 0;
    bool continueTutorial = false;
    Animator objectiveAnimator;
    GameManager tutorialObjectives;
    TextMeshProUGUI objective1Text;
    TextMeshProUGUI objective2Text;
    [SerializeField]
    GameObject interactButton;
    void Start()
    {
        tutorialCanvas = GameObject.Find("TutorialCanvas");
        objectiveAnimator = GameObject.Find("TutorialObjectives").GetComponent<Animator>();
        tutorialObjectives = GameObject.Find("TutorialObjectives").GetComponent<GameManager>();
        objective1Text = GameObject.Find("Objective1").GetComponent<TextMeshProUGUI>();
        objective2Text = GameObject.Find("Objective2").GetComponent<TextMeshProUGUI>();
        FindDeviceSize();
    }
    public void StartTutorial()
    {
        StartCoroutine(Tutorial());
    }
    public void FindDeviceSize()
    {
        deviceSize = new Vector2(Screen.width, Screen.height);

    }
    void OpenPopup(string message, Vector2 position, Vector2 size, int imageIndex, Color backgroundColor, float backgroundOpacity)
    {
        continueTutorial = false;
        currentPopup = null; // Replace with your popup instantiation logic
        currentPopup = Instantiate(tutorialPopupPrefab, position, Quaternion.identity, tutorialCanvas.transform);
        currentPopup.GetComponent<RectTransform>().sizeDelta = size;
        currentPopup.transform.Find("TutorialText").GetComponent<TextMeshProUGUI>().text = message;
        Image backgroundcolor = tutorialCanvas.transform.Find("Background").GetComponent<Image>();
        backgroundcolor.sprite = tutorialImages[imageIndex]; // Example of setting the tutorial image
        backgroundcolor.color = backgroundColor;
        StartCoroutine(LerpSpeed(1f, 0f, 0.6f)); // Example of slowing down time when popup opens
        StartCoroutine(LerpBg(0f, backgroundOpacity, 0.2f)); // Example of changing background opacity when popup opens
    }
    public void ClosePopup()
    {
        continueTutorial = true;
        Destroy(currentPopup);
        currentPopup = null;
        // Implement the logic to close the popup
        Debug.Log("Popup closed");
        StartCoroutine(LerpSpeed(0f, 1f, 0.6f)); // Example of restoring time when popup closes
        StartCoroutine(LerpBg(50f, 0f, 0.2f)); // Example of changing background opacity when popup closes
        currentTutorialIndex++;
    }
    IEnumerator LerpSpeed(float startSpeed, float endSpeed, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            Time.timeScale = Mathf.Lerp(startSpeed, endSpeed, t);
            elapsedTime += Time.unscaledDeltaTime; // Use unscaledDeltaTime to ensure the lerp progresses even when timeScale is changed
            yield return null;
        }
        // Ensure the final speed is set to endSpeed
        Time.timeScale = endSpeed;
        Debug.Log("Final Speed: " + endSpeed);
    }
    IEnumerator LerpBg(float startOpacity, float endOpacity, float duration)
    {
        float elapsedTime = 0f;
        Image backgroundcolor = tutorialCanvas.transform.Find("Background").GetComponent<Image>();
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            backgroundcolor.color = new Color(backgroundcolor.color.r, backgroundcolor.color.g, backgroundcolor.color.b, Mathf.Lerp(startOpacity / 100f, endOpacity / 100f, t));
            elapsedTime += Time.unscaledDeltaTime; // Use unscaledDeltaTime to ensure the lerp progresses even when timeScale is changed
            yield return null;
        }
        // Ensure the final opacity is set to endOpacity
        backgroundcolor.color = new Color(backgroundcolor.color.r, backgroundcolor.color.g, backgroundcolor.color.b, endOpacity / 100f);
        Debug.Log("Final Opacity: " + endOpacity);
    }
    IEnumerator Tutorial()
    {
        yield return new WaitForSeconds(0.5f);
        OpenPopup("Welcome to ASK First! Let's get you familiar with the controls and objectives!", new Vector2(deviceSize.x * 0.5f, deviceSize.y * 0.5f), new Vector2(360, 150), 0, new Color(0f / 255f, 4f / 255f, 71f / 255f, 0f), 50f);
        yield return new WaitUntil(() => continueTutorial == true);
        //Controls Tutorial
        yield return new WaitForSeconds(0.5f);
        OpenPopup("Use the joystick to move your character around the environment. \n\n Drag your finger on the right side of the screen to look around", new Vector2(deviceSize.x * 0.25f, deviceSize.y * 0.75f), new Vector2(360, 180), 1, new Color(1f, 1f, 1f, 0.5f), 100f);

        objectiveAnimator.SetBool("ObjectiveIn", false);
        yield return new WaitUntil(() => continueTutorial == true);
        //Objective Tutorial

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 startPosition = player.transform.position;
        Quaternion startRotation = player.transform.rotation;
        objectiveAnimator.SetBool("ObjectiveIn", true);
        bool hasMoved = false;
        bool hasLooked = false;

        while (!hasMoved || !hasLooked)
        {
            // Check movement
            if (Vector3.Distance(startPosition, player.transform.position) > 0.1f)
            {
                hasMoved = true;
                objective1Text.fontStyle = FontStyles.Strikethrough;
                objective1Text.color = new Color(0f / 255f, 255f / 255f, 0f / 255f, 1f);

            }

            // Check looking
            if (Quaternion.Angle(startRotation, player.transform.rotation) > 5f)
            {
                hasLooked = true;
                objective2Text.fontStyle = FontStyles.Strikethrough;
                objective2Text.color = new Color(0f / 255f, 255f / 255f, 0f / 255f, 1f);
            }

            yield return null;
        }
        objectiveAnimator.SetBool("ObjectiveIn", false);
        yield return new WaitForSeconds(0.5f);
        OpenPopup("Good Job! Now approach a person and look straight at them!", new Vector2(deviceSize.x * 0.5f, deviceSize.y * 0.5f), new Vector2(360, 150), 0, new Color(0f / 255f, 4f / 255f, 71f / 255f, 0f), 50f);

        //Teach interact button
        yield return new WaitUntil(() => interactButton.activeSelf == true);
        OpenPopup("Tap the interact button to start a conversation with the person!", new Vector2(deviceSize.x * 0.7f, deviceSize.y * 0.75f), new Vector2(360, 150), 2, new Color(1, 1, 1, 50f), 100f);
        yield return new WaitUntil(() => continueTutorial == true);
        //NPC Leaving
        yield return new WaitUntil(() => GameObject.Find("Tutorial").GetComponent<NPCBehavior>().interactedWith == true);
        OpenPopup("Great! Now that you've interacted with the person, they will leave the store.", new Vector2(deviceSize.x * 0.5f, deviceSize.y * 0.5f), new Vector2(360, 150), 0, new Color(0f / 255f, 4f / 255f, 71f / 255f, 0f), 50f);
        yield return new WaitUntil(() => continueTutorial == true);

        //Explain level layout
        OpenPopup("Each level will have multiple potential shoplifters. Talk to 2 or 3 to complete the level!", new Vector2(deviceSize.x * 0.5f, deviceSize.y * 0.5f), new Vector2(360, 150), 0, new Color(0f / 255f, 4f / 255f, 71f / 255f, 0f), 50f);
        yield return new WaitUntil(() => continueTutorial == true);
        //Remind about ASK First
        OpenPopup("Remember to practice the ASK protocol!", new Vector2(deviceSize.x * 0.5f, deviceSize.y * 0.5f), new Vector2(360, 150), 0, new Color(0f / 255f, 4f / 255f, 71f / 255f, 0f), 50f);
        //End tutorial
        yield return new WaitUntil(() => continueTutorial == true);
        yield return new WaitForSeconds(0.5f);
        GameManager.Instance.mistakesMade = new Mistake[3];
        for (int i = 0; i < GameManager.Instance.mistakesMade.Length; i++)
        {
            GameManager.Instance.mistakesMade[i] = new Mistake("", "", true, -1); // Initialize with default values
        }
        GameManager.Instance.EndLevel(10, "tutorial");
    }
}
