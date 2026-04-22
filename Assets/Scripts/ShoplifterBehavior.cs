using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using DialogueEditor;





[RequireComponent(typeof(NavMeshAgent))]
public class ShoplifterBehavior : MonoBehaviour
{
    [Header("Settings")]
    public float idleDuration = 5f;
    public List<BuyZone> walkInTargets;
    public Transform fleeTarget;
    public Transform deterredTarget;
    public Transform player;
    public Transform shoplifter;
    public float cautiousDurationBeforeDeterred = 5f;

    [Header("Face System")]
    public GameObject defaultFace;
    public GameObject confrontFace;
    public GameObject chillFace;

    [Header("Camera Control")]
    public MonoBehaviour playerCameraController;
    public Transform lookPoint;
    public bool freezeCameraOnInteract = true;
    public bool unlockCursorOnInteract = true;
    public float cameraTransitionSpeed = 2f;

    [Header("External Control")]
    public NPCConversation npcConversation;
    public bool IsDeterred = false;
    private bool hasBeenDeterredOnce = false;

    private NavMeshAgent agent;
    private Animator animator;
    private Coroutine stateCoroutine;
    private Coroutine interactionCoroutine;
    private Quaternion originalRotation;
    private Quaternion cachedRotation;
    private float idleTimer = 0f;
    private bool isIdleTimerPaused = false;
    private bool hasStarted = false;
    private BuyZone selectedWalkInTarget;
    private ShoplifterState savedState;
    private GameManager gameManager;

    // Face system variables
    private GameObject currentActiveFace;

    // Camera control variables
    // Camera control variabldettes
    private bool wasCameraEnabled;
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;
    private Camera playerCamera;
    private Coroutine cameraLookCoroutine;

    private BuyZone currentZone;


    // INTERACTION PROTECTION VARIABLES
    private bool isInteractionLocked = false; // Prevents state changes during dialogue

    private enum ShoplifterState
    {
        Waiting,
        WalkIn,
        Idle,
        Flee,
        Confront,
        Cautious,
        Inspected,
        Deterred,
        Interacting
    }

    private ShoplifterState currentState = ShoplifterState.Waiting;

    public bool IsCurrentlyInteracting()
    {
        return currentState == ShoplifterState.Interacting;
    }

    public bool IsInteracting => currentState == ShoplifterState.Interacting;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        originalRotation = transform.rotation;

        agent.enabled = true;
        agent.isStopped = false;

        if (agent.speed <= 0)
        {
            agent.speed = 3.5f;
            Debug.LogWarning($"{name}: Agent speed was 0, set to 3.5");
        }

        gameManager = FindObjectOfType<GameManager>();

        if (playerCameraController == null)
        {
            playerCameraController = FindObjectOfType<FirstPersonController>() as MonoBehaviour;
        }

        InitializeFaceSystem();
        SetState(ShoplifterState.Waiting);
        BeginShoplifting();

