using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using DialogueEditor;


[RequireComponent(typeof(NavMeshAgent))]
public class ShopperBehavior : MonoBehaviour
{
    [Header("Browse Settings")]
    public float browseTimeMin = 3f;
    public float browseTimeMax = 8f;

    [Header("Buy Zones")]
    public List<BuyZone> buyZones = new List<BuyZone>();

    [Header("Player Reference (for interactions)")]
    public Transform player;

    [Header("Face System")]
    public GameObject defaultFace;
    public GameObject confrontFace;
    public GameObject chillFace;

    [Header("Dialogue")]
    public NPCConversation conversation;

    [Header("Camera Control")]
    public MonoBehaviour playerCameraController;
    public Transform lookPoint;
    public bool freezeCameraOnInteract = true;
    public bool unlockCursorOnInteract = true;
    public float cameraTransitionSpeed = 2f;

    private NavMeshAgent agent;
    private Animator animator;
    private float maxAgentSpeed;

    private BuyZone currentZone;
    private BuyZone lastZone;

    private ShopperState savedState;
    private Quaternion cachedRotation;

    // Face system variables
    private GameObject currentActiveFace;

    // Camera control variables
    private bool wasCameraEnabled;
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;
    private Camera playerCamera;
    private Coroutine cameraLookCoroutine;

    // INTERACTION PROTECTION VARIABLES
    private bool isInteractionLocked = false; // Prevents state changes during dialogue
    private Coroutine currentBehaviorCoroutine; // Track the main behavior coroutine

    private enum ShopperState
    {
        GoingToZone,
        Browsing,
        Interacting
    }

    private ShopperState currentState;

    public bool IsInteracting => currentState == ShopperState.Interacting;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        maxAgentSpeed = agent.speed;

        if (playerCameraController == null)
        {
            playerCameraController = FindObjectOfType<FirstPersonController>() as MonoBehaviour;
        }

