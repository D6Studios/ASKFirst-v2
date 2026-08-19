using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class RemoteLoader : MonoBehaviour
{
    private string fileUrl = "https://b8888fb0-9a9a-4559-9af8-3749a8683cd3.client-api.unity3dusercontent.com/client_api/v1/environments/production/buckets/7ce1c4ee-c517-498e-873a-6b0f88b9eb03/release_by_badge/latest/entry_by_path/content/?path=ASKFirstDialogueFinal.txt";
    public string fileContent;
    public string mistakesUrl = "https://b8888fb0-9a9a-4559-9af8-3749a8683cd3.client-api.unity3dusercontent.com/client_api/v1/environments/production/buckets/7ce1c4ee-c517-498e-873a-6b0f88b9eb03/release_by_badge/latest/entry_by_path/content/?path=ASKFirstMistakes.txt";
    public string mistakesContent;
    public static RemoteLoader Instance { get; private set; }
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        StartCoroutine(DownloadNPCLines(fileUrl));
        StartCoroutine(DownloadMistakes(mistakesUrl));

    }

    IEnumerator DownloadNPCLines(string url)
    {
        Debug.Log("Waiting for response...");

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Failed to download: " + www.error);
        }
        else
        {
            Debug.Log("Response received!");

            fileContent = www.downloadHandler.text;
            Debug.Log(fileContent);
        }
    }
    IEnumerator DownloadMistakes(string url)
    {
        Debug.Log("Waiting for response...");

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Failed to download: " + www.error);
        }
        else
        {
            Debug.Log("Response received!");

            mistakesContent = www.downloadHandler.text;
            Debug.Log(mistakesContent);
        }
    }
}