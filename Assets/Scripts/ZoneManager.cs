using UnityEngine;
using System.Collections.Generic;


public class ZoneManager : MonoBehaviour
{
    public static ZoneManager Instance { get; private set; }

    private Dictionary<BuyZone, GameObject> reservedZones = new Dictionary<BuyZone, GameObject>();

    // PERFORMANCE FIX: Track shoplifters instead of using FindObjectsOfType
    private HashSet<ShoplifterBehavior> activeShoplifters = new HashSet<ShoplifterBehavior>();
    private Dictionary<BuyZone, ShoplifterBehavior> shoplifterZones = new Dictionary<BuyZone, ShoplifterBehavior>();

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Shoplifter Management
    /// <summary>
    /// Register a shoplifter with the zone manager (call this when shoplifter spawns)
    /// </summary>
    public void RegisterShoplifter(ShoplifterBehavior shoplifter)
    {
        if (shoplifter != null && !activeShoplifters.Contains(shoplifter))
        {
            activeShoplifters.Add(shoplifter);
            Debug.Log($"ZoneManager: Registered shoplifter '{shoplifter.name}'");
        }
    }

    /// <summary>
    /// Unregister a shoplifter (call this when shoplifter is destroyed/disabled)
    /// </summary>
    public void UnregisterShoplifter(ShoplifterBehavior shoplifter)
    {
        if (shoplifter != null && activeShoplifters.Contains(shoplifter))
        {
            activeShoplifters.Remove(shoplifter);

            // Clean up any zone associations
            var zonesToRemove = new List<BuyZone>();
            foreach (var kvp in shoplifterZones)
            {
                if (kvp.Value == shoplifter)
                {
                    zonesToRemove.Add(kvp.Key);
                }
            }

            foreach (var zone in zonesToRemove)
            {
                shoplifterZones.Remove(zone);
            }

            Debug.Log($"ZoneManager: Unregistered shoplifter '{shoplifter.name}'");
        }
    }

    /// <summary>
    /// Notify that a shoplifter is now using a specific zone
    /// </summary>
    public void SetShoplifterZone(ShoplifterBehavior shoplifter, BuyZone zone)
    {
        if (shoplifter == null || zone == null) return;

        // Remove any previous zone association for this shoplifter
        var previousZones = new List<BuyZone>();
        foreach (var kvp in shoplifterZones)
        {
            if (kvp.Value == shoplifter)
            {
                previousZones.Add(kvp.Key);
            }
        }
        foreach (var prevZone in previousZones)
        {
            shoplifterZones.Remove(prevZone);
        }

        // Set new zone
        shoplifterZones[zone] = shoplifter;
        Debug.Log($"ZoneManager: Shoplifter '{shoplifter.name}' is now using zone '{zone.name}'");
    }

    /// <summary>
    /// Clear a shoplifter's zone usage
    /// </summary>
    public void ClearShoplifterZone(ShoplifterBehavior shoplifter, BuyZone zone)
    {
        if (shoplifter != null && zone != null &&
            shoplifterZones.ContainsKey(zone) && shoplifterZones[zone] == shoplifter)
        {
            shoplifterZones.Remove(zone);
            Debug.Log($"ZoneManager: Shoplifter '{shoplifter.name}' cleared from zone '{zone.name}'");
        }
    }
    #endregion

    #region Zone Reservation (Regular Shoppers)
    /// <summary>
    /// Try to reserve a zone for a specific shopper
    /// </summary>
    /// <param name="zone">The zone to reserve</param>
    /// <param name="shopper">The shopper requesting the zone</param>
    /// <returns>True if reservation was successful, false if zone is already reserved or occupied</returns>
    public bool TryReserveZone(BuyZone zone, GameObject shopper)
    {
        if (zone == null || shopper == null)
        {
            Debug.LogWarning("ZoneManager: Tried to reserve with null zone or shopper");
            return false;
        }

        // Check if zone is already reserved, occupied, or used by shoplifter
        if (reservedZones.ContainsKey(zone) || zone.IsOccupied || IsZoneOccupiedByShoplifter(zone))
        {
            return false;
        }

        // Reserve the zone
        reservedZones[zone] = shopper;
        Debug.Log($"ZoneManager: Zone '{zone.name}' reserved by {shopper.name}");
        return true;
    }

    /// <summary>
    /// Release a zone reservation
    /// </summary>
    /// <param name="zone">The zone to release</param>
    /// <param name="shopper">The shopper releasing the zone</param>
    public void ReleaseZone(BuyZone zone, GameObject shopper)
    {
        if (zone == null || shopper == null)
        {
            Debug.LogWarning("ZoneManager: Tried to release with null zone or shopper");
            return;
        }

        // Only release if this shopper actually reserved it
        if (reservedZones.ContainsKey(zone) && reservedZones[zone] == shopper)
        {
            reservedZones.Remove(zone);
            Debug.Log($"ZoneManager: Zone '{zone.name}' released by {shopper.name}");
        }
    }

