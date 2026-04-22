using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueEditor; // Required for ConversationManager and Conversation

public class RaycastNPCInteraction : MonoBehaviour
{
    [Header("Player Reference")]
    public static Camera playerCamera; // Static reference set by player
    public static Transform playerRaycastPoint; // Changed from PlayerRaycastPoint to Transform
    public static KeyCode interactionKey = KeyCode.E;
    public static float interactionRange = 5f;

    [Header("UI Elements")]
    public GameObject interactButton;

    [Header("NPC Components")]
    public ShopperBehavior shopperBehavior; // Reference to the shopper behavior

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
    private Coroutine dialogueTimeoutCoroutine;
    private Outline npcOutline; // Reference to the outline component

    // Enhanced dialogue tracking using callbacks
    private bool isMyDialogueActive = false;

    private void Start()
    {
        // Hide interact button initially
        if (interactButton != null)
        {
            interactButton.SetActive(false);
        }

        // Auto-find shopper behavior if not assigned
        if (shopperBehavior == null)
        {
            shopperBehavior = GetComponent<ShopperBehavior>();
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

        // Subscribe to dialogue callbacks for more precise tracking
        SubscribeToDialogueCallbacks();
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

    private void OnDestroy()
    {
        StopDialogueTimeout();
        UnsubscribeFromDialogueCallbacks();
    }

    private void SubscribeToDialogueCallbacks()
    {
        if (ConversationManager.Instance != null)
        {
            ConversationManager.OnConversationStarted += OnDialogueStarted;
            ConversationManager.OnConversationEnded += OnDialogueEnded;
        }
    }

    private void UnsubscribeFromDialogueCallbacks()
    {
        if (ConversationManager.Instance != null)
        {
            ConversationManager.OnConversationStarted -= OnDialogueStarted;
            ConversationManager.OnConversationEnded -= OnDialogueEnded;
        }
    }

    private void OnDialogueStarted()
    {
        // Since we can't reliably identify which NPC's dialogue started,
        // we'll assume it's ours if we're currently interacting
        if (IsNPCInteracting())
        {
            isMyDialogueActive = true;
            Debug.Log($"{gameObject.name}: Dialogue started while interacting - protected from interruption");
        }
    }

    private void OnDialogueEnded()
    {
        if (isMyDialogueActive)
        {
            isMyDialogueActive = false;
            Debug.Log($"{gameObject.name}: My dialogue ended");

            // If we're still interacting but dialogue ended, stop interaction
            if (IsNPCInteracting())
            {
                shopperBehavior.StopInteract();
                StopDialogueTimeout();
            }
        }
    }

    private void Update()
    {
        // Always make the interact button face the camera
        if (playerCamera != null && interactButton != null)
        {
            interactButton.transform.LookAt(playerCamera.transform);
            interactButton.transform.Rotate(0, 180f, 0); // Optional flip if facing wrong way
        }

        // Check if player is looking at this NPC
        CheckIfBeingLookedAt();

        // Handle interaction input
        if (isBeingLookedAt && Input.GetKeyDown(interactionKey))
        {
            StartConversation();
        }

        // Check if dialogue has ended and we need to clean up
        if (IsNPCInteracting() && !IsDialogueActive())
        {
            // Dialogue has ended, safe to stop interaction
            if (shopperBehavior != null)
            {
                shopperBehavior.StopInteract();
                StopDialogueTimeout();
            }
        }
    }

    private void CheckIfBeingLookedAt()
    {
        // Use raycast point if available, otherwise fall back to camera center
        Ray ray;

        if (playerRaycastPoint != null)
        {
            // Use the raycast point position and direction
            ray = new Ray(playerRaycastPoint.position, playerRaycastPoint.forward);
        }
        else if (playerCamera != null)
        {
            // Fallback to center of screen raycast
            ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        }
        else
        {
            return; // No camera or raycast point available
        }

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

        // Only show interact button when looking at NPC
        if (interactButton != null)
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

        // ENHANCED: Only stop interaction if we're not protecting during dialogue
        // or if dialogue is not currently active
        if (shopperBehavior != null && IsNPCInteracting())
        {
            bool shouldProtectDialogue = protectDuringDialogue && (IsDialogueActive() || isMyDialogueActive);

            if (!shouldProtectDialogue)
            {
                // Safe to stop interaction
                shopperBehavior.StopInteract();
                StopDialogueTimeout();
                Debug.Log($"Stopped interaction with {gameObject.name} - dialogue not active or protection disabled");
            }
            else
            {
                // Dialogue is active and we're protecting it
                Debug.Log($"Player stopped looking at {gameObject.name} but dialogue is active - keeping interaction");

                // Start timeout if configured
                if (dialogueTimeoutDuration > 0 && dialogueTimeoutCoroutine == null)
                {
                    dialogueTimeoutCoroutine = StartCoroutine(DialogueTimeoutCoroutine());
                }
            }
        }

        Debug.Log($"Player stopped looking at {gameObject.name}");
    }

    private void StartConversation()
    {
        if (shopperBehavior != null)
        {
            // Start interaction when E is pressed
            shopperBehavior.Interact();

            // Then start the dialogue
            shopperBehavior.StartDialogue();
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
            Debug.LogWarning($"No ShopperBehavior found on {gameObject.name}");
        }
    }

    private bool IsNPCInteracting()
    {
        // Check if the NPC is actually in the interacting state
        return shopperBehavior != null && shopperBehavior.IsInteracting;
    }

    private bool IsDialogueActive()
    {
        // Check if ConversationManager exists and has an active conversation
        if (ConversationManager.Instance != null)
        {
            // Using the correct DialogueEditor API from documentation
            return ConversationManager.Instance.IsConversationActive;
        }
        return false;
    }

    private IEnumerator DialogueTimeoutCoroutine()
    {
        yield return new WaitForSeconds(dialogueTimeoutDuration);

        // Force stop interaction after timeout
        if (IsNPCInteracting())
        {
            Debug.LogWarning($"Dialogue timeout reached for {gameObject.name} - forcing stop interaction");
            if (shopperBehavior != null)
            {
                shopperBehavior.StopInteract();
            }
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

    // Method to manually end interaction (can be called from dialogue events)
    public void EndInteraction()
    {
        if (IsNPCInteracting())
        {
            isMyDialogueActive = false; // Reset our dialogue tracking
            shopperBehavior.StopInteract();
            StopDialogueTimeout();
            Debug.Log($"Manually ended interaction with {gameObject.name}");
        }
    }

    // Helper method to set global settings from player script
    public static void SetPlayerReference(Camera camera, Transform raycastPoint, float range, KeyCode key)
    {
        // Store all the references
        playerCamera = camera;
        playerRaycastPoint = raycastPoint;  // Now correctly typed as Transform
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

    // Optional: Draw debug ray in scene view to visualize interaction range
    private void OnDrawGizmosSelected()
    {
        // Use raycast point if available, otherwise use camera
        Vector3 rayOrigin;
        Vector3 rayDirection;

        if (playerRaycastPoint != null)
        {
            rayOrigin = playerRaycastPoint.position;
            rayDirection = playerRaycastPoint.forward;
        }
        else if (playerCamera != null)
        {
            rayOrigin = playerCamera.transform.position;
            rayDirection = playerCamera.transform.forward;
        }
        else
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(rayOrigin, rayDirection * interactionRange);

        // Draw sphere around this NPC to show interaction detection
        Gizmos.color = isBeingLookedAt ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}