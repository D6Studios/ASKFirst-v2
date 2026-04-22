using DialogueEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class ItemVariants
{
    [Header("Item Type")]
    public string itemName = "Item"; // For debugging/identification

    [Header("Item Variants")]
    public GameObject normalItem;
    public GameObject missingItem; // Removed tamperedItem

    public void DisableAll()
    {
        if (normalItem != null) normalItem.SetActive(false);
        if (missingItem != null) missingItem.SetActive(false);
    }

    public void EnableNormal()
    {
        DisableAll();
        if (normalItem != null) normalItem.SetActive(true);
    }

    public void EnableMissing()
    {
        DisableAll();
        if (missingItem != null) missingItem.SetActive(true);
    }

    public bool HasAnyVariant()
    {
        return normalItem != null || missingItem != null;
    }
}

[System.Serializable]
public class CashierItemCollection
{
    [Header("Multiple Item Types")]
    public List<ItemVariants> itemTypes = new List<ItemVariants>();

    public void DisableAllItems()
    {
        foreach (var itemType in itemTypes)
        {
            itemType.DisableAll();
        }
    }

    public ItemVariants GetRandomItemType()
    {
        var availableItems = itemTypes.Where(item => item.HasAnyVariant()).ToList();
        if (availableItems.Count == 0) return null;

        return availableItems[Random.Range(0, availableItems.Count)];
    }

    public ItemVariants GetItemByIndex(int index)
    {
        if (index >= 0 && index < itemTypes.Count)
            return itemTypes[index];
        return null;
    }
}

[System.Serializable]
public class CashierUIElements
{
    [Header("UI Elements for this Cashier")]
    public GameObject item1UI;
    public GameObject item2UI;
    public GameObject item3UI;
    public TextMeshProUGUI scannedCountText;

    public void DisableAllUI()
    {
        try
        {
            if (item1UI != null) item1UI.SetActive(false);
            if (item2UI != null) item2UI.SetActive(false);
            if (item3UI != null) item3UI.SetActive(false);
            Debug.Log("✅ CashierUIElements: All UI disabled");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ CashierUIElements: Error disabling UI: {e.Message}");
        }
    }

