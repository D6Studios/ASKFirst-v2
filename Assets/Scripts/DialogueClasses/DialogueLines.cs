using UnityEngine;
[System.Serializable]
public class DialogueLines
{
    public int id;
    public string line;
    public bool optionsBypass;
    public string option1;
    public string option2;

    public int option1NextId;
    public int option2NextId;
    public int option1MoodChange;
    public int option2MoodChange;
    public int option1MistakeId;
    public int option2MistakeId;
    public DialogueLines(int id, string line, bool optionsBypass = false, string option1 = "", int option1NextId = -1, int option1MoodChange = 0, int option1MistakeId = -1, string option2 = "", int option2NextId = -1, int option2MoodChange = 0, int option2MistakeId = -1)
    {
        this.id = id;
        this.line = line;
        this.optionsBypass = optionsBypass;
        this.option1 = option1;
        this.option2 = option2;

        this.option1NextId = option1NextId;
        this.option2NextId = option2NextId;

        this.option1MoodChange = option1MoodChange;
        this.option2MoodChange = option2MoodChange;

        this.option1MistakeId = option1MistakeId;
        this.option2MistakeId = option2MistakeId;

    }
}
