using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    string currentDialogueLine;
    string[] dialogueLines;
    int dialogueIndex = -1;
    void Start()
    {
        TextAsset dialogueFile = Resources.Load<TextAsset>("ASKFirstTest");
        string[] allLines = dialogueFile.text.Split('\n');
        for (int i = 0; i < allLines.Length; i++)
        {
            if (allLines[i].StartsWith(gameObject.name+","))
            {
                Debug.Log(gameObject.name + ":" + allLines[i]);
                dialogueLines = allLines[i].Trim().Split(',',System.StringSplitOptions.RemoveEmptyEntries);
                dialogueLines = dialogueLines[1..];
            }
        }
    }
    public string AdvanceDialogue()
    {
        dialogueIndex++;
        if (dialogueIndex < dialogueLines.Length)
        {
            currentDialogueLine = dialogueLines[dialogueIndex];
            return currentDialogueLine;
        }
        else
        {           
            dialogueIndex = -1; // Reset for next interaction  
            return null;
        }
    }
}
