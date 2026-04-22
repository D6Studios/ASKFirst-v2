using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraStateManager : MonoBehaviour
{
    public static CameraStateManager Instance { get; private set; }

    [Header("Camera Settings")]
    public MonoBehaviour playerCameraController;
    public bool debugCameraStates = true;

    private bool originalCameraEnabled;
    private CursorLockMode originalCursorLockMode;
    private bool originalCursorVisible;
    private bool cameraStateModified = false;
    private GameObject currentInteractingNPC = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Store original states
            if (playerCameraController != null)
            {
                originalCameraEnabled = playerCameraController.enabled;
            }
            originalCursorLockMode = Cursor.lockState;
            originalCursorVisible = Cursor.visible;

            if (debugCameraStates)
            {
                Debug.Log("📷 CameraStateManager: Initialized with original states stored");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool RequestCameraFreeze(GameObject requester)
    {
        if (currentInteractingNPC != null && currentInteractingNPC != requester)
        {
            if (debugCameraStates)
            {
                Debug.LogWarning($"📷 Camera freeze denied - {requester.name} requested but {currentInteractingNPC.name} is already using camera");
            }
            return false;
        }

        currentInteractingNPC = requester;

        if (!cameraStateModified)
        {
            if (playerCameraController != null)
            {
                playerCameraController.enabled = false;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            cameraStateModified = true;

            if (debugCameraStates)
            {
                Debug.Log($"📷 CameraStateManager: Camera frozen for {requester.name}");
            }
        }

        return true;
    }

    public void ReleaseCameraFreeze(GameObject requester)
    {
        if (currentInteractingNPC != requester)
        {
            if (debugCameraStates)
            {
                Debug.LogWarning($"📷 Camera release denied - {requester.name} tried to release but {currentInteractingNPC?.name} owns the camera");
            }
            return;
        }

        RestoreOriginalCameraState();
        currentInteractingNPC = null;

        if (debugCameraStates)
        {
            Debug.Log($"📷 CameraStateManager: Camera released by {requester.name}");
        }
    }

    public void ForceRestoreCamera(string reason = "Force restore")
    {
        if (debugCameraStates)
        {
            Debug.Log($"📷 CameraStateManager: FORCE RESTORE - Reason: {reason}");
        }

        RestoreOriginalCameraState();
        currentInteractingNPC = null;
    }

    private void RestoreOriginalCameraState()
    {
        if (cameraStateModified)
        {
            if (playerCameraController != null)
            {
                playerCameraController.enabled = originalCameraEnabled;
            }

            Cursor.lockState = originalCursorLockMode;
            Cursor.visible = originalCursorVisible;

            cameraStateModified = false;

            if (debugCameraStates)
            {
                Debug.Log("📷 CameraStateManager: Original camera state restored");
            }
        }
    }

    // Emergency restore method you can call from console or inspector
    [ContextMenu("Emergency Restore Camera")]
    public void EmergencyRestoreCamera()
    {
        ForceRestoreCamera("Emergency restore triggered");
    }

    // Debug info
    public void LogCurrentState()
    {
        Debug.Log($"📷 === CAMERA STATE DEBUG ===");
        Debug.Log($"Current Interacting NPC: {(currentInteractingNPC ? currentInteractingNPC.name : "None")}");
        Debug.Log($"Camera State Modified: {cameraStateModified}");
        Debug.Log($"Camera Controller Enabled: {(playerCameraController ? playerCameraController.enabled.ToString() : "NULL")}");
        Debug.Log($"Cursor Lock State: {Cursor.lockState}");
        Debug.Log($"Cursor Visible: {Cursor.visible}");
    }
}