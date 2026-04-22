using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueEditor; // Required for ConversationManager and Conversation

public class ButtonBehavior : MonoBehaviour
{
    public GameObject playerCamera;
    public GameObject interactButton;
    public NPCConversation myConversation;

    private bool isPlayerInTrigger = false;

    private void Update()
    {
        if (playerCamera != null)
        {
            transform.LookAt(playerCamera.transform);
            transform.Rotate(0, 180f, 0); // Optional flip if facing wrong way
        }

        if (isPlayerInTrigger && Input.GetKeyDown(KeyCode.E))
        {
            StartConvo();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;

            if (interactButton != null)
            {
                interactButton.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;

            if (interactButton != null)
            {
                interactButton.SetActive(false);
            }
        }
    }

    public void StartConvo()
    {
        if (myConversation != null)
        {
            ConversationManager.Instance.StartConversation(myConversation);
        }
        else
        {
            Debug.LogWarning("No Conversation assigned in ButtonBehavior.");
        }
    }

    
}