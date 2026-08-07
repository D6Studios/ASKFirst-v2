using System.Collections.Generic;
using UnityEngine;

public class FormSGRedirect : MonoBehaviour
{
    string currentURL;
    string urlCode = "";
    Dictionary<string, string> regionDictionary = new Dictionary<string, string>();
    TextAsset formSGLinksJSON;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentURL = Application.absoluteURL;
        urlCode = currentURL.Split('?')[1];
        formSGLinksJSON = Resources.Load<TextAsset>("formsglinks");
        RegionList regionList = JsonUtility.FromJson<RegionList>(formSGLinksJSON.text);


        foreach (Region region in regionList.Regions)
        {
            regionDictionary[region.name] = region.url;
        }

    }

    public void RedirectToFormSG()
    {
        if (regionDictionary.ContainsKey(urlCode))
        {
            Application.OpenURL(regionDictionary[urlCode]);
        }
        else
        {
            Application.OpenURL(regionDictionary[1.ToString()]); // Default to the first region if the code is not found
            Debug.LogError("Region code not found in the dictionary: " + urlCode);
        }
    }
}
[System.Serializable]
public class Region
{
    public string name;
    public string url;
}
[System.Serializable]
public class RegionList
{
    public Region[] Regions;
}