        if (ZoneManager.Instance != null)
        {
            ZoneManager.Instance.RegisterShoplifter(this);
        }
    }

    private void StartUsingZone(BuyZone zone)
    {
        currentZone = zone; // Your existing logic

        // Notify ZoneManager
        if (ZoneManager.Instance != null)
        {
            ZoneManager.Instance.SetShoplifterZone(this, zone);
        }
    }

    private void StopUsingZone(BuyZone zone)
    {
        // Notify ZoneManager before clearing
        if (ZoneManager.Instance != null)
        {
            ZoneManager.Instance.ClearShoplifterZone(this, zone);
        }

        currentZone = null; // Your existing logic
    }



    // Also call this when shoplifter is disabled/deactivated:
    void OnDisable()
    {
        if (ZoneManager.Instance != null)
        {
            ZoneManager.Instance.UnregisterShoplifter(this);
        }
    }

    void Update()
    {
        HandleLookBehavior();
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

    public void FreezeCameraAndUnlockCursor()
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

    public void RestoreCameraAndCursor()
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

    // ENHANCED STATE MANAGEMENT WITH INTERACTION PROTECTION
    private void SetState(ShoplifterState newState)
    {
        // CRITICAL: Prevent state changes during interaction
        if (isInteractionLocked && newState != ShoplifterState.Interacting)
        {
            Debug.Log($"{name}: State change to {newState} blocked - interaction is locked");
            return;
        }

        if (stateCoroutine != null)
            StopCoroutine(stateCoroutine);

        currentState = newState;

        switch (newState)
        {
            case ShoplifterState.Waiting:
                stateCoroutine = StartCoroutine(WaitingState());
                break;
            case ShoplifterState.WalkIn:
                stateCoroutine = StartCoroutine(WalkInState());
                break;
            case ShoplifterState.Idle:
                stateCoroutine = StartCoroutine(IdleState());
                break;
            case ShoplifterState.Flee:
                stateCoroutine = StartCoroutine(FleeState());
                break;
            case ShoplifterState.Confront:
                stateCoroutine = StartCoroutine(ConfrontState());
                break;
            case ShoplifterState.Cautious:
                stateCoroutine = StartCoroutine(CautiousState());
                break;
            case ShoplifterState.Inspected:
                stateCoroutine = StartCoroutine(InspectedState());
                break;
            case ShoplifterState.Deterred:
                stateCoroutine = StartCoroutine(DeterredState());
                break;
        }
    }

    private IEnumerator WaitingState()
    {
        while (!hasStarted && !isInteractionLocked) yield return null;
    }

    private IEnumerator WalkInState()
    {
        Debug.Log($"{name}: === STARTING WALK IN STATE ===");

        animator.SetBool("IsFlee", true);
        agent.isStopped = false;

        if (selectedWalkInTarget == null)
        {
            Debug.LogError($"{name}: selectedWalkInTarget is NULL in WalkInState!");
            yield break;
        }

        Debug.Log($"{name}: Walking to zone '{selectedWalkInTarget.name}' at position {selectedWalkInTarget.transform.position}");
        agent.SetDestination(selectedWalkInTarget.transform.position);

        // Debug agent state
        Debug.Log($"{name}: Agent state - isStopped: {agent.isStopped}, hasPath: {agent.hasPath}, pathStatus: {agent.pathStatus}");

        float stuckTimer = 0f;
        Vector3 lastPosition = transform.position;
        float maxWalkTime = 30f; // Maximum time to spend walking (prevents infinite loops)
        float walkTimer = 0f;

        // Wait until we reach the zone OR trigger detection handles it
        while (selectedWalkInTarget != null &&
               currentState == ShoplifterState.WalkIn && // Important: check if state changed via trigger
               !isInteractionLocked &&
               walkTimer < maxWalkTime)
        {
            walkTimer += Time.deltaTime;

            // Check if agent is stuck
            if (Vector3.Distance(transform.position, lastPosition) < 0.1f)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > 3f) // If stuck for 3 seconds
                {
                    Debug.LogWarning($"{name}: Agent appears stuck! Trying to recalculate path...");
                    agent.SetDestination(selectedWalkInTarget.transform.position);
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f;
                lastPosition = transform.position;
            }

            // Log progress every 2 seconds
            if (Time.frameCount % 120 == 0)
            {
                float distance = Vector3.Distance(transform.position, selectedWalkInTarget.transform.position);
                Debug.Log($"{name}: Distance to target: {distance:F2}, Velocity: {agent.velocity.magnitude:F2}");
            }

            // FALLBACK: If we're very close but trigger didn't fire
            float distanceToTarget = Vector3.Distance(transform.position, selectedWalkInTarget.transform.position);
            if (distanceToTarget < 1.0f && agent.velocity.magnitude < 0.1f)
            {
                Debug.LogWarning($"{name}: Very close to target but trigger didn't fire. Force entering zone.");
                OnReachedTargetZone();
                yield break;
            }

            yield return null;
        }

        // Handle timeout
        if (walkTimer >= maxWalkTime)
        {
            Debug.LogError($"{name}: Walk timeout! Forcing transition to Idle state.");
            if (selectedWalkInTarget != null)
            {
                OnReachedTargetZone();
            }
        }

        Debug.Log($"{name}: WalkInState coroutine ending. Current state: {currentState}");
    }

    private IEnumerator IdleState()
    {
        Debug.Log($"{name}: === STARTING IDLE STATE ===");

        idleTimer = 0f;
        isIdleTimerPaused = false;
        animator.SetBool("IsFlee", false);

        // Make sure agent is stopped when idling
        agent.isStopped = true;

        while (idleTimer < idleDuration && currentState == ShoplifterState.Idle && !isInteractionLocked)
        {
            if (!isIdleTimerPaused)
                idleTimer += Time.deltaTime;

            // Log progress every 5 seconds
            if (Time.frameCount % 300 == 0)
            {
                Debug.Log($"{name}: Idling... Time remaining: {(idleDuration - idleTimer):F1}s");
            }

            yield return null;
        }

        if (currentState == ShoplifterState.Interacting || isInteractionLocked)
        {
            Debug.Log($"{name}: Idle interrupted by interaction");
            yield break;
        }

        if (currentState == ShoplifterState.Idle) // Only transition if still in idle state
        {
            Debug.Log($"{name}: Idle time complete, transitioning to Flee");
            SetState(ShoplifterState.Flee);
        }
    }

    private IEnumerator FleeState()
    {
        // Don't flee if interaction is locked
        if (isInteractionLocked) yield break;

        Debug.Log($"{name}: 🏃 Starting Flee State");

        if (interactionCoroutine != null)
        {
            StopCoroutine(interactionCoroutine);
            interactionCoroutine = null;
        }

        animator.ResetTrigger("IsConfront");
        animator.ResetTrigger("IsChill");
        animator.SetBool("IsFlee", true);

        // FIX: Properly clear zone usage
        if (selectedWalkInTarget != null)
        {
            StopUsingZone(selectedWalkInTarget);
            selectedWalkInTarget.ClearAssignment();
            selectedWalkInTarget = null;
        }

        // CRITICAL: Ensure agent can move
        agent.isStopped = false;
        agent.enabled = true;

        if (fleeTarget != null)
        {
            Debug.Log($"{name}: Setting flee destination to {fleeTarget.position}");
            agent.SetDestination(fleeTarget.position);

            // Debug agent state
            Debug.Log($"{name}: Agent state - isStopped: {agent.isStopped}, hasPath: {agent.hasPath}, pathStatus: {agent.pathStatus}");

            // Wait a frame to let NavMesh calculate path
            yield return null;

            // Continue fleeing until we reach the target or state changes
            float fleeStartTime = Time.time;
            Vector3 lastPosition = transform.position;
            float stuckTimer = 0f;

            while (Vector3.Distance(transform.position, fleeTarget.position) > 1.0f &&
                   currentState == ShoplifterState.Flee && !isInteractionLocked)
            {
                // Check if agent is stuck
                if (Vector3.Distance(transform.position, lastPosition) < 0.1f)
                {
                    stuckTimer += Time.deltaTime;
                    if (stuckTimer > 2f)
                    {
                        Debug.LogWarning($"{name}: Agent stuck while fleeing! Recalculating path...");
                        agent.SetDestination(fleeTarget.position);
                        stuckTimer = 0f;
                    }
                }
                else
                {
                    stuckTimer = 0f;
                    lastPosition = transform.position;
                }

                // Log progress
                if (Time.frameCount % 120 == 0)
                {
                    float distance = Vector3.Distance(transform.position, fleeTarget.position);
                    Debug.Log($"{name}: Fleeing... Distance to exit: {distance:F2}, Velocity: {agent.velocity.magnitude:F2}");
                }

                // Timeout after 30 seconds
                if (Time.time - fleeStartTime > 30f)
                {
                    Debug.LogWarning($"{name}: Flee timeout! Force completing flee.");
                    break;
                }

                yield return null;
            }

            Debug.Log($"{name}: Flee complete - reached exit or state changed");
        }
        else
        {
            Debug.LogError($"{name}: Flee target is null!");
        }
    }

    private IEnumerator ConfrontState()
    {
        yield return null;
        SetState(ShoplifterState.Cautious);
    }

    private IEnumerator CautiousState()
    {
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < 1.5f && currentState != ShoplifterState.Interacting && !isInteractionLocked)
        {
            transform.rotation = Quaternion.Slerp(startRot, originalRotation, elapsed / 1.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = originalRotation;

        float cautiousTimer = 0f;
        while (cautiousTimer < cautiousDurationBeforeDeterred &&
               currentState != ShoplifterState.Interacting && !isInteractionLocked)
        {
            cautiousTimer += Time.deltaTime;
            yield return null;
        }

        if (currentState == ShoplifterState.Interacting || isInteractionLocked) yield break;

        SetState(ShoplifterState.Deterred);
    }

    private IEnumerator DeterredState()
    {
        // Don't move to deterred if interaction is locked
        if (isInteractionLocked) yield break;

        Debug.Log($"{name}: 😰 Starting Deterred State");

        animator.SetBool("IsFlee", true);

        // FIX: Properly clear zone usage before leaving
        if (selectedWalkInTarget != null)
        {
            StopUsingZone(selectedWalkInTarget);
            selectedWalkInTarget.ClearAssignment();
            selectedWalkInTarget = null;
        }

        // CRITICAL: Ensure agent can move
        agent.isStopped = false;
        agent.enabled = true;

        if (deterredTarget != null)
        {
            Debug.Log($"{name}: Moving to deterred position: {deterredTarget.position}");
            agent.SetDestination(deterredTarget.position);

            // Wait for path calculation
            yield return null;

            while (Vector3.Distance(transform.position, deterredTarget.position) > 0.5f &&
                   currentState == ShoplifterState.Deterred && !isInteractionLocked)
            {
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"{name}: Moving to deterred target. Distance: {Vector3.Distance(transform.position, deterredTarget.position):F2}");
                }
                yield return null;
            }

            Debug.Log($"{name}: Reached deterred position, now fleeing to exit.");

            if (fleeTarget != null && currentState == ShoplifterState.Deterred && !isInteractionLocked)
            {
                agent.SetDestination(fleeTarget.position);
                Debug.Log($"{name}: Now moving to flee target: {fleeTarget.position}");

                while (Vector3.Distance(transform.position, fleeTarget.position) > 1.0f &&
                       currentState == ShoplifterState.Deterred && !isInteractionLocked)
                {
                    if (Time.frameCount % 120 == 0)
                    {
                        Debug.Log($"{name}: Moving to exit. Distance: {Vector3.Distance(transform.position, fleeTarget.position):F2}");
                    }
                    yield return null;
                }
            }
        }
        else
        {
            Debug.LogWarning($"{name}: No deterred target assigned! Going directly to flee target.");

            if (fleeTarget != null)
            {
                agent.SetDestination(fleeTarget.position);

                while (Vector3.Distance(transform.position, fleeTarget.position) > 1.0f &&
                       currentState == ShoplifterState.Deterred && !isInteractionLocked)
                {
                    yield return null;
                }
            }
        }

        Debug.Log($"{name}: Deterred behavior complete.");
    }

    private IEnumerator InspectedState()
    {
        isIdleTimerPaused = true;
        animator.SetBool("IsFlee", false);
        agent.SetDestination(shoplifter.position);

        while (currentState == ShoplifterState.Inspected && !isInteractionLocked)
            yield return null;

        isIdleTimerPaused = false;
    }

    private void HandleLookBehavior()
    {
        if (currentState == ShoplifterState.Interacting && player != null)
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
            }
        }
    }

    // ENHANCED INTERACTION METHODS WITH LOCKING
    public void StartInteracting()
    {
        if (currentState == ShoplifterState.Interacting || isInteractionLocked)
        {
            Debug.Log($"{name}: Already interacting or interaction locked");
            return;
        }

        // LOCK THE INTERACTION - this prevents any state changes
        isInteractionLocked = true;

        savedState = currentState;
        cachedRotation = transform.rotation;
        currentState = ShoplifterState.Interacting;

        agent.isStopped = true;
        isIdleTimerPaused = true;

        FreezeCameraAndUnlockCursor();

        if (interactionCoroutine != null)
            StopCoroutine(interactionCoroutine);

        interactionCoroutine = StartCoroutine(PlayInteractionAnimationSequence());

        Debug.Log($"{name}: Started interacting with player - INTERACTION LOCKED");
    }

    private IEnumerator PlayInteractionAnimationSequence()
    {
        animator.SetTrigger("IsConfront");
        Confront();
        yield return new WaitForSeconds(1.0f);

        animator.SetTrigger("IsChill");
        Chill();
        yield return new WaitForSeconds(0.5f);

        if (savedState == ShoplifterState.WalkIn)
            animator.SetBool("IsFlee", true);
        else if (savedState == ShoplifterState.Idle)
            animator.SetBool("IsFlee", false);
    }

    public void StopInteracting()
    {
        if (currentState != ShoplifterState.Interacting)
        {
            Debug.Log($"{name}: StopInteracting called but not currently interacting");
            return;
        }

        Debug.Log($"{name}: StopInteracting called - UNLOCKING INTERACTION");

        Default();
        RestoreCameraAndCursor();

        // CRITICAL FIX: Handle Flee and Deterred states properly
        if (savedState == ShoplifterState.Flee || savedState == ShoplifterState.Deterred)
        {
            Debug.Log($"{name}: Resuming {savedState} state after interaction");

            // UNLOCK FIRST, then set the state
            isInteractionLocked = false;

            // Properly resume the flee/deterred behavior
            if (savedState == ShoplifterState.Flee)
            {
                SetState(ShoplifterState.Flee);
            }
            else if (savedState == ShoplifterState.Deterred)
            {
                SetState(ShoplifterState.Deterred);
            }
            return;
        }

        currentState = savedState;
        agent.isStopped = false;
        transform.rotation = cachedRotation;

        // UNLOCK THE INTERACTION - now state changes are allowed again
        isInteractionLocked = false;

        if (interactionCoroutine != null)
        {
            StopCoroutine(interactionCoroutine);
            interactionCoroutine = null;
        }

        switch (savedState)
        {
            case ShoplifterState.WalkIn:
                stateCoroutine = StartCoroutine(WalkInState());
                break;
            case ShoplifterState.Idle:
                isIdleTimerPaused = false;
                stateCoroutine = StartCoroutine(IdleState());
                break;
            case ShoplifterState.Confront:
                stateCoroutine = StartCoroutine(ConfrontState());
                break;
            case ShoplifterState.Cautious:
                stateCoroutine = StartCoroutine(CautiousState());
                break;
            case ShoplifterState.Inspected:
                stateCoroutine = StartCoroutine(InspectedState());
                break;
            default:
                SetState(ShoplifterState.Flee);
                break;
        }

        Debug.Log($"{name}: Stopped interacting with player - INTERACTION UNLOCKED");
    }

    public void SetInteractingWithPlayer(bool isInteracting)
    {
        if (isInteracting)
            StartInteracting();
        else
            StopInteracting();
    }

    public void SetInspected(bool value)
    {
        // ENHANCED PROTECTION: Don't change state during interaction
        if (isInteractionLocked)
        {
            Debug.Log($"{name}: SetInspected blocked - interaction is locked");
            return;
        }

        if (value && currentState != ShoplifterState.Inspected)
        {
            SetState(ShoplifterState.Inspected);
        }
        else if (!value && currentState == ShoplifterState.Inspected)
        {
            SetState(ShoplifterState.Idle);
            agent.SetDestination(fleeTarget.position);
            animator.SetBool("IsFlee", true);
        }
    }

    public void Deterred()
    {
        // ENHANCED PROTECTION: Don't change state during interaction
        if (isInteractionLocked)
        {
            Debug.Log($"{name}: Deterred() blocked - interaction is locked");
            return;
        }

        // NEW: Prevent multiple deterred scoring
        if (!hasBeenDeterredOnce)
        {
            hasBeenDeterredOnce = true; // Mark as deterred

            if (gameManager != null)
            {
                gameManager.ShoplifterDeterred();
                Debug.Log($"{name}: ✅ First time deterred - awarded points");
            }
            else
            {
                Debug.LogWarning($"{name}: GameManager not found! Cannot update deterred counter.");
            }
        }
        else
        {
            Debug.Log($"{name}: ⚠️ Already been deterred once - no additional points awarded");
        }

        IsDeterred = true;

        if (currentState != ShoplifterState.Deterred)
        {
            SetState(ShoplifterState.Deterred);
        }
    }

    public void BeginShoplifting()
    {
        Debug.Log($"{name}: === BEGIN SHOPLIFTING ===");

        if (isInteractionLocked)
        {
            Debug.Log($"{name}: BeginShoplifting blocked - interaction is locked");
            return;
        }

        if (walkInTargets == null || walkInTargets.Count == 0)
        {
            Debug.LogError($"{name}: No walk in targets configured!");
            return;
        }

        Debug.Log($"{name}: Attempting to reserve a walk-in target...");
        selectedWalkInTarget = ReserveWalkInTarget();

        if (selectedWalkInTarget == null)
        {
            Debug.LogWarning($"{name}: Failed to reserve target, will retry in 2 seconds");
            StartCoroutine(RetryReservation());
            return;
        }

        Debug.Log($"{name}: Successfully reserved target '{selectedWalkInTarget.name}', starting walk-in");
        hasStarted = true;
        SetState(ShoplifterState.WalkIn);
    }

    private BuyZone ReserveWalkInTarget()
    {
        Debug.Log($"{name}: === RESERVING WALK IN TARGET ===");
        Debug.Log($"{name}: Total walkInTargets: {walkInTargets?.Count ?? 0}");

        List<BuyZone> availableZones = new List<BuyZone>();

        foreach (BuyZone zone in walkInTargets)
        {
            if (zone == null)
            {
                Debug.LogWarning($"{name}: Found null zone in walkInTargets");
                continue;
            }

            Debug.Log($"{name}: Checking zone '{zone.name}':");
            Debug.Log($"  - IsOccupied: {zone.IsOccupied}");
            Debug.Log($"  - IsZoneOccupiedByShopper: {IsZoneOccupiedByShopper(zone)}");

            // Check ZoneManager too
            if (ZoneManager.Instance != null)
            {
                bool isReserved = ZoneManager.Instance.IsZoneReserved(zone);
                Debug.Log($"  - ZoneManager.IsZoneReserved: {isReserved}");
            }

            if (!zone.IsOccupied && !IsZoneOccupiedByShopper(zone))
            {
                availableZones.Add(zone);
                Debug.Log($"  - ✅ Zone is available");
            }
            else
            {
                Debug.Log($"  - ❌ Zone is NOT available");
            }
        }

        Debug.Log($"{name}: Found {availableZones.Count} available zones");

        if (availableZones.Count == 0)
        {
            Debug.LogWarning($"{name}: No available zones found!");
            return null;
        }

        BuyZone chosen = availableZones[Random.Range(0, availableZones.Count)];
        Debug.Log($"{name}: Chose zone '{chosen.name}'");

        // Assign the shopper
        chosen.AssignShopper(gameObject);
        Debug.Log($"{name}: Assigned shopper to zone '{chosen.name}'");

        selectedWalkInTarget = chosen;
        Debug.Log($"{name}: Set selectedWalkInTarget to '{chosen.name}'");

        return chosen;
    }

    private bool IsZoneOccupiedByShopper(BuyZone zone)
    {
        // First check ZoneManager (more efficient)
        if (ZoneManager.Instance != null)
        {
            bool zoneManagerOccupied = ZoneManager.Instance.IsZoneReserved(zone);
            if (zoneManagerOccupied)
            {
                Debug.Log($"{name}: Zone '{zone.name}' is reserved according to ZoneManager");
                return true;
            }
        }

        // Fallback to original method
        ShopperBehavior[] shoppers = FindObjectsOfType<ShopperBehavior>();
        foreach (ShopperBehavior shopper in shoppers)
        {
            if (shopper.IsUsingZone(zone))
            {
                Debug.Log($"{name}: Zone '{zone.name}' is occupied by shopper '{shopper.name}'");
                return true;
            }
        }

        return false;
    }

    private IEnumerator RetryReservation()
    {
        yield return new WaitForSeconds(2f);
        BeginShoplifting();
    }

    private void OnDestroy()
    {
        if (selectedWalkInTarget != null)
            selectedWalkInTarget.ClearAssignment();

        if (currentState == ShoplifterState.Interacting)
        {
            RestoreCameraAndCursor();
        }

        // Unregister from ZoneManager
        if (ZoneManager.Instance != null)
        {
            ZoneManager.Instance.UnregisterShoplifter(this);
        }
    }

    public void StartConversation()
    {
        if (npcConversation != null && !ConversationManager.Instance.IsConversationActive)
            ConversationManager.Instance.StartConversation(npcConversation);
    }

    public bool IsUsingZone(BuyZone zone)
    {
        return currentZone == zone;
    }

    public void DebugMovementStatus()
    {
        Debug.Log($"=== {name} Movement Debug ===");
        Debug.Log($"Current State: {currentState}");
        Debug.Log($"Interaction Locked: {isInteractionLocked}");
        Debug.Log($"Agent Enabled: {agent.enabled}");
        Debug.Log($"Agent isStopped: {agent.isStopped}");
        Debug.Log($"Agent Speed: {agent.speed}");
        Debug.Log($"Agent hasPath: {agent.hasPath}");
        Debug.Log($"Agent pathStatus: {agent.pathStatus}");
        Debug.Log($"Agent velocity: {agent.velocity}");
        Debug.Log($"Agent destination: {agent.destination}");
        Debug.Log($"Distance to destination: {Vector3.Distance(transform.position, agent.destination)}");
        Debug.Log($"Animator Apply Root Motion: {animator.applyRootMotion}");
        Debug.Log($"=== End Debug ===");
    }

    public void DebugShoplifterState()
    {
        Debug.Log($"=== {name} SHOPLIFTER DEBUG ===");
        Debug.Log($"Current State: {currentState}");
        Debug.Log($"Saved State: {savedState}");
        Debug.Log($"Interaction Locked: {isInteractionLocked}");
        Debug.Log($"Has Started: {hasStarted}");
        Debug.Log($"Is Deterred: {IsDeterred}");
        Debug.Log($"Has Been Deterred Once: {hasBeenDeterredOnce}"); // NEW
        Debug.Log($"Selected Walk In Target: {(selectedWalkInTarget != null ? selectedWalkInTarget.name : "NULL")}");
        Debug.Log($"Current Zone: {(currentZone != null ? currentZone.name : "NULL")}");

        if (agent != null)
        {
            Debug.Log($"Agent Enabled: {agent.enabled}");
            Debug.Log($"Agent isStopped: {agent.isStopped}");
            Debug.Log($"Agent Speed: {agent.speed}");
            Debug.Log($"Agent hasPath: {agent.hasPath}");
            Debug.Log($"Agent pathStatus: {agent.pathStatus}");
            Debug.Log($"Agent velocity: {agent.velocity}");
            Debug.Log($"Agent destination: {agent.destination}");
            if (agent.hasPath)
            {
                Debug.Log($"Distance to destination: {Vector3.Distance(transform.position, agent.destination)}");
            }
        }

        Debug.Log($"=== End Debug ===");
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if we entered a BuyZone
        BuyZone zone = other.GetComponent<BuyZone>();
        if (zone != null)
        {
            Debug.Log($"{name}: 🎯 TRIGGER ENTERED zone '{zone.name}'");

            // If this is our target zone, mark as reached
            if (zone == selectedWalkInTarget && currentState == ShoplifterState.WalkIn)
            {
                Debug.Log($"{name}: ✅ Reached our target zone '{zone.name}' via trigger!");
                OnReachedTargetZone();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        BuyZone zone = other.GetComponent<BuyZone>();
        if (zone != null)
        {
            Debug.Log($"{name}: 🚪 TRIGGER EXITED zone '{zone.name}'");

            // If we're leaving our current zone, clean up
            if (zone == currentZone)
            {
                Debug.Log($"{name}: Left our current zone '{zone.name}'");
                StopUsingZone(zone);
            }
        }
    }

    private void OnReachedTargetZone()
    {
        if (selectedWalkInTarget == null)
        {
            Debug.LogWarning($"{name}: OnReachedTargetZone called but selectedWalkInTarget is null!");
            return;
        }

        Debug.Log($"{name}: ✅ Successfully entered zone {selectedWalkInTarget.name}");

        // Notify ZoneManager
        StartUsingZone(selectedWalkInTarget);

        // Stop movement and transition to idle
        agent.isStopped = true;
        animator.SetBool("IsFlee", false);

        // Transition to idle state
        SetState(ShoplifterState.Idle);
    }


    public void DebugZoneDetection()
    {
        Debug.Log($"=== {name} ZONE DETECTION DEBUG ===");
        Debug.Log($"Current State: {currentState}");
        Debug.Log($"Selected Walk In Target: {(selectedWalkInTarget != null ? selectedWalkInTarget.name : "NULL")}");
        Debug.Log($"Current Zone: {(currentZone != null ? currentZone.name : "NULL")}");

        if (selectedWalkInTarget != null)
        {
            float distance = Vector3.Distance(transform.position, selectedWalkInTarget.transform.position);
            Debug.Log($"Distance to target: {distance:F2}");
            Debug.Log($"Target position: {selectedWalkInTarget.transform.position}");
            Debug.Log($"Shoplifter position: {transform.position}");

            // Check if target has a collider
            Collider targetCollider = selectedWalkInTarget.GetComponent<Collider>();
            if (targetCollider != null)
            {
                Debug.Log($"Target collider: {targetCollider.GetType().Name}, isTrigger: {targetCollider.isTrigger}");
            }
            else
            {
                Debug.LogWarning($"Target zone has no collider!");
            }
        }

        // Check shoplifter's collider
        Collider shoplifterCollider = GetComponent<Collider>();
        if (shoplifterCollider != null)
        {
            Debug.Log($"Shoplifter collider: {shoplifterCollider.GetType().Name}, isTrigger: {shoplifterCollider.isTrigger}");
        }
        else
        {
            Debug.LogWarning($"Shoplifter has no collider!");
        }

        Debug.Log($"=== End Zone Debug ===");



    }
}