    /// <summary>
    /// Check if a zone is reserved (but not necessarily occupied yet)
    /// </summary>
    /// <param name="zone">The zone to check</param>
    /// <returns>True if zone is reserved</returns>
    public bool IsZoneReserved(BuyZone zone)
    {
        return zone != null && reservedZones.ContainsKey(zone);
    }

    /// <summary>
    /// Check if a zone is reserved by a specific shopper
    /// </summary>
    /// <param name="zone">The zone to check</param>
    /// <param name="shopper">The shopper to check for</param>
    /// <returns>True if zone is reserved by this specific shopper</returns>
    public bool IsZoneReservedBy(BuyZone zone, GameObject shopper)
    {
        return zone != null && shopper != null &&
               reservedZones.ContainsKey(zone) &&
               reservedZones[zone] == shopper;
    }

    /// <summary>
    /// Get who has reserved a specific zone
    /// </summary>
    /// <param name="zone">The zone to check</param>
    /// <returns>The GameObject that reserved the zone, or null if not reserved</returns>
    public GameObject GetZoneReserver(BuyZone zone)
    {
        if (zone != null && reservedZones.ContainsKey(zone))
        {
            return reservedZones[zone];
        }
        return null;
    }
    #endregion

    #region Zone Availability
    /// <summary>
    /// Get all available zones (not reserved, not occupied, not used by shoplifters)
    /// </summary>
    /// <param name="allZones">List of all zones to check</param>
    /// <param name="excludeZone">Zone to exclude (like last zone visited)</param>
    /// <returns>List of available zones</returns>
    public List<BuyZone> GetAvailableZones(List<BuyZone> allZones, BuyZone excludeZone = null)
    {
        List<BuyZone> availableZones = new List<BuyZone>();

        foreach (BuyZone zone in allZones)
        {
            if (zone == null) continue;
            if (zone == excludeZone) continue;
            if (IsZoneReserved(zone)) continue;
            if (zone.IsOccupied) continue;
            if (IsZoneOccupiedByShoplifter(zone)) continue;

            availableZones.Add(zone);
        }

        return availableZones;
    }

    /// <summary>
    /// OPTIMIZED: Check if a zone is being used by a shoplifter using cached data
    /// </summary>
    private bool IsZoneOccupiedByShoplifter(BuyZone zone)
    {
        // PERFORMANCE FIX: Use cached lookup instead of FindObjectsOfType
        return shoplifterZones.ContainsKey(zone);
    }
    #endregion

    #region Debug and Utility Methods
    /// <summary>
    /// Debug method to see current reservations
    /// </summary>
    public void LogCurrentReservations()
    {
        Debug.Log($"ZoneManager: Current reservations ({reservedZones.Count}):");
        foreach (var kvp in reservedZones)
        {
            Debug.Log($"  - Zone '{kvp.Key.name}' reserved by '{kvp.Value.name}'");
        }

        Debug.Log($"ZoneManager: Shoplifter zones ({shoplifterZones.Count}):");
        foreach (var kvp in shoplifterZones)
        {
            Debug.Log($"  - Zone '{kvp.Key.name}' used by shoplifter '{kvp.Value.name}'");
        }

        Debug.Log($"ZoneManager: Active shoplifters ({activeShoplifters.Count})");
    }

    /// <summary>
    /// Force release all reservations (useful for cleanup or debugging)
    /// </summary>
    public void ClearAllReservations()
    {
        int count = reservedZones.Count;
        reservedZones.Clear();
        Debug.Log($"ZoneManager: Cleared {count} reservations");
    }

    /// <summary>
    /// Clean up any null references (call periodically or when objects are destroyed)
    /// </summary>
    public void CleanupNullReferences()
    {
        // Clean up null shoplifters
        activeShoplifters.RemoveWhere(s => s == null);

        // Clean up null shoplifter zones
        var zonesToRemove = new List<BuyZone>();
        foreach (var kvp in shoplifterZones)
        {
            if (kvp.Value == null)
            {
                zonesToRemove.Add(kvp.Key);
            }
        }
        foreach (var zone in zonesToRemove)
        {
            shoplifterZones.Remove(zone);
        }

        // Clean up null reserved zones
        var reservationsToRemove = new List<BuyZone>();
        foreach (var kvp in reservedZones)
        {
            if (kvp.Value == null)
            {
                reservationsToRemove.Add(kvp.Key);
            }
        }
        foreach (var zone in reservationsToRemove)
        {
            reservedZones.Remove(zone);
        }
    }
    #endregion

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}