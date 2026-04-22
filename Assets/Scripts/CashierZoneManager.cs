
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Centralized manager for handling cashier zone reservations
/// Prevents NPCs from switching zones mid-checkout and handles conflicts
/// </summary>
public class CashierZoneManager : MonoBehaviour
{
    public static CashierZoneManager Instance { get; private set; }

    [Header("Zone Management")]
    [Tooltip("All cashier zones in the scene that can be reserved")]
    public List<CashierZone> allCashierZones = new List<CashierZone>();

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showReservationStatus = false;

    // Internal reservation tracking
    private Dictionary<CashierZone, GameObject> zoneReservations = new Dictionary<CashierZone, GameObject>();
    private Dictionary<GameObject, CashierZone> npcReservations = new Dictionary<GameObject, CashierZone>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            InitializeZones();
        }
        else
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"CashierZoneManager: Multiple instances found! Destroying {gameObject.name}");
            }
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Auto-find cashier zones if none assigned
        if (allCashierZones.Count == 0)
        {
            AutoFindCashierZones();
        }

        ValidateZones();
    }

    /// <summary>
    /// Initialize all zones as available
    /// </summary>
    private void InitializeZones()
    {
        foreach (var zone in allCashierZones)
        {
            if (zone != null)
            {
                zoneReservations[zone] = null;
            }
        }

        if (enableDebugLogs)
        {
            Debug.Log($"CashierZoneManager: Initialized {allCashierZones.Count} cashier zones");
        }
    }

    /// <summary>
    /// Auto-find all CashierZone components in the scene
    /// </summary>
    private void AutoFindCashierZones()
    {
        CashierZone[] foundZones = FindObjectsOfType<CashierZone>();
        allCashierZones.AddRange(foundZones);

        if (enableDebugLogs)
        {
            Debug.Log($"CashierZoneManager: Auto-found {foundZones.Length} cashier zones in scene");
        }

        // Re-initialize with found zones
        InitializeZones();
    }

    /// <summary>
    /// Validate that all assigned zones are valid
    /// </summary>
    private void ValidateZones()
    {
        for (int i = allCashierZones.Count - 1; i >= 0; i--)
        {
            if (allCashierZones[i] == null)
            {
                allCashierZones.RemoveAt(i);
                if (enableDebugLogs)
                {
                    Debug.LogWarning($"CashierZoneManager: Removed null zone reference at index {i}");
                }
            }
        }
    }

    /// <summary>
    /// Try to reserve any available cashier zone for the requesting NPC
    /// </summary>
    /// <param name="requester">The NPC requesting a cashier</param>
    /// <returns>Reserved CashierZone or null if none available</returns>
    public CashierZone TryReserveCashier(GameObject requester)
    {
        if (requester == null)
        {
            Debug.LogError("CashierZoneManager: Cannot reserve cashier for null requester");
            return null;
        }

        // Check if this NPC already has a reservation
        if (npcReservations.ContainsKey(requester))
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"CashierZoneManager: {requester.name} already has a cashier reserved: {npcReservations[requester].name}");
            }
            return npcReservations[requester];
        }

        // Find available zones
        var availableZones = allCashierZones.Where(zone =>
            zone != null &&
            (!zoneReservations.ContainsKey(zone) || zoneReservations[zone] == null) &&
            zone.IsAvailable).ToList();

        if (availableZones.Count > 0)
        {
            // Choose a random available zone
            var chosenZone = availableZones[Random.Range(0, availableZones.Count)];

            // Make the reservation
            zoneReservations[chosenZone] = requester;
            npcReservations[requester] = chosenZone;

            // Also reserve through the zone's own system
            chosenZone.Reserve(requester);

            if (enableDebugLogs)
            {
                Debug.Log($"🏪 CashierZoneManager: Reserved {chosenZone.name} for {requester.name}");
            }

            return chosenZone;
        }

        if (enableDebugLogs)
        {
            Debug.Log($"🏪 CashierZoneManager: No available cashiers for {requester.name} (Available: {GetAvailableCashierCount()})");
        }

        return null;
    }

    /// <summary>
    /// Release a cashier zone reservation
    /// </summary>
    /// <param name="zone">The zone to release</param>
    /// <param name="requester">The NPC releasing the zone</param>
    public void ReleaseCashier(CashierZone zone, GameObject requester)
    {
        if (zone == null)
        {
            Debug.LogError("CashierZoneManager: Cannot release null zone");
            return;
        }

        if (requester == null)
        {
            Debug.LogError("CashierZoneManager: Cannot release zone for null requester");
            return;
        }

        // Verify this NPC actually has this zone reserved
        if (zoneReservations.ContainsKey(zone) && zoneReservations[zone] == requester)
        {
            // Release from our tracking
            zoneReservations[zone] = null;
            npcReservations.Remove(requester);

            // Release from the zone's own system
            zone.Release();

            if (enableDebugLogs)
            {
                Debug.Log($"🏪 CashierZoneManager: Released {zone.name} from {requester.name}");
            }
        }
        else
        {
            if (enableDebugLogs)
            {
                GameObject currentOwner = zoneReservations.ContainsKey(zone) ? zoneReservations[zone] : null;
                string ownerName = currentOwner != null ? currentOwner.name : "None";
                Debug.LogWarning($"🏪 CashierZoneManager: {requester.name} tried to release {zone.name} but it's reserved by: {ownerName}");
            }
        }
    }

    /// <summary>
    /// Check if a specific NPC has any cashier reservation
    /// </summary>
    /// <param name="npc">The NPC to check</param>
    /// <returns>True if the NPC has a reservation</returns>
    public bool HasReservation(GameObject npc)
    {
        return npc != null && npcReservations.ContainsKey(npc);
    }

    /// <summary>
    /// Get the zone reserved by a specific NPC
    /// </summary>
    /// <param name="npc">The NPC to check</param>
    /// <returns>The reserved zone or null if none</returns>
    public CashierZone GetReservedZone(GameObject npc)
    {
        if (npc != null && npcReservations.ContainsKey(npc))
        {
            return npcReservations[npc];
        }
        return null;
    }

    /// <summary>
    /// Force release all reservations for an NPC (cleanup method)
    /// </summary>
    /// <param name="npc">The NPC to release all reservations for</param>
    public void ForceReleaseAllForNPC(GameObject npc)
    {
        if (npc == null) return;

        // Find all zones reserved by this NPC
        var zonesToRelease = zoneReservations.Where(kvp => kvp.Value == npc).ToList();

        foreach (var kvp in zonesToRelease)
        {
            CashierZone zone = kvp.Key;

            // Clear our tracking
            zoneReservations[zone] = null;
            zone.Release();

            if (enableDebugLogs)
            {
                Debug.Log($"🏪 CashierZoneManager: Force released {zone.name} from {npc.name}");
            }
        }

        // Remove from NPC tracking
        npcReservations.Remove(npc);
    }

    /// <summary>
    /// Get count of available (unreserved) cashiers
    /// </summary>
    /// <returns>Number of available cashiers</returns>
    public int GetAvailableCashierCount()
    {
        return allCashierZones.Count(zone =>
            zone != null &&
            (!zoneReservations.ContainsKey(zone) || zoneReservations[zone] == null) &&
            zone.IsAvailable);
    }

    /// <summary>
    /// Get count of reserved cashiers
    /// </summary>
    /// <returns>Number of reserved cashiers</returns>
    public int GetReservedCashierCount()
    {
        return zoneReservations.Count(kvp => kvp.Value != null);
    }

    /// <summary>
    /// Get list of all NPCs with reservations
    /// </summary>
    /// <returns>List of NPCs that have cashier reservations</returns>
    public List<GameObject> GetNPCsWithReservations()
    {
        return npcReservations.Keys.ToList();
    }

    /// <summary>
    /// Debug method to print current reservation status
    /// </summary>
    [ContextMenu("Print Reservation Status")]
    public void PrintReservationStatus()
    {
        Debug.Log("=== CASHIER ZONE RESERVATION STATUS ===");
        Debug.Log($"Total Zones: {allCashierZones.Count}");
        Debug.Log($"Available: {GetAvailableCashierCount()}");
        Debug.Log($"Reserved: {GetReservedCashierCount()}");

        foreach (var kvp in zoneReservations)
        {
            if (kvp.Key != null)
            {
                string status = kvp.Value != null ? $"Reserved by {kvp.Value.name}" : "Available";
                Debug.Log($"  {kvp.Key.name}: {status}");
            }
        }
        Debug.Log("========================================");
    }

    /// <summary>
    /// Clean up any null references in our tracking
    /// </summary>
    public void CleanupNullReferences()
    {
        // Clean up null NPCs from reservations
        var nullNPCs = npcReservations.Where(kvp => kvp.Key == null).Select(kvp => kvp.Key).ToList();
        foreach (var nullNPC in nullNPCs)
        {
            npcReservations.Remove(nullNPC);
        }

        // Clean up zones with null NPCs
        var zonesToClean = zoneReservations.Where(kvp => kvp.Value == null || kvp.Key == null).ToList();
        foreach (var kvp in zonesToClean)
        {
            if (kvp.Key != null)
            {
                zoneReservations[kvp.Key] = null;
                kvp.Key.Release();
            }
        }

        if (enableDebugLogs && (nullNPCs.Count > 0 || zonesToClean.Count > 0))
        {
            Debug.Log($"CashierZoneManager: Cleaned up {nullNPCs.Count} null NPCs and {zonesToClean.Count} orphaned zones");
        }
    }

    private void Update()
    {
        // Optional: Show reservation status in inspector during play
        if (showReservationStatus && Application.isPlaying)
        {
            // This will update the inspector values for debugging
            // You can remove this if it causes performance issues
        }

        // Periodic cleanup (every 10 seconds)
        if (Time.time % 10f < Time.deltaTime)
        {
            CleanupNullReferences();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #region Editor Helpers

#if UNITY_EDITOR
    [Header("Editor Tools")]
    [SerializeField] private bool autoAssignZonesInEditor = true;
    
    private void OnValidate()
    {
        if (autoAssignZonesInEditor && Application.isPlaying == false)
        {
            // Auto-assign zones in editor if list is empty
            if (allCashierZones.Count == 0)
            {
                CashierZone[] foundZones = FindObjectsOfType<CashierZone>();
                allCashierZones.AddRange(foundZones);
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
    }
#endif

    #endregion
}