    public void UpdateScannedCount(int count)
    {
        try
        {
            if (scannedCountText != null)
            {
                scannedCountText.text = $"{count} Items Scanned";
                Debug.Log($"✅ CashierUIElements: Updated count to {count}");
            }
            else
            {
                Debug.LogWarning("⚠️ CashierUIElements: scannedCountText is null!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ CashierUIElements: Error updating count: {e.Message}");
        }
    }

    public void EnableUIForItem(int itemIndex)
    {
        try
        {
            Debug.Log($"🎯 CashierUIElements: Attempting to enable UI for item index {itemIndex}");

            GameObject targetUI = null;
            string itemName = "";

            switch (itemIndex)
            {
                case 0:
                    targetUI = item1UI;
                    itemName = "Item1";
                    break;
                case 1:
                    targetUI = item2UI;
                    itemName = "Item2";
                    break;
                case 2:
                    targetUI = item3UI;
                    itemName = "Item3";
                    break;
                default:
                    Debug.LogError($"❌ CashierUIElements: INVALID ITEM INDEX {itemIndex}! Valid range is 0-2");
                    return;
            }

            if (targetUI != null)
            {
                targetUI.SetActive(true);
                Debug.Log($"✅ CashierUIElements: Successfully enabled {itemName} UI (index {itemIndex})");
            }
            else
            {
                Debug.LogError($"❌ CashierUIElements: {itemName} UI GameObject is NULL! Check inspector assignments.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ CashierUIElements: Exception enabling UI for item {itemIndex}: {e.Message}");
        }
    }

    public bool ValidateUIElements()
    {
        bool isValid = true;

        if (item1UI == null) { Debug.LogWarning("⚠️ CashierUIElements: item1UI is null!"); isValid = false; }
        if (item2UI == null) { Debug.LogWarning("⚠️ CashierUIElements: item2UI is null!"); isValid = false; }
        if (item3UI == null) { Debug.LogWarning("⚠️ CashierUIElements: item3UI is null!"); isValid = false; }
        if (scannedCountText == null) { Debug.LogWarning("⚠️ CashierUIElements: scannedCountText is null!"); isValid = false; }

        return isValid;
    }
}

[System.Serializable]
public class CashierBasketPair
{
    public CashierZone zone;
    public GameObject basket;
    public Transform lookTarget;
    public CashierItemCollection itemCollection;
    public CashierUIElements uiElements; // Added UI elements
}

[System.Serializable]
public class NPCBasketItemWithVariants
{
    [Header("Basket Item Variants")]
    public string itemName = "Basket Item"; // For debugging
    public GameObject normalBasketItem; // Item with barcode
    public GameObject missingBarcodeBasketItem; // Item without barcode

    public void DisableAll()
    {
        if (normalBasketItem != null) normalBasketItem.SetActive(false);
        if (missingBarcodeBasketItem != null) missingBarcodeBasketItem.SetActive(false);
    }

    public void EnableNormal()
    {
        DisableAll();
        if (normalBasketItem != null) normalBasketItem.SetActive(true);
    }

    public void EnableMissingBarcode()
    {
        DisableAll();
        if (missingBarcodeBasketItem != null) missingBarcodeBasketItem.SetActive(true);
    }

    public bool HasVariants()
    {
        return normalBasketItem != null || missingBarcodeBasketItem != null;
    }
}

[System.Serializable]
public class NPCBasketItemsEnhanced
{
    [Header("NPC's Own Basket Items with Barcode Variants")]
    public NPCBasketItemWithVariants basketItem1;
    public NPCBasketItemWithVariants basketItem2;
    public NPCBasketItemWithVariants basketItem3;

    public void DisableAllBasketItems()
    {
        if (basketItem1 != null) basketItem1.DisableAll();
        if (basketItem2 != null) basketItem2.DisableAll();
        if (basketItem3 != null) basketItem3.DisableAll();
    }

    public void EnableBasketItem(int itemIndex, bool showMissingBarcode = false)
    {
        NPCBasketItemWithVariants targetItem = GetBasketItem(itemIndex);
        if (targetItem != null)
        {
            if (showMissingBarcode)
            {
                targetItem.EnableMissingBarcode();
            }
            else
            {
                targetItem.EnableNormal();
            }
        }
    }

    public void EnableBasketItems(int numberOfItems, List<bool> missingBarcodeFlags = null)
    {
        DisableAllBasketItems();

        for (int i = 0; i < numberOfItems && i < 3; i++)
        {
            bool showMissingBarcode = missingBarcodeFlags != null &&
                                    i < missingBarcodeFlags.Count &&
                                    missingBarcodeFlags[i];
            EnableBasketItem(i, showMissingBarcode);
        }
    }

    private NPCBasketItemWithVariants GetBasketItem(int index)
    {
        switch (index)
        {
            case 0: return basketItem1;
            case 1: return basketItem2;
            case 2: return basketItem3;
            default: return null;
        }
    }

    public bool HasBasketItem(int index)
    {
        NPCBasketItemWithVariants item = GetBasketItem(index);
        return item != null && item.HasVariants();
    }
}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class ScannerNPC : MonoBehaviour
{
    [Header("Queueing and Checkout Zones")]
    public List<LineSpot> lineSpots;
    public List<CashierZone> cashierZones;
    public Transform fleeZone;
    public Transform deterZone;

    [Header("NPC Behavior Settings")]
    public float cashierWaitDuration = 2.5f;

    [Header("Visuals")]
    public GameObject basket;

    [Header("NPC Basket Items with Barcode Variants")]
    public NPCBasketItemsEnhanced npcBasketItemsEnhanced;

    [Header("Face System")]
    [Header("Non-Suspicious Faces")]
    public GameObject normalDefaultFace;
    public GameObject normalChattingFace;
    public GameObject normalLookingFace;

    [Header("Suspicious Faces")]
    public GameObject suspiciousDefaultFace;
    public GameObject suspiciousChattingFace;
    public GameObject suspiciousLookingFace;

    [Header("Cashier Visuals")]
    public List<CashierBasketPair> cashierBaskets;
    private GameObject activeCashierBasket;
    private CashierItemCollection activeItemCollection;
    private CashierUIElements activeUIElements; // Added UI elements tracking
    private CashierZone chosenCashierZone;
    private Transform chosenLookTarget;

    [Header("Scanning Settings - FIXED TO 3 ITEMS")]
    [SerializeField] private int fixedItemsToScan = 3; // Fixed at 3 items
    public float scanDelay = 1f;

    [Header("Item Selection Settings")]
    [Tooltip("If true, randomly selects item types for each scan. If false, cycles through available types.")]
    public bool randomItemSelection = true;

    [Tooltip("If true, uses the same item type for the entire scanning session")]
    public bool useSameItemTypePerSession = false;

    [Header("Shoplifter Item Settings - ONLY MISSING ITEMS")]
    [Range(0f, 1f)] public float chanceToUseMissingItem = 0.6f; // Increased since it's the only steal method

    [Header("Feedback Settings")]
    public AudioClip scanSound;

    [Header("Shoplifter Settings")]
    public bool Shoplifter = false;
    [Range(0f, 1f)] public float chanceToSteal = 0.4f;
    public bool suspicious = false;
    public bool Caught { get; private set; } = false;

    [Header("Debug Info")]
    [SerializeField] private bool debugShoplifterStatus = true;
    [SerializeField] private bool debugItemSelection = true;

    [Header("Dialogue Settings")]
    public bool useDialogueSystem = true;
    public float dialogueSetupTimeout = 5f;
    public float initialDialogueDelay = 1f;

    [Header("Interaction Settings")]
    public Transform playerTransform;
    public float lookSpeed = 5f;

    [Header("Reservation Management")]
    private bool hasReservedCashier = false;
    private Coroutine cashierWaitCoroutine;
    private bool isScanningProtected = false;
    private Coroutine activeScanningCoroutine;

    private List<bool> scanResults;
    private List<ItemVariants> selectedItemsForSession; // Track which items to use for each scan
    private ItemVariants sessionItemType; // If using same item type per session
    private int scanIndex = 0;
    private int itemsToScan = 3; // Always 3 items
    private int scannedItemCount = 0; // Track successfully scanned items for UI
    private bool isWaitingToMove = false;
    private bool isScanning = false;

    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;

    private ScannerState currentState = ScannerState.Queuing;
    private ScannerState savedState;

    private int currentLineIndex = -1;
    private Quaternion cachedRotation;

    public GameManagerCashier gameManager;

    [Header("Caught Feedback")]
    public GameObject caughtParticle;
    public GameObject falseAccuseParticle;

    [Header("Audio Response Manager")]
    public AudioResponseManager audioResponseManager;

    [Header("Suspicious Visuals")]
    public GameObject suspicionParticle;

    [Header("Camera Control")]
    public MonoBehaviour playerCameraController;
    public Transform lookPoint;
    public bool freezeCameraOnInteract = true;
    public bool unlockCursorOnInteract = true;
    public float cameraTransitionSpeed = 2f;

    [Header("Shopping Bag")]
    public GameObject shoppingBag;

    private bool wasCameraEnabled;
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;
    private Camera playerCamera;
    private Coroutine cameraLookCoroutine;

    public NPCConversation myConversation;

    public bool Interacting => currentState == ScannerState.Interacting;

    private GameObject currentActiveFace;

    private enum ScannerState { Queuing, MovingToCashier, WaitingAtCashier, Fleeing, Interacting }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (basket != null) basket.SetActive(true);

        // Initialize basket items - disable all first, then enable a random number
        if (npcBasketItemsEnhanced != null)
        {
            npcBasketItemsEnhanced.DisableAllBasketItems();

            // Enable a random number of basket items (1-3) when NPC spawns
            int initialItemCount = 3; // 1, 2, or 3 items

            // For initial spawn, show all items as normal (with barcodes)
            // The barcode variants will be determined during scanning preparation
            npcBasketItemsEnhanced.EnableBasketItems(initialItemCount);

            if (debugItemSelection)
            {
                Debug.Log($"🧺 {name}: Spawned with {initialItemCount} normal items in basket");
            }
        }

        // Ensure shopping bag starts disabled
        if (shoppingBag != null) shoppingBag.SetActive(false);

        Shoplifter = Random.value < 0.25f;
        suspicious = Random.value < (Shoplifter ? 0.8f : 0.2f);

        if (debugShoplifterStatus)
        {
            Debug.Log($"🎯 {name} initialized - Shoplifter: {Shoplifter}, Suspicious: {suspicious}");
        }

        if (suspicionParticle != null)
            suspicionParticle.SetActive(suspicious);

        InitializeFaceSystem();

        if (useDialogueSystem)
        {
            StartCoroutine(SetupDialogueSuspicion());
        }

        TryMoveToNextAvailableLineSpot();
    }

    private void InitializeFaceSystem()
    {
        DisableAllFaces();
        Default();
    }

    private void DisableAllFaces()
    {
        if (normalDefaultFace != null) normalDefaultFace.SetActive(false);
        if (normalChattingFace != null) normalChattingFace.SetActive(false);
        if (normalLookingFace != null) normalLookingFace.SetActive(false);
        if (suspiciousDefaultFace != null) suspiciousDefaultFace.SetActive(false);
        if (suspiciousChattingFace != null) suspiciousChattingFace.SetActive(false);
        if (suspiciousLookingFace != null) suspiciousLookingFace.SetActive(false);

        currentActiveFace = null;
    }

    public void Default()
    {
        DisableAllFaces();

        if (suspicious)
        {
            if (suspiciousDefaultFace != null)
            {
                suspiciousDefaultFace.SetActive(true);
                currentActiveFace = suspiciousDefaultFace;
            }
        }
        else
        {
            if (normalDefaultFace != null)
            {
                normalDefaultFace.SetActive(true);
                currentActiveFace = normalDefaultFace;
            }
        }
    }

    public void Chatting()
    {
        DisableAllFaces();

        if (suspicious)
        {
            if (suspiciousChattingFace != null)
            {
                suspiciousChattingFace.SetActive(true);
                currentActiveFace = suspiciousChattingFace;
            }
        }
        else
        {
            if (normalChattingFace != null)
            {
                normalChattingFace.SetActive(true);
                currentActiveFace = normalChattingFace;
            }
        }
    }

    public void Looking()
    {
        DisableAllFaces();

        if (suspicious)
        {
            if (suspiciousLookingFace != null)
            {
                suspiciousLookingFace.SetActive(true);
                currentActiveFace = suspiciousLookingFace;
            }
        }
        else
        {
            if (normalLookingFace != null)
            {
                normalLookingFace.SetActive(true);
                currentActiveFace = normalLookingFace;
            }
        }
    }

    public GameObject GetCurrentFace()
    {
        return currentActiveFace;
    }

    private IEnumerator SetupDialogueSuspicion()
    {
        if (!useDialogueSystem)
        {
            if (debugShoplifterStatus)
            {
                Debug.Log($"📞 {name} - Dialogue system disabled, skipping parameter setup.");
            }
            yield break;
        }

        if (debugShoplifterStatus)
        {
            Debug.Log($"⏳ {name} - Waiting {initialDialogueDelay} seconds before setting up dialogue...");
        }
        yield return new WaitForSeconds(initialDialogueDelay);

        float waitTime = 0f;
        while (ConversationManager.Instance == null && waitTime < dialogueSetupTimeout)
        {
            if (debugShoplifterStatus && waitTime == 0f)
            {
                Debug.Log($"🔍 {name} - Looking for ConversationManager...");
            }
            yield return new WaitForSeconds(0.2f);
            waitTime += 0.2f;
        }

        if (ConversationManager.Instance == null)
        {
            Debug.LogWarning($"⚠️ {name} - ConversationManager not found after {dialogueSetupTimeout} seconds. Dialogue parameters not set.");
            yield break;
        }

        if (debugShoplifterStatus)
        {
            Debug.Log($"✅ {name} - ConversationManager found! Waiting additional 0.3s for full initialization...");
        }
        yield return new WaitForSeconds(0.3f);

        try
        {
            if (suspicious)
            {
                ConversationManager.Instance.SetBool("Sus", true);
                if (debugShoplifterStatus)
                {
                    Debug.Log($"✅ {name} - Dialogue parameter 'Sus' set to TRUE (suspicious NPC).");
                }
            }
            else
            {
                ConversationManager.Instance.SetBool("Sus", false);
                if (debugShoplifterStatus)
                {
                    Debug.Log($"✅ {name} - Dialogue parameter 'Sus' set to FALSE (normal NPC).");
                }
            }

            if (Shoplifter)
            {
                ConversationManager.Instance.SetBool("Shoplifter", true);
                if (debugShoplifterStatus)
                {
                    Debug.Log($"🎯 {name} - Dialogue parameter 'Shoplifter' set to TRUE (actual shoplifter).");
                }
            }
            else
            {
                ConversationManager.Instance.SetBool("Shoplifter", false);
                if (debugShoplifterStatus)
                {
                    Debug.Log($"🎯 {name} - Dialogue parameter 'Shoplifter' set to FALSE (innocent shopper).");
                }
            }

            if (debugShoplifterStatus)
            {
                Debug.Log($"🎉 {name} - Dialogue setup completed successfully!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ {name} - Failed to set dialogue parameters: {e.Message}");
        }
    }

    private void Update()
    {
        // Priority check: If caught shoplifter and not already fleeing, start immediate flee
        if (Caught && Shoplifter && currentState != ScannerState.Fleeing)
        {
            if (debugShoplifterStatus)
            {
                Debug.Log($"🏃 {name}: Update detected caught shoplifter not fleeing - triggering immediate flee");
            }
            StartCoroutine(ImmediateFlee());
            return;
        }

        if (currentState == ScannerState.Interacting && playerTransform != null)
        {
            Vector3 direction = playerTransform.position - transform.position;
            direction.y = 0f;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookSpeed);
            }
            return;
        }

        switch (currentState)
        {
            case ScannerState.Queuing:
                if (!agent.pathPending && agent.remainingDistance <= 0.1f && !isWaitingToMove)
                {
                    isWaitingToMove = true;
                    StartCoroutine(QueueAdvanceDelay());
                }
                break;

            case ScannerState.MovingToCashier:
                if (!agent.pathPending && agent.remainingDistance <= 0.1f)
                {
                    HandleArrivalAtCashier();
                }
                break;

            case ScannerState.Fleeing:
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && agent.velocity.sqrMagnitude < 0.06f)
                {
                    if (activeItemCollection != null)
                    {
                        activeItemCollection.DisableAllItems();
                    }

                    // Disable UI when leaving
                    if (activeUIElements != null)
                    {
                        activeUIElements.DisableAllUI();
                    }

                    Destroy(gameObject);
                }
                break;
        }

