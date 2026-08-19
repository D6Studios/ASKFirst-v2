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
        string formSGLinksContent = RemoteLoader.Instance.formSGLinksContent;
        string[] pairs = formSGLinksContent.Split("\n");



        foreach (string pair in pairs)
        {
            string[] keyValue = pair.Split('|');

            regionDictionary[keyValue[0]] = keyValue[1];
            Debug.Log("Key: " + keyValue[0] + ", Value: " + keyValue[1]);
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
            Application.OpenURL(regionDictionary["alpha"]); // Default to the first region if the code is not found
            Debug.LogError("Region code not found in the dictionary: " + urlCode);
        }
    }
}

