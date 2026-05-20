using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class DialogueLineBatch
{
    public string batchId;
    public List<DialogueLines> dialogueLines;
    public DialogueLineBatch(string batchId, List<DialogueLines> dialogueLines)
    {
        this.batchId = batchId;
        this.dialogueLines = new List<DialogueLines>(dialogueLines);
    }
}