        InitializeFaceSystem();
        StartShoppingCycle();
    }

    void Update()
    {
        // Animation bools are set directly in state changes
    }

    #region Face System

    private void InitializeFaceSystem()
    {
        if (defaultFace != null) defaultFace.SetActive(false);
        if (confrontFace != null) confrontFace.SetActive(false);
        if (chillFace != null) chillFace.SetActive(false);

        Default();
    }

    public void Default()
    {
        SetActiveFace(defaultFace);
    }

    public void Confront()
    {
        SetActiveFace(confrontFace);
    }

    public void Chill()
    {
        SetActiveFace(chillFace);
    }

    private void SetActiveFace(GameObject newFace)
    {
        if (currentActiveFace != null)
        {
            currentActiveFace.SetActive(false);
        }

        if (newFace != null)
        {
            newFace.SetActive(true);
            currentActiveFace = newFace;
        }
        else
        {
            Debug.LogWarning($"{name}: Tried to set a null face!");
            currentActiveFace = null;
        }
    }

    #endregion

    #region Camera Control

    private void FreezeCameraAndUnlockCursor()
    {
        if (freezeCameraOnInteract && playerCameraController != null)
        {
            wasCameraEnabled = playerCameraController.enabled;
            playerCameraController.enabled = false;
            Debug.Log($"{name}: Disabled camera controller");
        }

        if (unlockCursorOnInteract)
        {
            previousCursorLockMode = Cursor.lockState;
            previousCursorVisible = Cursor.visible;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log($"{name}: Unlocked cursor");
        }
    }

    private void RestoreCameraAndCursor()
    {
        if (freezeCameraOnInteract && playerCameraController != null)
        {
            playerCameraController.enabled = wasCameraEnabled;
            Debug.Log($"{name}: Restored camera controller");
        }

        if (unlockCursorOnInteract)
        {
            Cursor.lockState = previousCursorLockMode;
            Cursor.visible = previousCursorVisible;
            Debug.Log($"{name}: Restored cursor state");
        }
    }

    #endregion

    void StartShoppingCycle()
    {
        // ENHANCED INTERACTION PROTECTION
        if (currentState == ShopperState.Interacting || isInteractionLocked)
        {
            Debug.Log($"{name}: Cannot start shopping cycle - currently interacting or interaction locked");
            return;
        }

        // Stop any existing behavior coroutine
        if (currentBehaviorCoroutine != null)
        {
            StopCoroutine(currentBehaviorCoroutine);
        }

        if (ZoneManager.Instance == null)
        {
            Debug.LogError($"{name}: ZoneManager not found! Make sure ZoneManager is in the scene.");
            return;
        }

        BuyZone targetZone = ReserveRandomBuyZone();

        if (targetZone == null)
        {
            Debug.LogWarning($"{name}: No available buy zones. Retrying in 2 seconds...");
            currentBehaviorCoroutine = StartCoroutine(RetryAfterDelay(2f));
            return;
        }

        currentZone = targetZone;
        currentState = ShopperState.GoingToZone;

        if (animator != null)
        {
            animator.SetBool("Walking", true);
            animator.SetBool("Thinking", false);
        }

        Debug.Log($"{name}: Reserved zone '{currentZone.name}', heading there now.");
        agent.SetDestination(currentZone.transform.position);

        currentBehaviorCoroutine = StartCoroutine(WalkToBuyZone());
    }

    IEnumerator WalkToBuyZone()
    {
        while (!ReachedDestination() && currentState != ShopperState.Interacting && !isInteractionLocked)
        {
            // ENHANCED SAFETY CHECK WITH INTERACTION PROTECTION
            if (currentZone != null && !ZoneManager.Instance.IsZoneReservedBy(currentZone, gameObject))
            {
                // Only interrupt if we're not interacting
                if (currentState != ShopperState.Interacting && !isInteractionLocked)
                {
                    Debug.LogWarning($"{name}: Lost reservation for zone '{currentZone.name}' while traveling. Finding new zone.");
                    currentZone = null;
                    StartShoppingCycle();
                    yield break;
                }
                else
                {
                    // We're interacting, so just wait and check again later
                    Debug.Log($"{name}: Lost zone reservation but currently interacting - waiting...");
                    yield return new WaitForSeconds(1f);
                    continue;
                }
            }

            yield return null;
        }

        // Don't proceed if we're now interacting
        if (currentState == ShopperState.Interacting || isInteractionLocked) yield break;

        if (currentZone != null)
        {
            currentZone.AssignShopper(gameObject);
            Debug.Log($"{name}: Arrived at zone '{currentZone.name}', starting to browse.");
            currentBehaviorCoroutine = StartCoroutine(BrowseAtZone());
        }
    }

    IEnumerator BrowseAtZone()
    {
        currentState = ShopperState.Browsing;
        agent.ResetPath();

        if (animator != null)
        {
            animator.SetBool("Walking", false);
            animator.SetBool("Thinking", true);
        }

        float browseTime = Random.Range(browseTimeMin, browseTimeMax);
        Debug.Log($"{name}: Browsing for {browseTime:F1} seconds...");

        float elapsed = 0f;
        while (elapsed < browseTime && currentState != ShopperState.Interacting && !isInteractionLocked)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Don't proceed if we're now interacting
        if (currentState == ShopperState.Interacting || isInteractionLocked) yield break;

        Debug.Log($"{name}: Finished browsing at '{currentZone.name}'.");

        if (animator != null)
        {
            animator.SetBool("Thinking", false);
        }

        lastZone = currentZone;
        currentZone.ClearAssignment();
        ZoneManager.Instance.ReleaseZone(currentZone, gameObject);
        currentZone = null;

        StartShoppingCycle();
    }

    IEnumerator RetryAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartShoppingCycle();
    }

    private BuyZone ReserveRandomBuyZone()
    {
        List<BuyZone> availableZones = ZoneManager.Instance.GetAvailableZones(buyZones, lastZone);

        if (availableZones.Count == 0)
        {
            availableZones = ZoneManager.Instance.GetAvailableZones(buyZones);
        }

        if (availableZones.Count == 0)
        {
            Debug.LogWarning($"{name}: No zones available at all!");
            return null;
        }

        while (availableZones.Count > 0)
        {
            int randomIndex = Random.Range(0, availableZones.Count);
            BuyZone chosenZone = availableZones[randomIndex];

            if (ZoneManager.Instance.TryReserveZone(chosenZone, gameObject))
            {
                return chosenZone;
            }
            else
            {
                availableZones.RemoveAt(randomIndex);
                Debug.Log($"{name}: Zone '{chosenZone.name}' became unavailable, trying another...");
            }
        }

        return null;
    }

    private bool ReachedDestination()
    {
        return !agent.pathPending &&
               agent.remainingDistance <= agent.stoppingDistance &&
               (!agent.hasPath || agent.velocity.sqrMagnitude < 0.1f);
    }

    // ENHANCED INTERACTION METHODS WITH LOCKING
    public void Interact()
    {
        if (currentState == ShopperState.Interacting || isInteractionLocked)
        {
            Debug.Log($"{name}: Already interacting or interaction locked");
            return;
        }

        // LOCK THE INTERACTION - this prevents any state changes
        isInteractionLocked = true;

        savedState = currentState;
        cachedRotation = transform.rotation;
        currentState = ShopperState.Interacting;

        agent.isStopped = true;

        if (animator != null)
        {
            animator.SetBool("Walking", false);
            animator.SetBool("Thinking", false);
        }

        FreezeCameraAndUnlockCursor();

        // Stop any behavior coroutines
        if (currentBehaviorCoroutine != null)
        {
            StopCoroutine(currentBehaviorCoroutine);
            currentBehaviorCoroutine = null;
        }

        StartCoroutine(FacePlayer());

        Debug.Log($"{name}: Started interacting with player - INTERACTION LOCKED");
    }

    public void StartDialogue()
    {
        if (conversation != null && ConversationManager.Instance != null)
        {
            Confront();
            ConversationManager.Instance.StartConversation(conversation);
            Debug.Log($"{name}: Started dialogue conversation.");
            StartCoroutine(SwitchToChillAfterDelay(0.5f));
        }
        else
        {
            Debug.LogWarning($"{name}: Cannot start dialogue - conversation or ConversationManager is null.");
        }
    }

    private IEnumerator SwitchToChillAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (currentState == ShopperState.Interacting)
        {
            Chill();
        }
    }

    public void StopInteract()
    {
        if (currentState != ShopperState.Interacting)
        {
            Debug.Log($"{name}: StopInteract called but not currently interacting");
            return;
        }

        Debug.Log($"{name}: StopInteract called - UNLOCKING INTERACTION");

        Default();
        RestoreCameraAndCursor();

        currentState = savedState;
        agent.isStopped = false;
        transform.rotation = cachedRotation;

        // UNLOCK THE INTERACTION - now state changes are allowed again
        isInteractionLocked = false;

        // Resume previous behavior based on saved state
        if (savedState == ShopperState.GoingToZone && currentZone != null)
        {
            if (ZoneManager.Instance.IsZoneReservedBy(currentZone, gameObject))
            {
                if (animator != null)
                {
                    animator.SetBool("Walking", true);
                    animator.SetBool("Thinking", false);
                }
                agent.SetDestination(currentZone.transform.position);
                currentBehaviorCoroutine = StartCoroutine(WalkToBuyZone());
            }
            else
            {
                currentZone = null;
                StartShoppingCycle();
            }
        }
        else if (savedState == ShopperState.Browsing && currentZone != null && currentZone.IsOccupiedBy(gameObject))
        {
            // Instead of resuming browsing (which might immediately finish),
            // finish the current browsing session and move to a new zone
            Debug.Log($"{name}: Was browsing when interrupted - finishing browsing and moving to new zone");

            if (animator != null)
            {
                animator.SetBool("Walking", false);
                animator.SetBool("Thinking", false);
            }

            // Clean up current zone
            lastZone = currentZone;
            currentZone.ClearAssignment();
            ZoneManager.Instance.ReleaseZone(currentZone, gameObject);
            currentZone = null;

            // Start a new shopping cycle
            StartShoppingCycle();
        }
        else
        {
            // Default case - start fresh shopping cycle
            StartShoppingCycle();
        }

        Debug.Log($"{name}: Stopped interacting with player - INTERACTION UNLOCKED");
    }

    IEnumerator FacePlayer()
    {
        while (currentState == ShopperState.Interacting && player != null)
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
            }

            yield return null;
        }
    }

    public bool IsUsingZone(BuyZone zone)
    {
        return currentZone == zone;
    }

    void OnDestroy()
    {
        if (currentZone != null && ZoneManager.Instance != null)
        {
            currentZone.ClearAssignment();
            ZoneManager.Instance.ReleaseZone(currentZone, gameObject);
        }

        if (currentState == ShopperState.Interacting)
        {
            RestoreCameraAndCursor();
        }
    }
}