using UnityEngine;
using System.Collections.Generic;

public class NPCDialogue : MonoBehaviour
{
    string currentDialogueLine;
    public List<DialogueLineBatch> dialogueLineBatches;
    int dialogueIndex = 0;
    void Start()
    {
        TextAsset dialogueFile = Resources.Load<TextAsset>("ASKFirstDialogue2Options"); // Load dialogue from Resources folder
        string[] allLines = dialogueFile.text.Split('\n');
        for (int i = 0; i < allLines.Length; i++)
        {
            if (allLines[i].StartsWith(gameObject.name + ",")) //Filter lines for this NPC based on name
            {
                string[] splitLine = allLines[i].Split(','); //Split line into components
                foreach (string line in splitLine)
                {
                    line.Trim(); //Trim whitespace from each component
                    if (line.Contains("|||"))
                    {
                        line.Replace("|||", ",");
                    }
                }
                DialogueLines currentDialogueLine;
                if (bool.Parse(splitLine[4]) == false)
                {
                    currentDialogueLine = new DialogueLines(int.Parse(splitLine[2]), //ID
                    splitLine[3], //Line
                    bool.Parse(splitLine[4]), //Options Bypass
                    splitLine[5], //Option 1
                    int.Parse(splitLine[6]), //Option 1 Next ID
                    int.Parse(splitLine[7]), //Option 1 Mood Change
                    int.Parse(splitLine[8]), //Option 1 Mistake ID
                    splitLine[9], //Option 2
                    int.Parse(splitLine[10]), //Option 2 Next ID
                    int.Parse(splitLine[11]), //Option 2 Mood Change
                    int.Parse(splitLine[12]), //Option 2 Mistake ID
                    splitLine[13], //Option 3
                    int.Parse(splitLine[14]), //Option 3 Next ID
                    int.Parse(splitLine[15]), //Option 3 Mood Change
                    int.Parse(splitLine[16]),//Option 3 Mistake ID
                    splitLine[17], //Option 4
                    int.Parse(splitLine[18]), //Option 4 Next ID   
                    int.Parse(splitLine[19]), //Option 4 Mood Change
                    int.Parse(splitLine[20]) //Option 4 Mistake ID
                    );
                }
                else
                {
                    currentDialogueLine = new DialogueLines(int.Parse(splitLine[2]), //ID
                    splitLine[3], //Line
                    bool.Parse(splitLine[4]), //Options Bypass
                    splitLine[5], //Option 1
                    int.Parse(splitLine[6])//Option 1 Next ID


                    );
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
