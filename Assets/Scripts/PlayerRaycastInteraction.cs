using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueEditor; // Required for ConversationManager and Conversation

public class PlayerRaycastInteraction : MonoBehaviour
{
    [Header("Player Reference")]
    public static Camera playerCamera; // Static reference set by player
    public static KeyCode interactionKey = KeyCode.E;
    public static float interactionRange = 5f;

    [Header("UI Elements")]
    public GameObject interactButton;

    [Header("NPC Components")]
    public ScannerNPC scannerNPC; // Reference to the scanner NPC behavior

    [Header("Outline Settings")]
    [Tooltip("Color of the outline when looking at NPC")]
    public Color outlineColor = Color.yellow;
    [Tooltip("Width of the outline")]
    public float outlineWidth = 10f;
    [Tooltip("Outline mode - OutlineAll, OutlineVisible, or OutlineHidden")]
    public Outline.Mode outlineMode = Outline.Mode.OutlineAll;

    [Header("Dialogue Protection")]
    [Tooltip("If true, NPC won't stop interacting when line of sight is broken during dialogue")]
    public bool protectDuringDialogue = true;

    [Tooltip("Maximum time to wait for dialogue to finish before forcing stop (0 = no timeout)")]
    public float dialogueTimeoutDuration = 30f;

    private bool isBeingLookedAt = false;
    private bool isInConversation = false; // Track conversation state
    private Coroutine dialogueTimeoutCoroutine;
    private Outline npcOutline; // Reference to the outline component

