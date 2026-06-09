using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Mistake
{
    public bool positive;
    public string title;
    public string hint;
    public int id;
    public Mistake(int id, string title, string hint, bool positive)
    {
        this.id = id;
        this.positive = positive;
        this.title = title;
        this.hint = hint;
    }
}
