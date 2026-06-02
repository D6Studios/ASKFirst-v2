using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Mistake
{
    public bool wrong;
    public string title;
    public string hint;
    public Mistake(bool wrong, string title, string hint)
    {
        this.wrong = wrong;
        this.title = title;
        this.hint = hint;
    }
}
