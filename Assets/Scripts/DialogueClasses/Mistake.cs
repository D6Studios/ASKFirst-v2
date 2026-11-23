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

    // Tab-separated sheet: category, description, positive, id. First row is the header.
    public static List<Mistake> ParseAll(string sheet)
    {
        var list = new List<Mistake>();
        string[] lines = sheet.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split('\t');
            list.Add(new Mistake(cols[0], cols[1], bool.Parse(cols[2]), int.Parse(cols[3])));
        }
        return list;
    }
}
