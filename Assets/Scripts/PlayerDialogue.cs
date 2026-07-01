using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
public class PlayerDialogue : MonoBehaviour
{
    Canvas dialogueUI;
    GameObject mobileControls;
    bool advanceDialogue = false;
    bool textScrolling = false;
    bool moodUpdating = false;
    [SerializeField]
    TextMeshProUGUI dialogueText;
    [SerializeField]
    TextMeshProUGUI option1Text;
    [SerializeField]
    TextMeshProUGUI option2Text;
    [SerializeField]
    TextMeshProUGUI option3Text;
    [SerializeField]
    TextMeshProUGUI option4Text;
    [SerializeField]
    public float mood;
    Material moodSlider;
    int optionPicked;
    void Start()
    {
        dialogueUI = GameObject.FindGameObjectWithTag("DialogueUI").GetComponent<Canvas>();
        dialogueUI.enabled = false;
        mobileControls = GameObject.FindGameObjectWithTag("MobileControls");
        dialogueText = dialogueUI.transform.Find("DialogueBox/DialogueText").GetComponent<TextMeshProUGUI>();
        option1Text = dialogueUI.transform.Find("Option 1").GetChild(0).GetComponent<TextMeshProUGUI>();
        option2Text = dialogueUI.transform.Find("Option 2").GetChild(0).GetComponent<TextMeshProUGUI>();
        option3Text = dialogueUI.transform.Find("Option 3").GetChild(0).GetComponent<TextMeshProUGUI>();
        option4Text = dialogueUI.transform.Find("Option 4").GetChild(0).GetComponent<TextMeshProUGUI>();
        mood = 5;
        moodSlider = dialogueUI.transform.Find("Mood/FillShader").GetComponent<Image>().material;
        moodSlider.SetFloat("_stepValue", mood);
    }
    public void StartDialogue(GameObject npc)
    {

        GameManager.Instance.DisplayDialogueUI();
        StartCoroutine(DialogueCoroutine(npc));

    }
    public void EndDialogue(GameObject npc)
    {
        GameManager.Instance.DisplayNormalUI();
        mobileControls.GetComponent<MobileControls>().InteractEvent.Invoke(false);
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInteraction>().isBusy = false;
        GameObject.FindGameObjectWithTag("NPCFocusCamera").GetComponent<NPCFocusCamera>().ResetFocus();
    }
    /*
    IEnumerator DialogueCoroutine(GameObject npc)
    {
        DialogueLines dialogueLine = null;

        while (true)
        {
            if (dialogueLine == null)
            {
                dialogueLine = npc.GetComponent<NPCDialogue>().AdvanceDialogue(0);

            }
            else if (dialogueLine.option1NextId == -1)
            {
                EndDialogue(npc);
                break;
            }
            else if (dialogueLine.option1NextId == -2)
            {
                GameManager.Instance.EndLevel(mood, npc.name);
                break;
            }
            else if (dialogueLine.optionsBypass == true)
            {
                dialogueLine = npc.GetComponent<NPCDialogue>().AdvanceDialogue(dialogueLine.option1NextId);

            }
            else if (dialogueLine.optionsBypass == false)
            {
                switch (optionPicked)
                {
                    case 1:
                        GameManager.Instance.AddMistake(dialogueLine.option1MistakeId);
                        dialogueLine = npc.GetComponent<NPCDialogue>().AdvanceDialogue(dialogueLine.option1NextId);
                        break;
                    case 2:
                        GameManager.Instance.AddMistake(dialogueLine.option2MistakeId);
                        dialogueLine = npc.GetComponent<NPCDialogue>().AdvanceDialogue(dialogueLine.option2NextId);
                        break;
                    case 3:
                        GameManager.Instance.AddMistake(dialogueLine.option3MistakeId);
                        dialogueLine = npc.GetComponent<NPCDialogue>().AdvanceDialogue(dialogueLine.option3NextId);
                        break;
                    case 4:
                        GameManager.Instance.AddMistake(dialogueLine.option4MistakeId);
                        dialogueLine = npc.GetComponent<NPCDialogue>().AdvanceDialogue(dialogueLine.option4NextId);
                        break;
                }


            }

            if (dialogueLine == null)
            {
                break;
            }
            else
            {
                Coroutine textScroll = StartCoroutine(TextScrolling(dialogueLine.line));
                if (dialogueLine.optionsBypass == false)
                {
                    //Enable option buttons
                    option2Text.transform.parent.gameObject.SetActive(true);
                    option3Text.transform.parent.gameObject.SetActive(true);
                    option4Text.transform.parent.gameObject.SetActive(true);
                    //Set option text
                    option1Text.text = dialogueLine.option1;
                    option2Text.text = dialogueLine.option2;
                    option3Text.text = dialogueLine.option3;
                    option4Text.text = dialogueLine.option4;
                }
                else
                {
                    //Disable option buttons
                    option2Text.transform.parent.gameObject.SetActive(false);
                    option3Text.transform.parent.gameObject.SetActive(false);
                    option4Text.transform.parent.gameObject.SetActive(false);
                    //Clear option text
                    option1Text.text = "...";
                    option2Text.text = "";
                    option3Text.text = "";
                    option4Text.text = "";
                }
                yield return new WaitUntil(() => advanceDialogue);
                if (textScrolling == true)
                {
                    textScrolling = false;
                    advanceDialogue = false;
                    StopCoroutine(textScroll);
                    dialogueText.text = dialogueLine.line;
                    yield return new WaitUntil(() => advanceDialogue);
                }
                StopCoroutine(textScroll);
                advanceDialogue = false;
                if (dialogueLine.optionsBypass == false)
                {
                    switch (optionPicked)
                    {
                        case 1:
                            StartCoroutine(MoodChange(dialogueLine.option1MoodChange));
                            break;
                        case 2:
                            StartCoroutine(MoodChange(dialogueLine.option2MoodChange));
                            break;
                        case 3:
                            StartCoroutine(MoodChange(dialogueLine.option3MoodChange));
                            break;
                        case 4:
                            StartCoroutine(MoodChange(dialogueLine.option4MoodChange));
                            break;
                    }
                }
            }
        }




    }
    */
    IEnumerator DialogueCoroutine(GameObject npc)
    {
        DialogueLines dialogueLine = null;

        while (true)
        {
            if (dialogueLine == null)
            {
                dialogueLine = npc.GetComponent<NPCDialogue>().AdvanceDialogue(0);

            }
            else if (dialogueLine.option1NextId == -1)
            {
                EndDialogue(npc);
                break;
            }
            else if (dialogueLine.option1NextId == -2)
            {
                GameManager.Instance.EndLevel(mood, npc.name);
                break;
            }
            else if (dialogueLine.optionsBypass == true)
            {
                dialogueLine = npc.GetComponent<NPCDialogue>().AdvanceDialogue(dialogueLine.option1NextId);

            }
            else if (dialogueLine.optionsBypass == false)
            {
                switch (optionPicked)
                {
                    case 1:
                        GameManager.Instance.AddMistake(dialogueLine.option1MistakeId);
                        dialogueLine = npc.GetComponent<NPCDialogue>().AdvanceDialogue(dialogueLine.option1NextId);
                        break;
                    case 2:
                        GameManager.Instance.AddMistake(dialogueLine.option2MistakeId);
                        dialogueLine = npc.GetComponent<NPCDialogue>().AdvanceDialogue(dialogueLine.option2NextId);
                        break;
                }


            }

            if (dialogueLine == null)
            {
                break;
            }
            else
            {
                Coroutine textScroll = StartCoroutine(TextScrolling(dialogueLine.line));
                if (dialogueLine.optionsBypass == false)
                {
                    //Enable option buttons
                    option2Text.transform.parent.gameObject.SetActive(true);
                    //Set option text
                    option1Text.text = dialogueLine.option1;
                    option2Text.text = dialogueLine.option2;
                }
                else
                {
                    //Disable option buttons
                    option2Text.transform.parent.gameObject.SetActive(false);
                    //Clear option text
                    option1Text.text = "...";
                    option2Text.text = "";
                }
                yield return new WaitUntil(() => advanceDialogue);
                if (textScrolling == true)
                {
                    textScrolling = false;
                    advanceDialogue = false;
                    StopCoroutine(textScroll);
                    dialogueText.text = dialogueLine.line;
                    yield return new WaitUntil(() => advanceDialogue);
                }
                StopCoroutine(textScroll);
                advanceDialogue = false;
                if (dialogueLine.optionsBypass == false)
                {
                    switch (optionPicked)
                    {
                        case 1:
                            StartCoroutine(MoodChange(dialogueLine.option1MoodChange));
                            break;
                        case 2:
                            StartCoroutine(MoodChange(dialogueLine.option2MoodChange));
                            break;
                    }
                }
            }
        }




    }
    IEnumerator TextScrolling(string dialogueLine)
    {

        textScrolling = true;
        dialogueText.text = "";
        foreach (char c in dialogueLine)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.05f);
        }
        textScrolling = false;
    }
    public void OptionSelect(int optionId)
    {
        advanceDialogue = true;
        optionPicked = optionId;
    }
    IEnumerator MoodChange(int moodChange)
    {
        if (moodUpdating)
        {
            yield return new WaitUntil(() => moodUpdating == false);
        }
        moodUpdating = true;
        float elapsedTime = 0f;
        float startValue = mood;
        float targetValue = Mathf.Clamp(mood + moodChange, 0, 10);
        while (elapsedTime < 0.4f)
        {
            mood = Mathf.Lerp(startValue, targetValue, elapsedTime / 0.4f);
            moodSlider.SetFloat("_stepValue", mood);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        mood = targetValue;
        moodUpdating = false;
    }
}
