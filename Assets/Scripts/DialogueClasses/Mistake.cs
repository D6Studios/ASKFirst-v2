using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Mistake
{
    public string catagory;
    public string description;
    public bool positive;
    public int id;

    public Mistake(string catagory, string description, bool positive, int id)
    {
        this.catagory = catagory;
        this.description = description;
        this.positive = positive;
        this.id = id;
    }
}
