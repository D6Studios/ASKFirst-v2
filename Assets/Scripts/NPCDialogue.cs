using Unity.Android.Gradle;
using UnityEngine;
using System.Collections.Generic;

public class NPCDialogue : MonoBehaviour
{
    string currentDialogueLine;
    public List<DialogueLineBatch> dialogueLineBatches;
    int dialogueIndex = 0;
    void Start()
    {
        TextAsset dialogueFile = Resources.Load<TextAsset>("ASK First Branching Test");
        string[] allLines = dialogueFile.text.Split('\n');
        for (int i = 0; i < allLines.Length; i++)
        {
            if (allLines[i].StartsWith(gameObject.name +","))
            {
                string[] splitLine = allLines[i].Split(',');
                foreach (string line in splitLine)
                {
                    line.Trim();
                }
                Debug.Log(splitLine[2] + ", " + splitLine[3] + ", " +splitLine[4] + ", " + splitLine[5] + ", " + splitLine[6] + ", " + splitLine[7] + ", " + splitLine[8] + ", " + splitLine[9] + ", " + splitLine[10] + ", " + splitLine[11] + ", " + splitLine[12]);
                DialogueLines currentDialogueLine;
                if (bool.Parse(splitLine[4]) == false)
                {
                     currentDialogueLine = new DialogueLines(int.Parse(splitLine[2]), splitLine[3], bool.Parse(splitLine[4]), splitLine[5], splitLine[7], splitLine[9], splitLine[11], int.Parse(splitLine[6]), int.Parse(splitLine[8]), int.Parse(splitLine[10]), int.Parse(splitLine[12]));

                }
                else
                {
                    currentDialogueLine = new DialogueLines(int.Parse(splitLine[2]), splitLine[3], bool.Parse(splitLine[4]),null, null, null, null, int.Parse(splitLine[6]));
                }

                //Add lines to batch
                Debug.Log("Batch ID: " + splitLine[1]);
                if (dialogueLineBatches.Count <= int.Parse(splitLine[1]))
                {
                    dialogueLineBatches.Add(new DialogueLineBatch(splitLine[1], new List<DialogueLines>()));
                }
                dialogueLineBatches[int.Parse(splitLine[1])].dialogueLines.Add(currentDialogueLine);

            }
        }
        Debug.Log(dialogueLineBatches);
    }
    public DialogueLines AdvanceDialogue(int id)
    {
        if (dialogueLineBatches[dialogueIndex].dialogueLines[id] != null)
        {
            return dialogueLineBatches[dialogueIndex].dialogueLines[id];
        }
        else
        {           
            dialogueIndex = -1; // Reset for next interaction  
            return null;
        }
    }
}
