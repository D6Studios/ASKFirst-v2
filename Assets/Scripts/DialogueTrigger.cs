using UnityEngine;
using TMPro;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue UI")]
    public GameObject dialogueUI;
    public TextMeshProUGUI npcNameText;

    [Header("Player Reference")]
    public Transform player;

    public void TriggerDialogue()
    {
        FacePlayer();
        ShowDialogueUI();
    }

    private void FacePlayer()
    {
        if (player == null)
        {
            Debug.LogWarning("Player reference not assigned!");
            return;
        }

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f; // Keep rotation on the Y axis only

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation;
        }
    }

    private void ShowDialogueUI()
    {
        if (dialogueUI != null && npcNameText != null)
        {
            npcNameText.text = CompareTag("Shopper") ? "Shopper" : "Shoplifter";
            dialogueUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Dialogue UI or NPC Name Text not assigned!");
        }
    }
}
