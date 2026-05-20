using UnityEngine;
using System.Collections;
using TMPro;
public class PlayerDialogue : MonoBehaviour
{
    Canvas dialogueUI;
    GameObject mobileControls;
    bool advanceDialogue = false;
    bool textScrolling = false;
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
    int optionPicked;
    void Start()
    {
        dialogueUI = GameObject.FindGameObjectWithTag("DialogueUI").GetComponent<Canvas>();
        dialogueUI.enabled = false;
        mobileControls = GameObject.FindGameObjectWithTag("MobileControls");
    }
    public void StartDialogue(GameObject npc)
    {
        dialogueUI.enabled = true;
        StartCoroutine(DialogueCoroutine(npc));
    }
    public void EndDialogue(GameObject npc)
    {
        dialogueUI.enabled = false;
        mobileControls.GetComponent<MobileControls>().InteractEvent.Invoke(false);
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInteraction>().isBusy = false;
        
    }
    IEnumerator DialogueCoroutine(GameObject npc)
    {
        DialogueLines dialogueLine=null;
        Debug.Log(npc);
        while (true)
        {
            if (dialogueLine == null)
            {
                dialogueLine = npc.GetComponent<NPCDialogue>().AdvanceDialogue(0);
                Debug.Log(dialogueLine);
            }
            else if (dialogueLine.option1NextId == -1)
            {
                EndDialogue(npc);
            }
            else if (dialogueLine.optionsBypass == true)
            {
                dialogueLine=npc.GetComponent<NPCDialogue>().AdvanceDialogue(dialogueLine.option1NextId);
                Debug.Log(dialogueLine);
            }
            else if (dialogueLine.optionsBypass == false)
            {
                Debug.Log("Option picked: " + optionPicked);
                switch (optionPicked)
                {
                    case 1:
                        dialogueLine = npc.GetComponent<NPCDialogue>().AdvanceDialogue(dialogueLine.option1NextId);
                        break;
                    case 2:
                        dialogueLine = npc.GetComponent<NPCDialogue>().AdvanceDialogue(dialogueLine.option2NextId);
                        break;
                    case 3:
                        dialogueLine = npc.GetComponent<NPCDialogue>().AdvanceDialogue(dialogueLine.option3NextId);
                        break;
                    case 4:
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
                        option1Text.text = dialogueLine.option1;
                        option2Text.text = dialogueLine.option2;
                        option3Text.text = dialogueLine.option3;
                        option4Text.text = dialogueLine.option4;
                }
                else
                {
                    option1Text.text = "";
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
                advanceDialogue = false;
                
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
}