        UpdateAnimator();
    }

    private void HandleArrivalAtCashier()
    {
        // Double-check we're still assigned to this cashier
        if (chosenCashierZone == null || !hasReservedCashier)
        {
            Debug.LogWarning($"{name}: Arrived at cashier but no longer reserved! Finding new cashier...");
            hasReservedCashier = false;
            if (cashierWaitCoroutine == null)
            {
                cashierWaitCoroutine = StartCoroutine(WaitForOpenCashierRoutine());
            }
            return;
        }

        // Hide NPC's basket when they reach the cashier
        if (basket != null) basket.SetActive(false);

        if (chosenLookTarget != null)
        {
            Vector3 lookDirection = chosenLookTarget.position - transform.position;
            lookDirection.y = 0;
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        if (chosenCashierZone != null)
        {
            foreach (var pair in cashierBaskets)
            {
                if (pair.zone == chosenCashierZone)
                {
                    if (pair.basket != null)
                    {
                        pair.basket.SetActive(true);
                        activeCashierBasket = pair.basket;
                        Debug.Log($"✅ {name}: Activated cashier basket");
                    }

                    activeItemCollection = pair.itemCollection;
                    activeUIElements = pair.uiElements;

                    // Validate UI elements
                    if (activeUIElements != null)
                    {
                        Debug.Log($"🔍 {name}: UI Elements assigned - validating...");
                        if (!activeUIElements.ValidateUIElements())
                        {
                            Debug.LogError($"⚠️ {name}: UI validation failed! Check inspector assignments.");
                        }
                    }
                    else
                    {
                        Debug.LogError($"⚠️ {name}: No UI elements assigned to this cashier pair!");
                    }

                    // Validate item collection
                    if (activeItemCollection != null)
                    {
                        Debug.Log($"✅ {name}: Item collection assigned with {activeItemCollection.itemTypes.Count} item types");
                        if (activeItemCollection.itemTypes.Count == 0)
                        {
                            Debug.LogError($"⚠️ {name}: Item collection has no item types!");
                        }
                    }
                    else
                    {
                        Debug.LogError($"⚠️ {name}: No item collection assigned to this cashier pair!");
                    }

                    // Initialize scanning items (all disabled initially)
                    if (activeItemCollection != null)
                    {
                        activeItemCollection.DisableAllItems();
                    }

                    // ENHANCED UI INITIALIZATION - Reset everything properly
                    ResetUIState();
                    Debug.Log($"✅ {name}: UI initialized and reset");

                    break;
                }
            }
        }

        currentState = ScannerState.WaitingAtCashier;
        Debug.Log($"🎯 {name}: Setup complete - starting scanning routine");
        StartCoroutine(ScanItemsRoutine());
    }

    private IEnumerator QueueAdvanceDelay()
    {
        yield return new WaitForSeconds(1f);
        TryMoveToNextAvailableLineSpot();
        isWaitingToMove = false;
    }

    private void TryMoveToNextAvailableLineSpot()
    {
        // Don't move in line if we already have a cashier reservation
        if (hasReservedCashier) return;

        for (int i = currentLineIndex + 1; i < lineSpots.Count; i++)
        {
            if (!lineSpots[i].IsOccupied)
            {
                if (currentLineIndex >= 0) lineSpots[currentLineIndex].IsOccupied = false;

                currentLineIndex = i;
                lineSpots[i].IsOccupied = true;

                agent.SetDestination(lineSpots[i].transform.position);
                return;
            }
        }

        // Only start cashier waiting if we haven't already
        if (cashierWaitCoroutine == null && !hasReservedCashier)
        {
            cashierWaitCoroutine = StartCoroutine(WaitForOpenCashierRoutine());
        }
    }

    private IEnumerator WaitForOpenCashierRoutine()
    {
        while (!hasReservedCashier)
        {
            CashierZone assignedZone = null;

            // Try zone manager first if available
            if (CashierZoneManager.Instance != null)
            {
                assignedZone = CashierZoneManager.Instance.TryReserveCashier(gameObject);
            }
            else
            {
                // Fallback to original method if no zone manager
                var openCashiers = cashierZones.Where(c => c.IsAvailable).ToList();
                if (openCashiers.Count > 0)
                {
                    assignedZone = openCashiers[Random.Range(0, openCashiers.Count)];
                    assignedZone.Reserve(gameObject);
                }
            }

            if (assignedZone != null)
            {
                chosenCashierZone = assignedZone;
                hasReservedCashier = true;

                agent.SetDestination(chosenCashierZone.transform.position);
                chosenLookTarget = cashierBaskets.FirstOrDefault(p => p.zone == chosenCashierZone)?.lookTarget;

                if (currentLineIndex >= 0)
                {
                    lineSpots[currentLineIndex].IsOccupied = false;
                    currentLineIndex = -1;
                }

                currentState = ScannerState.MovingToCashier;
                cashierWaitCoroutine = null;
                yield break;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator ScanItemsRoutine()
    {
        // Prevent multiple scanning coroutines
        if (isScanningProtected)
        {
            Debug.LogWarning($"{name}: Already scanning, ignoring duplicate call");
            yield break;
        }

        // Verify we're still at the right cashier
        if (chosenCashierZone == null || !hasReservedCashier)
        {
            Debug.LogWarning($"{name}: No valid cashier reservation, but forcing scan anyway (DEBUG MODE).");

            // Fallback: grab the first cashierBasket if available
            if (activeItemCollection == null && cashierBaskets.Count > 0)
            {
                var fallback = cashierBaskets[0];
                activeItemCollection = fallback.itemCollection;
                activeUIElements = fallback.uiElements;
                chosenCashierZone = fallback.zone;
            }
        }

        isScanningProtected = true;
        activeScanningCoroutine = StartCoroutine(ActualScanningProcess());

        yield return activeScanningCoroutine;

        isScanningProtected = false;
        activeScanningCoroutine = null;
    }

    private void PrepareItemsForScanning()
    {
        if (activeItemCollection == null)
        {
            Debug.LogError($"{name}: No active item collection found!");
            return;
        }

        if (activeItemCollection.itemTypes == null || activeItemCollection.itemTypes.Count == 0)
        {
            Debug.LogError($"{name}: Active item collection has no item types!");
            return;
        }

        // FIXED: Use a consistent number of items (3) and ensure proper indexing
        itemsToScan = 3; // Always scan 3 items for consistency

        Debug.Log($"📋 {name}: Preparing to scan {itemsToScan} items");

        // Pre-determine which items will be stolen - ALIGNED WITH INDICES
        List<bool> willStealItems = new List<bool>();
        int plannedSteals = 0;

        for (int i = 0; i < itemsToScan; i++)
        {
            bool willSteal = Shoplifter && Random.value < chanceToSteal;
            willStealItems.Add(willSteal);
            if (willSteal) plannedSteals++;
        }

        // Ensure shoplifters steal at least 1 item
        if (Shoplifter && plannedSteals == 0 && itemsToScan > 0)
        {
            int forceStealIndex = Random.Range(0, itemsToScan);
            willStealItems[forceStealIndex] = true;
            plannedSteals = 1;
            Debug.Log($"🎯 {name}: Forced steal at index {forceStealIndex}");
        }

        // Store the steal plan - THIS IS THE CRITICAL ALIGNMENT
        scanResults = willStealItems;

        // Update basket items to show correct variants - SAME INDICES AS SCAN RESULTS
        if (npcBasketItemsEnhanced != null)
        {
            npcBasketItemsEnhanced.EnableBasketItems(itemsToScan, willStealItems);

            if (debugItemSelection)
            {
                for (int i = 0; i < itemsToScan; i++)
                {
                    string status = willStealItems[i] ? "MISSING BARCODE (STEAL)" : "NORMAL BARCODE (SCAN)";
                    Debug.Log($"🧺 {name}: Basket Item {i}: {status}");
                }
            }
        }

        // Prepare scanning items list - CRITICAL: USE SAME INDICES
        selectedItemsForSession = new List<ItemVariants>();
        for (int i = 0; i < itemsToScan; i++)
        {
            // Ensure we don't go out of bounds
            int itemTypeIndex = i % activeItemCollection.itemTypes.Count;
            ItemVariants selectedItem = activeItemCollection.itemTypes[itemTypeIndex];
            selectedItemsForSession.Add(selectedItem);

            if (debugItemSelection)
            {
                string stealStatus = willStealItems[i] ? " (WILL STEAL)" : " (WILL SCAN)";
                Debug.Log($"🎯 {name}: Scan Index {i} -> ItemType Index {itemTypeIndex} ({selectedItem?.itemName}){stealStatus}");
            }
        }

        Debug.Log($"🛒 {name}: INDEX ALIGNMENT - Items: {itemsToScan}, Steals: {plannedSteals}, ScanResults.Count: {scanResults.Count}, SelectedItems.Count: {selectedItemsForSession.Count}");

        // Disable all scanning items initially
        if (activeItemCollection != null)
        {
            activeItemCollection.DisableAllItems();
        }
    }

    private IEnumerator ActualScanningProcess()
    {
        selectedItemsForSession = new List<ItemVariants>();
        scanIndex = 0;
        scannedItemCount = 0; // This tracks LEGITIMATE scans for UI

        PrepareItemsForScanning();

        // VALIDATION: Ensure all arrays are the same length
        if (scanResults == null)
        {
            Debug.LogError($"{name}: scanResults is null after PrepareItemsForScanning!");
            yield break;
        }

        if (selectedItemsForSession == null)
        {
            Debug.LogError($"{name}: selectedItemsForSession is null after PrepareItemsForScanning!");
            yield break;
        }

        if (scanResults.Count != selectedItemsForSession.Count)
        {
            Debug.LogError($"{name}: INDEX MISMATCH! scanResults.Count: {scanResults.Count}, selectedItems.Count: {selectedItemsForSession.Count}");
            yield break;
        }

        Debug.Log($"🎬 {name}: Starting scanning with {scanResults.Count} items. Index validation passed.");

        if (debugShoplifterStatus)
        {
            int stolenItems = scanResults.Count(x => x); // true = stolen
            int scannedItems = scanResults.Count(x => !x); // false = scanned
            Debug.Log($"🛒 {name} SCAN PLAN - Total: {itemsToScan}, Stolen: {stolenItems}, Scanned: {scannedItems}, Shoplifter: {Shoplifter}");
        }

        animator.SetBool("IsWalking", false);
        animator.SetBool("Idle", true);

        yield return new WaitForSeconds(0.5f);
        isScanning = true;

        while (scanIndex < scanResults.Count)
        {
            // Immediate flee check
            if (Caught && Shoplifter)
            {
                Debug.Log($"🏃 {name}: SCAN {scanIndex} interrupted - caught shoplifter fleeing!");
                isScanning = false;
                yield break;
            }

            // State validation
            if (currentState != ScannerState.WaitingAtCashier && currentState != ScannerState.Interacting)
            {
                Debug.LogWarning($"{name}: SCAN {scanIndex} interrupted - state changed to {currentState}");
                break;
            }

            if (currentState == ScannerState.Interacting)
            {
                yield return null;
                continue;
            }

            // INDEX SAFETY CHECK
            if (scanIndex >= scanResults.Count || scanIndex >= selectedItemsForSession.Count)
            {
                Debug.LogError($"{name}: INDEX OUT OF BOUNDS! scanIndex: {scanIndex}, scanResults.Count: {scanResults.Count}, selectedItems.Count: {selectedItemsForSession.Count}");
                break;
            }

            bool isStolen = scanResults[scanIndex]; // true = stolen
            bool isLegitimate = !isStolen; // false = stolen, true = legitimate

            Debug.Log($"🔄 {name}: Processing SCAN {scanIndex} - IsStolen: {isStolen}, IsLegitimate: {isLegitimate}");

            // Set up the scanning item for current scan
            SetupItemForScanning(isLegitimate, scanIndex);

            if (isLegitimate) // Item will be properly scanned
            {
                animator.SetTrigger("IsScanning");
                ItemScanned(); // Play sound

                // CRITICAL UI UPDATE - Map scan index to UI display position
                if (activeUIElements != null)
                {
                    // Instead of using scanIndex directly, use scannedItemCount for UI positioning
                    // This ensures UI items appear in order: first legitimate scan goes to UI position 0, etc.
                    int uiPosition = scannedItemCount; // Current UI position (0, 1, 2...)

                    Debug.Log($"📺 {name}: SCAN {scanIndex} -> UI position {uiPosition} (scannedCount: {scannedItemCount})");

                    if (uiPosition < 3) // Ensure we don't exceed UI slots
                    {
                        activeUIElements.EnableUIForItem(uiPosition);
                        scannedItemCount++;
                        activeUIElements.UpdateScannedCount(scannedItemCount);
                        Debug.Log($"✅ {name}: SCAN {scanIndex} SUCCESS - UI position {uiPosition} enabled, count now {scannedItemCount}");
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ {name}: SCAN {scanIndex} - UI position {uiPosition} exceeds available slots!");
                    }
                }
                else
                {
                    Debug.LogError($"❌ {name}: SCAN {scanIndex} - No active UI elements!");
                }
            }
            else // Item is being stolen
            {
                animator.SetTrigger("IsFake");
                Debug.Log($"🤫 {name}: SCAN {scanIndex} STOLEN - no sound, no UI update (scannedCount remains: {scannedItemCount})");
                // Note: scannedItemCount is NOT incremented for stolen items
            }

            scanIndex++;
            yield return new WaitForSeconds(scanDelay);

            // Hide the scanning item after processing
            if (activeItemCollection != null)
            {
                activeItemCollection.DisableAllItems();
            }
        }

        isScanning = false;
        Debug.Log($"🏁 {name}: Scanning completed. Final scan index: {scanIndex}, Final UI count: {scannedItemCount}");

        // Continue with the rest of the method
        if (scanIndex >= scanResults.Count && !(Caught && Shoplifter))
        {
            StartCoroutine(WaitAtCashierThenFlee());
        }
        else if (Caught && Shoplifter)
        {
            Debug.Log($"🏃 {name}: Caught shoplifter will be handled by ImmediateFlee");
        }
    }


    private void SetupItemForScanning(bool isLegitScan, int currentScanIndex)
    {
        if (activeItemCollection == null)
        {
            Debug.LogError($"{name}: No active item collection!");
            return;
        }

        if (activeItemCollection.itemTypes == null || activeItemCollection.itemTypes.Count == 0)
        {
            Debug.LogError($"{name}: Active item collection has no item types!");
            return;
        }

        // CRITICAL FIX: Use selectedItemsForSession instead of direct indexing
        ItemVariants currentItem = null;

        if (selectedItemsForSession != null && currentScanIndex < selectedItemsForSession.Count)
        {
            currentItem = selectedItemsForSession[currentScanIndex];
        }
        else
        {
            // Fallback: use modulo to prevent out-of-bounds
            int fallbackIndex = currentScanIndex % activeItemCollection.itemTypes.Count;
            currentItem = activeItemCollection.itemTypes[fallbackIndex];
            Debug.LogWarning($"⚠️ {name}: Using fallback item at index {fallbackIndex} for scan {currentScanIndex}");
        }

        if (currentItem == null)
        {
            Debug.LogError($"{name}: No scanning item found for scan index {currentScanIndex}!");
            return;
        }

        // First disable all items in the collection
        activeItemCollection.DisableAllItems();

        if (isLegitScan)
        {
            currentItem.EnableNormal();
            Debug.Log($"✅ {name}: SCAN {currentScanIndex} -> Enabling NORMAL item ({currentItem.itemName}) - WILL UPDATE UI");
        }
        else if (Shoplifter)
        {
            currentItem.EnableMissing();
            Debug.Log($"🚫 {name}: SCAN {currentScanIndex} -> Enabling MISSING item ({currentItem.itemName}) - NO UI UPDATE");
        }
        else
        {
            currentItem.EnableNormal();
            Debug.LogWarning($"⚠️ {name}: SCAN {currentScanIndex} -> Non-shoplifter fake scan, using normal item");
        }
    }

    private bool ValidateIndexAlignment()
    {
        bool isValid = true;

        if (scanResults == null)
        {
            Debug.LogError($"{name}: scanResults is null!");
            isValid = false;
        }

        if (selectedItemsForSession == null)
        {
            Debug.LogError($"{name}: selectedItemsForSession is null!");
            isValid = false;
        }

        if (scanResults != null && selectedItemsForSession != null && scanResults.Count != selectedItemsForSession.Count)
        {
            Debug.LogError($"{name}: COUNT MISMATCH! scanResults: {scanResults.Count}, selectedItems: {selectedItemsForSession.Count}");
            isValid = false;
        }

        if (activeItemCollection != null && activeItemCollection.itemTypes.Count < 3)
        {
            Debug.LogWarning($"{name}: Item collection has only {activeItemCollection.itemTypes.Count} types, need at least 3!");
        }

        return isValid;
    }

    private IEnumerator WaitAtCashierThenFlee()
    {
        yield return new WaitForSeconds(cashierWaitDuration);

        // Release through zone manager if available, otherwise fallback to direct release
        if (chosenCashierZone != null && hasReservedCashier)
        {
            if (CashierZoneManager.Instance != null)
            {
                CashierZoneManager.Instance.ReleaseCashier(chosenCashierZone, gameObject);
            }
            else
            {
                chosenCashierZone.Release();
            }
            hasReservedCashier = false;
        }

        NavMeshPath path = new NavMeshPath();
        Transform destination = (Shoplifter && Caught && deterZone != null) ? deterZone : fleeZone;

        if (agent.CalculatePath(destination.position, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            agent.ResetPath();
            yield return null;
            agent.SetDestination(destination.position);
            currentState = ScannerState.Fleeing;

            // Hide cashier basket and items
            if (activeCashierBasket != null)
                activeCashierBasket.SetActive(false);

            if (activeItemCollection != null)
            {
                activeItemCollection.DisableAllItems();
            }

            // Disable UI when fleeing
            if (activeUIElements != null)
            {
                activeUIElements.DisableAllUI();
            }

            // Enable shopping bag when starting to flee
            EnableShoppingBag();
        }
        else
        {
            Debug.LogError($"{name} could NOT find a valid path to destination zone!");
        }
    }

    private void UpdateAnimator()
    {
        if (currentState == ScannerState.Interacting || isScanning) return;

        bool isMoving = agent.velocity.sqrMagnitude > 0.05f;

        animator.SetBool("IsWalking", isMoving);
        animator.SetBool("Idle", !isMoving);
    }

    private void OnTriggerExit(Collider other)
    {
        CashierZone zone = other.GetComponent<CashierZone>();
        if (zone != null && zone == chosenCashierZone && hasReservedCashier)
        {
            // Only release if we're actually fleeing or destroyed
            if (currentState == ScannerState.Fleeing)
            {
                if (CashierZoneManager.Instance != null)
                {
                    CashierZoneManager.Instance.ReleaseCashier(zone, gameObject);
                }
                else
                {
                    zone.Release();
                }
                hasReservedCashier = false;
            }
        }
    }

    private void ResetUIState()
    {
        if (activeUIElements != null)
        {
            activeUIElements.DisableAllUI();
            activeUIElements.UpdateScannedCount(0);
            scannedItemCount = 0;
            Debug.Log($"🔄 {name}: UI state reset - all items disabled, count reset to 0");
        }
    }

    public void ItemScanned()
    {
        if (scanSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(scanSound);
        }

        // No green screen flash anymore - UI handles feedback
    }

    public void Interact()
    {
        if (currentState == ScannerState.Interacting) return;

        savedState = currentState;
        cachedRotation = transform.rotation;
        currentState = ScannerState.Interacting;
        agent.isStopped = true;
        animator.SetBool("IsWalking", false);
        animator.SetBool("Idle", true);

        Chatting();
    }

    public void StopInteract()
    {
        Debug.LogWarning("interact stopped.");
        if (currentState != ScannerState.Interacting) return;

        // Check if this NPC was caught while interacting
        if (Caught && Shoplifter)
        {
            // Don't restore previous state, let ImmediateFlee handle it
            if (debugShoplifterStatus)
            {
                Debug.Log($"🏃 {name}: Interaction stopped - caught shoplifter will flee immediately");
            }
            RestoreCameraAndCursor();
            Default();
            return;
        }

        currentState = savedState;
        agent.isStopped = false;
        transform.rotation = cachedRotation;
        RestoreCameraAndCursor();

        Default();

        // Only restart scanning if we were in the middle of it and it's protected
        if (savedState == ScannerState.WaitingAtCashier && isScanningProtected && activeScanningCoroutine == null)
        {
            Debug.Log($"{name}: Returning to protected scanning process");
        }
    }

    public void StartDialogue()
    {
        FreezeCameraAndUnlockCursor();
        if (myConversation != null)
        {
            ConversationManager.Instance.StartConversation(myConversation);
        }
        else
        {
            Debug.LogWarning("No Conversation assigned in ButtonBehavior.");
        }
    }

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

    public void IsCaught()
    {
        Caught = true;

        if (Shoplifter)
        {
            Debug.Log($"{name} has been caught! Fleeing immediately to deter zone.");

            if (gameManager != null)
                gameManager.ShoplifterDeterred();

            if (audioResponseManager != null)
                audioResponseManager.GoodResponse();

            if (caughtParticle != null)
            {
                GameObject pfx = Instantiate(caughtParticle, transform.position + Vector3.up * 1f, Quaternion.identity);
                Destroy(pfx, 5f);
            }

            // Immediately flee when caught - interrupt any current activity
            StartCoroutine(ImmediateFlee());
        }
        else
        {
            Debug.Log($"{name} was falsely accused! Not a shoplifter.");

            if (gameManager != null)
                gameManager.ShopperFalselyAccused();

            if (audioResponseManager != null)
                audioResponseManager.BadResponse();

            if (falseAccuseParticle != null)
            {
                GameObject pfx = Instantiate(falseAccuseParticle, transform.position + Vector3.up * 1f, Quaternion.identity);
                Destroy(pfx, 5f);
            }
        }
    }

    /// <summary>
    /// Immediately flee when caught - interrupts all current activities
    /// </summary>
    private IEnumerator ImmediateFlee()
    {
        // Stop any ongoing scanning
        if (activeScanningCoroutine != null)
        {
            StopCoroutine(activeScanningCoroutine);
            activeScanningCoroutine = null;
        }

        // Stop cashier waiting
        if (cashierWaitCoroutine != null)
        {
            StopCoroutine(cashierWaitCoroutine);
            cashierWaitCoroutine = null;
        }

        // Reset scanning protection flags
        isScanningProtected = false;
        isScanning = false;

        // Clean up visuals immediately
        if (basket != null) basket.SetActive(false);
        if (activeCashierBasket != null) activeCashierBasket.SetActive(false);
        if (activeItemCollection != null) activeItemCollection.DisableAllItems();

        // Clean up UI immediately
        if (activeUIElements != null)
        {
            activeUIElements.DisableAllUI();
        }

        // Release cashier reservation immediately
        if (chosenCashierZone != null && hasReservedCashier)
        {
            if (CashierZoneManager.Instance != null)
            {
                CashierZoneManager.Instance.ReleaseCashier(chosenCashierZone, gameObject);
            }
            else
            {
                chosenCashierZone.Release();
            }
            hasReservedCashier = false;
            chosenCashierZone = null;
        }

        // Release line spot if occupied
        if (currentLineIndex >= 0 && currentLineIndex < lineSpots.Count)
        {
            lineSpots[currentLineIndex].IsOccupied = false;
            currentLineIndex = -1;
        }

        // Set state to fleeing
        currentState = ScannerState.Fleeing;
        agent.isStopped = false;

        // Enable shopping bag for immediate flee
        EnableShoppingBag();

        // Calculate path to deter zone (caught shoplifters go to deter zone)
        Transform destination = deterZone != null ? deterZone : fleeZone;
        NavMeshPath path = new NavMeshPath();

        if (agent.CalculatePath(destination.position, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            agent.ResetPath();
            yield return null; // Wait one frame for agent reset
            agent.SetDestination(destination.position);

            if (debugShoplifterStatus)
            {
                Debug.Log($"🏃 {name} (CAUGHT SHOPLIFTER) immediately fleeing to {destination.name}");
            }
        }
        else
        {
            Debug.LogError($"{name} could NOT find a valid path to deter zone! Trying flee zone instead.");

            // Fallback to flee zone if deter zone path fails
            if (fleeZone != null && agent.CalculatePath(fleeZone.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                agent.ResetPath();
                yield return null;
                agent.SetDestination(fleeZone.position);
            }
            else
            {
                Debug.LogError($"{name} cannot find any valid escape path! Destroying immediately.");
                RestoreCameraAndCursor();
                Destroy(gameObject);
            }
        }
    }

    private void EnableShoppingBag()
    {
        if (shoppingBag != null)
        {
            shoppingBag.SetActive(true);
            if (debugShoplifterStatus)
            {
                Debug.Log($"🛍️ {name}: Shopping bag enabled - NPC is now fleeing/exiting with items");
            }
        }
        else if (debugShoplifterStatus)
        {
            Debug.LogWarning($"⚠️ {name}: No shopping bag assigned in inspector!");
        }
    }

    private void OnDestroy()
    {
        Debug.Log($"🗑️ {name}: OnDestroy called - cleaning up reservations");

        // Stop any running coroutines first
        if (cashierWaitCoroutine != null)
        {
            StopCoroutine(cashierWaitCoroutine);
            cashierWaitCoroutine = null;
        }

        if (activeScanningCoroutine != null)
        {
            StopCoroutine(activeScanningCoroutine);
            activeScanningCoroutine = null;
        }

        // Safe cleanup with zone manager - ADD NULL CHECKS
        if (CashierZoneManager.Instance != null)
        {
            try
            {
                CashierZoneManager.Instance.ForceReleaseAllForNPC(gameObject);
                Debug.Log($"✅ {name}: Zone manager cleanup completed");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ {name}: Zone manager cleanup failed: {e.Message}");
            }
        }

        // Fallback cleanup if no zone manager - ADD NULL CHECK FOR CASHIER ZONE
        if (chosenCashierZone != null && hasReservedCashier)
        {
            try
            {
                // Additional null check before calling Release
                if (chosenCashierZone != null && chosenCashierZone.gameObject != null)
                {
                    chosenCashierZone.Release();
                    Debug.Log($"✅ {name}: Direct cashier cleanup completed");
                }
                else
                {
                    Debug.Log($"ℹ️ {name}: CashierZone was already destroyed - skipping direct release");
                }
            }
            catch (MissingReferenceException)
            {
                Debug.Log($"ℹ️ {name}: CashierZone was already destroyed - skipping direct release");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ {name}: Direct cashier cleanup failed: {e.Message}");
            }
            finally
            {
                hasReservedCashier = false;
                chosenCashierZone = null;
            }
        }

        // Release line spot if occupied
        if (currentLineIndex >= 0 && currentLineIndex < lineSpots.Count && lineSpots[currentLineIndex] != null)
        {
            try
            {
                lineSpots[currentLineIndex].IsOccupied = false;
                Debug.Log($"✅ {name}: Line spot {currentLineIndex} released");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ {name}: Line spot cleanup failed: {e.Message}");
            }
        }

        // Clean up UI when destroyed
        if (activeUIElements != null)
        {
            try
            {
                activeUIElements.DisableAllUI();
                Debug.Log($"✅ {name}: UI cleanup completed");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ {name}: UI cleanup failed: {e.Message}");
            }
        }

        // Restore camera if NPC was interacting
        try
        {
            //RestoreCameraAndCursor();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ {name}: Camera restoration failed: {e.Message}");
        }

        Debug.Log($"🏁 {name}: OnDestroy cleanup completed");
    }

    #region Public Helper Methods for Item Management

    /// <summary>
    /// Get the currently active item collection for this NPC
    /// </summary>
    public CashierItemCollection GetActiveItemCollection()
    {
        return activeItemCollection;
    }

    /// <summary>
    /// Get the currently active UI elements for this NPC
    /// </summary>
    public CashierUIElements GetActiveUIElements()
    {
        return activeUIElements;
    }

    /// <summary>
    /// Get the current scanned item count
    /// </summary>
    public int GetScannedItemCount()
    {
        return scannedItemCount;
    }

    /// <summary>
    /// Get all available item types from the active collection
    /// </summary>
    public List<ItemVariants> GetAvailableItemTypes()
    {
        if (activeItemCollection == null) return new List<ItemVariants>();
        return activeItemCollection.itemTypes.Where(item => item.HasAnyVariant()).ToList();
    }

    /// <summary>
    /// Get the item that will be used for a specific scan index
    /// </summary>
    public ItemVariants GetItemForScan(int scanIndex)
    {
        if (selectedItemsForSession == null || scanIndex < 0 || scanIndex >= selectedItemsForSession.Count)
            return null;
        return selectedItemsForSession[scanIndex];
    }

    /// <summary>
    /// Check if the NPC is currently scanning a specific item type
    /// </summary>
    public bool IsCurrentlyScanningItemType(ItemVariants itemType)
    {
        if (!isScanning || itemType == null) return false;

        int currentIndex = scanIndex - 1; // scanIndex is incremented after each scan
        if (currentIndex < 0 || selectedItemsForSession == null || currentIndex >= selectedItemsForSession.Count)
            return false;

        return selectedItemsForSession[currentIndex] == itemType;
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Debug Current Setup")]
    private void DebugCurrentSetup()
    {
        Debug.Log($"=== {name} DEBUG INFO ===");
        Debug.Log($"Current State: {currentState}");
        Debug.Log($"Has Reserved Cashier: {hasReservedCashier}");
        Debug.Log($"Chosen Cashier Zone: {(chosenCashierZone ? chosenCashierZone.name : "NULL")}");
        Debug.Log($"Active Item Collection: {(activeItemCollection != null ? "Valid" : "NULL")}");
        Debug.Log($"Active UI Elements: {(activeUIElements != null ? "Valid" : "NULL")}");

        if (activeItemCollection != null)
        {
            Debug.Log($"Item Collection Types: {activeItemCollection.itemTypes.Count}");
        }

        if (activeUIElements != null)
        {
            activeUIElements.ValidateUIElements();
        }

        Debug.Log($"Items To Scan: {itemsToScan}");
        Debug.Log($"Scan Index: {scanIndex}");
        Debug.Log($"Scanned Item Count: {scannedItemCount}");
        Debug.Log($"Is Scanning: {isScanning}");
    }

    [ContextMenu("Test UI Updates")]
    private void TestUIUpdates()
    {
        if (activeUIElements == null)
        {
            Debug.LogError("No active UI elements!");
            return;
        }

        Debug.Log("Testing UI updates...");
        activeUIElements.DisableAllUI();

        StartCoroutine(TestUISequence());
    }

    private IEnumerator TestUISequence()
    {
        for (int i = 0; i < 3; i++)
        {
            Debug.Log($"Testing UI item {i}");
            activeUIElements.EnableUIForItem(i);
            activeUIElements.UpdateScannedCount(i + 1);
            yield return new WaitForSeconds(1f);
        }
    }

    [ContextMenu("Validate UI Setup")]
    private void ValidateUISetup()
    {
        Debug.Log($"=== {name} UI VALIDATION ===");

        if (activeUIElements == null)
        {
            Debug.LogError($"❌ {name}: activeUIElements is NULL!");

            // Try to find it from cashier baskets
            if (chosenCashierZone != null)
            {
                foreach (var pair in cashierBaskets)
                {
                    if (pair.zone == chosenCashierZone)
                    {
                        activeUIElements = pair.uiElements;
                        Debug.Log($"🔄 {name}: Found UI elements from cashier basket pair");
                        break;
                    }
                }
            }
        }

        if (activeUIElements != null)
        {
            Debug.Log($"✅ {name}: activeUIElements found, validating...");
            activeUIElements.ValidateUIElements();
        }
        else
        {
            Debug.LogError($"❌ {name}: Still no active UI elements after search!");
        }
    }

    #endregion
}