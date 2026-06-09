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
    public int option1MoodChange;
    public int option2MoodChange;
    public int option3MoodChange;
    public int option4MoodChange;
    public int option1MistakeId;
    public int option2MistakeId;
    public int option3MistakeId;
    public int option4MistakeId;
    public DialogueLines(int id, string line, bool optionsBypass = false, string option1 = "",  int option1NextId = -1, int option1MoodChange = 0, int option1MistakeId = -1, string option2 = "", int option2NextId = -1, int option2MoodChange = 0, int option2MistakeId = -1, string option3 = "",  int option3NextId = -1, int option3MoodChange = 0, int option3MistakeId = -1,  string option4 = "",  int option4NextId = -1,int option4MoodChange = 0, int option4MistakeId = -1)
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
        this.option1MoodChange = option1MoodChange;
        this.option2MoodChange = option2MoodChange;
        this.option3MoodChange = option3MoodChange;
        this.option4MoodChange = option4MoodChange;
        this.option1MistakeId = option1MistakeId;
        this.option2MistakeId = option2MistakeId;
        this.option3MistakeId = option3MistakeId;
        this.option4MistakeId = option4MistakeId;
    }
}
