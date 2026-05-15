using UnityEngine;
using System.Collections;
public class PlayerDialogue : MonoBehaviour
{
    Canvas dialogueUI;
    GameObject mobileControls;
    bool advanceDialogue = false;
    bool textScrolling = false;
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
        string dialogueLine;
        while (true)
        {
            dialogueLine=npc.GetComponent<NPCDialogue>().AdvanceDialogue();
            if (dialogueLine == null)
            {
                break;
            }
            else
            {
                Coroutine textScroll = StartCoroutine(TextScrolling(dialogueLine));
                yield return new WaitUntil(() => advanceDialogue);
                if (textScrolling == true)
                {
                    textScrolling = false;
                    advanceDialogue = false;
                    StopCoroutine(textScroll);
                    dialogueUI.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = dialogueLine;
                    yield return new WaitUntil(() => advanceDialogue);
                }
                advanceDialogue = false;
                
            }
        }
        

        EndDialogue(npc);
        
    }
    IEnumerator TextScrolling(string dialogueLine)
    {
        textScrolling = true;
        dialogueUI.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "";
        foreach (char c in dialogueLine)
        {
            dialogueUI.GetComponentInChildren<TMPro.TextMeshProUGUI>().text += c;
            yield return new WaitForSeconds(0.05f);
        }
        textScrolling = false;
    }
    public void SkipDialogue()
    {
        advanceDialogue = true;
    }
}