    private void Start()
    {
        // Hide interact button initially
        if (interactButton != null)
        {
            interactButton.SetActive(false);
        }

        // Auto-find scanner NPC if not assigned
        if (scannerNPC == null)
        {
            scannerNPC = GetComponent<ScannerNPC>();
        }

        // Auto-find player camera if not set
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindObjectOfType<Camera>();
            }
        }

        // Setup outline component
        SetupOutline();
    }

    private void SetupOutline()
    {
        // Get or add the Outline component
        npcOutline = GetComponent<Outline>();
        if (npcOutline == null)
        {
            npcOutline = gameObject.AddComponent<Outline>();
        }

        // Configure outline settings
        npcOutline.OutlineMode = outlineMode;
        npcOutline.OutlineColor = Color.yellow;
        npcOutline.OutlineWidth = outlineWidth;

        // Start with outline disabled
        npcOutline.enabled = false;
    }

    private void Update()
    {
        // Always make the interact button face the camera
        if (playerCamera != null && interactButton != null)
        {
            interactButton.transform.LookAt(playerCamera.transform);
            interactButton.transform.Rotate(0, 180f, 0); // Optional flip if facing wrong way
        }

        // Only check for look detection if not in conversation
        if (!isInConversation)
        {
            // Check if player is looking at this NPC
            CheckIfBeingLookedAt();

            // Handle interaction input
            if (isBeingLookedAt && Input.GetKeyDown(interactionKey))
            {
                StartConversation();
            }
        }
        else
        {
            // During conversation, check if dialogue system is still active
            // If dialogue system ends, we need to end the conversation
            if (!IsDialogueActive())
            {
                EndConversation();
            }
        }
    }

    private void CheckIfBeingLookedAt()
    {
        if (playerCamera == null) return;

        // Cast ray from center of screen
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            // Check if the ray hit this NPC specifically
            if (hit.collider.gameObject == gameObject)
            {
                if (!isBeingLookedAt)
                {
                    // Player just started looking at this NPC
                    OnLookAt();
                }
            }
            else
            {
                if (isBeingLookedAt)
                {
                    // Player is no longer looking at this NPC
                    OnLookAway();
                }
            }
        }
        else
        {
            if (isBeingLookedAt)
            {
                // No hit within range
                OnLookAway();
            }
        }
    }

    private void OnLookAt()
    {
        isBeingLookedAt = true;

        // Enable outline when looking at NPC
        if (npcOutline != null)
        {
            npcOutline.enabled = true;
        }

        // Only show interact button when looking at NPC and not in conversation
        if (interactButton != null && !isInConversation)
        {
            interactButton.SetActive(true);
        }

        Debug.Log($"Player started looking at {gameObject.name}");
    }

    private void OnLookAway()
    {
        isBeingLookedAt = false;

        // Disable outline when not looking at NPC
        if (npcOutline != null)
        {
            npcOutline.enabled = false;
        }

        // Hide interact button
        if (interactButton != null)
        {
            interactButton.SetActive(false);
        }

        // ENHANCED: Only end interaction if we're not in conversation or if dialogue protection is disabled
        if (!isInConversation)
        {
            // Not in conversation, safe to stop interaction normally
            if (scannerNPC != null && IsNPCInteracting())
            {
                scannerNPC.StopInteract();
            }
        }
        else if (protectDuringDialogue && IsDialogueActive())
        {
            // In conversation and dialogue is active - protect it
            Debug.Log($"Player stopped looking at {gameObject.name} but dialogue is active - keeping interaction");

            // Start timeout if configured
            if (dialogueTimeoutDuration > 0 && dialogueTimeoutCoroutine == null)
            {
                dialogueTimeoutCoroutine = StartCoroutine(DialogueTimeoutCoroutine());
            }
        }
        else
        {
            // Dialogue protection is disabled or dialogue not active
            EndConversation();
        }

        Debug.Log($"Player stopped looking at {gameObject.name}");
    }

    private void StartConversation()
    {
        if (scannerNPC != null)
        {
            isInConversation = true; // Set conversation flag FIRST

            // Start interaction when E is pressed
            scannerNPC.Interact();
            scannerNPC.Looking();

            // Hide the interact button during conversation
            if (interactButton != null)
            {
                interactButton.SetActive(false);
            }

            // Then start the dialogue
            scannerNPC.StartDialogue();
            Debug.Log($"Started conversation with {gameObject.name}");

            // Start dialogue timeout if configured
            if (dialogueTimeoutDuration > 0)
            {
                if (dialogueTimeoutCoroutine != null)
                {
                    StopCoroutine(dialogueTimeoutCoroutine);
                }
                dialogueTimeoutCoroutine = StartCoroutine(DialogueTimeoutCoroutine());
            }
        }
        else
        {
            Debug.LogWarning($"No ScannerNPC found on {gameObject.name}");
        }
    }

    private void EndConversation()
    {
        Debug.Log($"Ending conversation with {gameObject.name}");

        isInConversation = false; // Clear conversation flag
        StopDialogueTimeout();

        if (scannerNPC != null && IsNPCInteracting())
        {
            scannerNPC.StopInteract();
        }

        // Resume normal look detection
        // The interact button will be shown again if player is still looking at NPC
        if (isBeingLookedAt && interactButton != null)
        {
            interactButton.SetActive(true);
        }
    }

    private bool IsNPCInteracting()
    {
        return scannerNPC != null && scannerNPC.Interacting;
    }

    private bool IsDialogueActive()
    {
        // Check if ConversationManager exists and has an active conversation
        if (ConversationManager.Instance != null)
        {
            return ConversationManager.Instance.IsConversationActive;
        }
        return false;
    }

    private IEnumerator DialogueTimeoutCoroutine()
    {
        yield return new WaitForSeconds(dialogueTimeoutDuration);

        // Force end conversation after timeout
        if (isInConversation)
        {
            Debug.LogWarning($"Dialogue timeout reached for {gameObject.name} - forcing end conversation");
            EndConversation();
        }

        dialogueTimeoutCoroutine = null;
    }

    private void StopDialogueTimeout()
    {
        if (dialogueTimeoutCoroutine != null)
        {
            StopCoroutine(dialogueTimeoutCoroutine);
            dialogueTimeoutCoroutine = null;
        }
    }

    public void StartConvo()
    {
        // Legacy method for backward compatibility
        StartConversation();
    }

    // Public method that can be called by dialogue system when conversation ends
    public void OnConversationEnd()
    {
        EndConversation();
    }

    // Method to manually end interaction (can be called from dialogue events)
    public void EndInteraction()
    {
        EndConversation();
    }

    // Helper method to set global settings from player script
    public static void SetPlayerReference(Camera camera, float range = 5f, KeyCode key = KeyCode.E)
    {
        playerCamera = camera;
        interactionRange = range;
        interactionKey = key;
    }

    // Method to change outline settings at runtime
    public void SetOutlineSettings(Color color, float width, Outline.Mode mode)
    {
        outlineColor = color;
        outlineWidth = width;
        outlineMode = mode;

        if (npcOutline != null)
        {
            npcOutline.OutlineColor = color;
            npcOutline.OutlineWidth = width;
            npcOutline.OutlineMode = mode;
        }
    }

    private void OnDestroy()
    {
        StopDialogueTimeout();
    }

    // Optional: Draw debug ray in scene view to visualize interaction range
    private void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 rayOrigin = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;
            Gizmos.DrawRay(rayOrigin, rayDirection * interactionRange);

            // Draw sphere around this NPC to show interaction detection
            Gizmos.color = isBeingLookedAt ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}