
using UnityEngine;

public class PlayerInteractionSetup : MonoBehaviour
{
    [Header("Interaction Settings")]
    public Camera playerCamera;
    public Transform raycastPoint;
    public float interactionRange = 5f;
    public KeyCode interactionKey = KeyCode.E;

    private void Start()
    {
        // Auto-find camera if not assigned
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
        }

        // Auto-find or create raycast point if not assigned
        if (raycastPoint == null)
        {
            // Look for existing raycast point as child of camera
            Transform existingPoint = playerCamera.transform.Find("RaycastPoint");
            if (existingPoint != null)
            {
                raycastPoint = existingPoint;
                Debug.Log("Found existing RaycastPoint");
            }
            else
            {
                // Create new raycast point as child of camera
                GameObject raycastObj = new GameObject("RaycastPoint");
                raycastObj.transform.SetParent(playerCamera.transform);
                raycastObj.transform.localPosition = Vector3.zero;
                raycastPoint = raycastObj.transform;
                Debug.Log("Created new RaycastPoint as child of camera");
            }
        }

        // Validate that raycast point is child of camera
        if (raycastPoint.parent != playerCamera.transform)
        {
            Debug.LogWarning("RaycastPoint is not a child of the player camera. Consider making it a child for proper positioning.");
        }

        // Set up the global interaction system with raycast point
        RaycastNPCInteraction.SetPlayerReference(playerCamera, raycastPoint, interactionRange, interactionKey);
        Debug.Log($"Player interaction system initialized with range: {interactionRange}, key: {interactionKey}, raycast point: {raycastPoint.name}");
    }
}