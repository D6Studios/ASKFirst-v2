using UnityEngine;
[System.Serializable]
public class DialogueLines
{
    public int id;
    public string line;
    public bool optionsBypass;
    public string option1;
    public string option2;
    public string option3;
    public string option4;
    public int option1NextId;
    public int option2NextId;
    public int option3NextId;
    public int option4NextId;
    public DialogueLines(int id, string line, bool optionsBypass = false, string option1 = "", string option2 = "", string option3 = "", string option4 = "", int option1NextId = -1, int option2NextId = -1, int option3NextId = -1, int option4NextId = -1)
    {
        this.id = id;
        this.line = line;
        this.optionsBypass = optionsBypass;
        this.option1 = option1;
        this.option2 = option2;
        this.option3 = option3;
        this.option4 = option4;
        this.option1NextId = option1NextId;
        this.option2NextId = option2NextId;
        this.option3NextId = option3NextId;
        this.option4NextId = option4NextId;
    }
}
