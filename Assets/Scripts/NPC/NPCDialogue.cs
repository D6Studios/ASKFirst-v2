using UnityEngine;
using System.Collections.Generic;
using System;

public class NPCDialogue : MonoBehaviour
{
    string currentDialogueLine;
    public List<DialogueLineBatch> dialogueLineBatches;
    int dialogueIndex = 0;
    void Start()
    {
        TextAsset dialogueFile = Resources.Load<TextAsset>("ASKFirstDialogueTab"); // Load dialogue from Resources folder
        string[] allLines = dialogueFile.text.Split('\n');
        for (int i = 0; i < allLines.Length; i++)
        {
            if (allLines[i].StartsWith(gameObject.name + "\t")) //Filter lines for this NPC based on name
            {
                string[] splitLine = allLines[i].Split('\t'); //Split line into components
                Debug.Log("Split Line: " + string.Join(", ", splitLine));
                DialogueLines currentDialogueLine;
                if (bool.Parse(splitLine[4]) == false)
                {
                    try
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
                        int.Parse(splitLine[12]) //Option 2 Mistake ID
                        );
                    }
                    catch (FormatException e)
                    {
                        //find which field is causing the error
                        Debug.LogError("FormatException: " + e.Message + " in line: " + allLines[i]);

                        continue; //Skip this line and continue with the next one
                    }

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
