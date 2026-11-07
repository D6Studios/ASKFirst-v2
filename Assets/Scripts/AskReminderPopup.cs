using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AskReminderPopup : MonoBehaviour
{
    public static AskReminderPopup Instance { get; private set; }

    [SerializeField] Canvas reminderCanvas;
    [SerializeField] TextMeshProUGUI bodyText;
    [SerializeField] Button closeButton;

    [TextArea(4, 10)]
    [SerializeField]
    string reminderMessage =
        "S - Safety First\nMaintain a safe distance when interacting with suspicious individuals. Do not attempt any physical contact or confrontation.\n\n" +
        "K - Keep Close\nMonitor discreetly by performing routine duties.";

    void Awake()
    {
        Instance = this;
        reminderCanvas.enabled = false;
        closeButton.onClick.AddListener(Close);
    }

    public void Show()
    {
        bodyText.text = reminderMessage;
        reminderCanvas.enabled = true;
        Time.timeScale = 0f;
    }

    void Close()
    {
        reminderCanvas.enabled = false;
        Time.timeScale = 1f;
    }
}
