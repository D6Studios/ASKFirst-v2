using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PauseManager : MonoBehaviour
{
    [Header("Pause Settings")]
    public KeyCode pauseKey = KeyCode.Escape;
    public GameObject pauseMenuUI;

    [Header("Player References")]
    public FirstPersonController playerController;

    [Header("UI Controls")]
    public Slider fovSlider;
    public Slider sensitivitySlider;

    [Header("Slider Settings")]
    public float minFOV = 30f;
    public float maxFOV = 120f;
    public float minSensitivity = 0.1f;
    public float maxSensitivity = 10f;

    // Internal variables
    private bool isPaused = false;
    private bool originalCursorLockState;
    private bool originalCursorVisible;
    private float originalTimeScale;

    void Start()
    {
        // Initialize pause menu as inactive
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        // Store original time scale
        originalTimeScale = Time.timeScale;

        // Setup sliders if they exist
        SetupSliders();

        // Find FirstPersonController if not assigned
        if (playerController == null)
            playerController = FindObjectOfType<FirstPersonController>();
    }

    void Update()
    {
        // Check for pause key input
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    void SetupSliders()
    {
        if (playerController == null) return;

        // Setup FOV slider
        if (fovSlider != null)
        {
            fovSlider.minValue = minFOV;
            fovSlider.maxValue = maxFOV;
            fovSlider.value = playerController.fov;
            fovSlider.onValueChanged.AddListener(OnFOVChanged);
        }

        // Setup sensitivity slider
        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = minSensitivity;
            sensitivitySlider.maxValue = maxSensitivity;
            sensitivitySlider.value = playerController.mouseSensitivity;
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;

        // Store original cursor state
        originalCursorLockState = (Cursor.lockState == CursorLockMode.Locked);
        originalCursorVisible = Cursor.visible;

        // Unlock and show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Freeze time
        Time.timeScale = 0f;

        // Disable player camera movement
        if (playerController != null)
        {
            playerController.cameraCanMove = false;
            playerController.playerCanMove = false;
        }

        // Show pause menu
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        // Update slider values to current settings
        UpdateSliderValues();
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;

        // Restore original cursor state
        Cursor.lockState = originalCursorLockState ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = originalCursorVisible;

        // Unfreeze time
        Time.timeScale = originalTimeScale;

        // Enable player camera movement
        if (playerController != null)
        {
            playerController.cameraCanMove = true;
            playerController.playerCanMove = true;
        }

        // Hide pause menu
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

    void UpdateSliderValues()
    {
        if (playerController == null) return;

        if (fovSlider != null)
            fovSlider.value = playerController.fov;

        if (sensitivitySlider != null)
            sensitivitySlider.value = playerController.mouseSensitivity;
    }

    // Slider event handlers
    public void OnFOVChanged(float value)
    {
        if (playerController != null)
        {
            playerController.fov = value;
            // Also update the camera FOV immediately
            if (playerController.playerCamera != null)
                playerController.playerCamera.fieldOfView = value;
        }
    }

    public void OnSensitivityChanged(float value)
    {
        if (playerController != null)
        {
            playerController.mouseSensitivity = value;
        }
    }

    // Public methods for UI buttons
    public void OnResumeButtonClick()
    {
        ResumeGame();
    }

    public void OnQuitButtonClick()
    {
        // For in-editor testing
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Reset to default values
    public void ResetToDefaults()
    {
        if (fovSlider != null)
        {
            fovSlider.value = 60f; // Default FOV
            OnFOVChanged(60f);
        }

        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = 2f; // Default sensitivity
            OnSensitivityChanged(2f);
        }
    }

    // Getter for pause state (useful for other scripts)
    public bool IsPaused()
    {
        return isPaused;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // Optional: Auto-pause when window loses focus
        if (!hasFocus && !isPaused)
        {
            // PauseGame(); // Uncomment if you want auto-pause on focus loss
        }
    }
}