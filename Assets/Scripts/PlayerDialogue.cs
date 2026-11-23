using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
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
    RectTransform dialogueBox, option1Button, option2Button;
    Vector2 option1Home, option2Home; // layout positions from the prefab
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
        dialogueBox = (RectTransform)dialogueText.transform.parent;
        option1Button = (RectTransform)option1Text.transform.parent;
        option2Button = (RectTransform)option2Text.transform.parent;
        option1Home = option1Button.anchoredPosition;
        option2Home = option2Button.anchoredPosition;
        mood = 10;
        moodSlider = dialogueUI.transform.Find("Mood/FillShader").GetComponent<Image>().material;
        moodSlider.SetFloat("_stepValue", mood);
    }
    public void StartDialogue(GameObject npc)
    {

        GameManager.Instance.DisplayDialogueUI();
        // The background-customer intro moves these; always start from the normal layout.
        dialogueBox.localScale = Vector3.one;
        option1Button.anchoredPosition = option1Home;
        option2Button.anchoredPosition = option2Home;
        AmbientNPC ambient = npc.GetComponent<AmbientNPC>();
        StartCoroutine(ambient != null ? AmbientDialogueCoroutine(npc, ambient) : DialogueCoroutine(npc));

    }
    public void EndDialogue(GameObject npc)
    {
        GameManager.Instance.DisplayNormalUI();
        mobileControls.GetComponent<MobileControls>().InteractEvent.Invoke(false);
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInteraction>().isBusy = false;
        GameObject.FindGameObjectWithTag("NPCFocusCamera").GetComponent<NPCFocusCamera>().ResetFocus();
        NPCBehavior npcBehavior = npc.GetComponent<NPCBehavior>();
        npcBehavior.interactedWith = true;
        npcBehavior.EndEncounter();
        if (!npcBehavior.isShoplifter)
        {
            npcBehavior.ChangeState(NPCState.Idle);
        }
        else
        {
            npcBehavior.ChangeState(NPCState.Leaving);
        }
        if (npcBehavior.safeDistanceBreached)
        {
            // Any breach shows the reminder, but only getting really close counts against the score
            if (npcBehavior.penaltyDistanceBreached) GameManager.Instance.AddMistake(4);
            if (AskReminderPopup.Instance != null) AskReminderPopup.Instance.Show();
        }

    }
    IEnumerator DialogueCoroutine(GameObject npc)
    {
        DialogueLines dialogueLine = null;
        NPCAnimator npcAnimator = npc.GetComponent<NPCAnimator>();

        while (true)
        {
            if (dialogueLine == null)
            {
                dialogueLine = npc.GetComponent<NPCDialogue>().AdvanceDialogue(0);

            }
            else
            {
                bool first = optionPicked != 2; // bypass lines only have option 1
                GameManager.Instance.AddMistake(first ? dialogueLine.option1MistakeId : dialogueLine.option2MistakeId);
                int nextId = first ? dialogueLine.option1NextId : dialogueLine.option2NextId;
                if (nextId < 0) // -1 = end conversation, -2 = end and count the objective
                {
                    EndDialogue(npc);
                    if (nextId == -2)
                        GameManager.Instance.GetComponent<LevelObjectives>().UpdateObjectives(npc.GetComponent<NPCBehavior>(), mood);
                    yield break;
                }
                dialogueLine = npc.GetComponent<NPCDialogue>().AdvanceDialogue(nextId);
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
                    option1Text.text = "Next →";
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
                    int moodChange = optionPicked == 2 ? dialogueLine.option2MoodChange : dialogueLine.option1MoodChange;
                    StartCoroutine(MoodChange(moodChange));
                    npcAnimator.Animate(moodChange >= 0 ? NPCAnimation.Happy : NPCAnimation.Upset);
                    SoundManager.Instance.PlaySound(Resources.Load<AudioClip>(moodChange >= 0 ? "SFX/goodchoice" : "SFX/badchoice"));
                }
            }
        }




    }
    // Player speaks first: pick an approach line, NPC replies, player closes with closingLine.
    IEnumerator AmbientDialogueCoroutine(GameObject npc, AmbientNPC ambient)
    {
        bool twoOptions = ambient.approaches.Length > 1;
        // No box yet: the approach lines sit centred vertically until one is picked.
        dialogueBox.localScale = Vector3.zero;
        dialogueText.text = "";
        float centreShift = -(twoOptions ? (option1Home.y + option2Home.y) / 2f : option1Home.y);
        option1Button.anchoredPosition = option1Home + Vector2.up * centreShift;
        option2Button.anchoredPosition = option2Home + Vector2.up * centreShift;
        option1Text.text = ambient.approaches[0].playerLine;
        option2Text.text = twoOptions ? ambient.approaches[1].playerLine : "";
        option2Text.transform.parent.gameObject.SetActive(twoOptions);
        advanceDialogue = false;
        yield return new WaitUntil(() => advanceDialogue);
        advanceDialogue = false;

        npc.GetComponent<NPCAnimator>().Animate(NPCAnimation.Happy);
        option2Text.transform.parent.gameObject.SetActive(false);
        option1Text.text = ambient.closingLine;
        string reply = ambient.approaches[Mathf.Clamp(optionPicked - 1, 0, ambient.approaches.Length - 1)].npcReply;
        StartCoroutine(RevealDialogueBox(option1Button.anchoredPosition));
        Coroutine textScroll = StartCoroutine(TextScrolling(reply));
        yield return new WaitUntil(() => advanceDialogue);
        if (textScrolling) // first tap skips the scroll, like the main dialogue
        {
            StopCoroutine(textScroll);
            textScrolling = false;
            dialogueText.text = reply;
            advanceDialogue = false;
            yield return new WaitUntil(() => advanceDialogue);
        }
        advanceDialogue = false;
        EndDialogue(npc);
    }
    // Box pops in to full size while the button slides back to its normal spot below it.
    IEnumerator RevealDialogueBox(Vector2 buttonFrom)
    {
        const float duration = 0.35f;
        for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
        {
            float back = 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f); // ease-out-back: slight overshoot
            dialogueBox.localScale = Vector3.one * back;
            option1Button.anchoredPosition = Vector2.Lerp(buttonFrom, option1Home, 1f - Mathf.Pow(1f - t, 3f));
            yield return null;
        }
        dialogueBox.localScale = Vector3.one;
        option1Button.anchoredPosition = option1Home;
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
        if (optionId == 1)
        {
            SoundManager.Instance.PlaySound(Resources.Load<AudioClip>("pop"));
        }
        else if (optionId == 2)
        {
            SoundManager.Instance.PlaySound(Resources.Load<AudioClip>("pop"));
        }


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
