using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;




public class GameManager : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Total game time in seconds.")]
    public float totalTime = 120f;
    public TextMeshProUGUI timerText;

    [Header("Shoplifter Settings")]
    public List<GameObject> shoplifterPrefabs; // Changed to List for multiple variants
    public Transform[] spawnPoints;
    public float spawnIntervalMin = 5f;
    public float spawnIntervalMax = 10f;

    [Header("Zone References")]
    public EscapeZone triggerZone;
    public DeterZone deterZone;

    [Header("Shoplifter Counters")]
    public int shoplifterEscaped = 0;
    public int shopliftersDeterred = 0;

    [Header("UI Elements")]
    public TextMeshProUGUI askPointsText; // New UI element for displaying ASK Points
    public GameObject feedbackUI; // UI GameObject for counter feedback animation
    public CanvasGroup feedbackCanvasGroup; // Canvas Group for fade animation
    public TextMeshProUGUI feedbackText; // Text component in the feedback UI

    [Header("Animation Settings")]
    public float animationDuration = 0.5f;
    public float moveUpDistance = 30f; // How much the feedback UI moves up

    [Header("Win Screen UI")]
    public GameObject winPanel;
    public TextMeshProUGUI winTxtAskPoints;
    public TextMeshProUGUI winTxtStatus;

    [Header("Lose Screen UI")]
    public GameObject losePanel;
    public TextMeshProUGUI loseTxtAskPoints;
    public TextMeshProUGUI loseTxtStatus;

    [Header("Camera Control")]
    [Tooltip("Reference to the main camera or camera controller to freeze when game ends.")]
    public MonoBehaviour cameraController; // Generic reference to any camera controller script

    private float currentTime;
    private float nextSpawnTime;
    private bool gameEnded = false;
    private Vector3 feedbackOriginalPosition;
    private bool originalCameraControllerState;
    private CursorLockMode originalCursorLockMode;
    private bool originalCursorVisible;

    private void Start()
    {
        currentTime = totalTime;
        ScheduleNextSpawn();
        ResetGame();

        if (triggerZone != null)
            triggerZone.SetGameManager(this);
        if (deterZone != null)
            deterZone.SetGameManager(this);

        // Ensure end game panels are disabled at start
        if (winPanel != null)
            winPanel.SetActive(false);
        if (losePanel != null)
            losePanel.SetActive(false);

        // Setup feedback UI
        SetupFeedbackUI();

        // Update ASK Points display
        UpdateAskPointsUI();

        // Ensure camera is enabled at start
        if (cameraController != null)
            cameraController.enabled = true;
    }

    private void Update()
    {
        if (!gameEnded)
        {
            HandleTimer();
            HandleSpawning();
        }
    }

    /// <summary>
    /// Freezes camera movement and unlocks cursor for UI interaction.
    /// </summary>
    private void FreezeCameraAndUnlockCursor()
    {
        // Store current states before changing them
        originalCursorLockMode = Cursor.lockState;
        originalCursorVisible = Cursor.visible;
        if (cameraController != null)
        {
            originalCameraControllerState = cameraController.enabled;
        }

        // Unlock and show cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable camera controller to freeze camera movement
        if (cameraController != null)
        {
            cameraController.enabled = false;
        }
    }

    /// <summary>
    /// Restores camera movement and cursor to gameplay states.
    /// </summary>
    private void RestoreCameraAndCursor()
    {
        // Restore cursor state (or set to typical gameplay state)
        Cursor.lockState = originalCursorLockMode;
        Cursor.visible = originalCursorVisible;

        // Re-enable camera controller
        if (cameraController != null)
        {
            cameraController.enabled = true; // Always enable for gameplay
        }
    }

    /// <summary>
    /// Sets up the feedback UI for animations.
    /// </summary>
    private void SetupFeedbackUI()
    {
        if (feedbackUI != null)
        {
            // Store original position
            feedbackOriginalPosition = feedbackUI.transform.localPosition;

            // Ensure the feedback UI starts invisible
            if (feedbackCanvasGroup != null)
            {
                feedbackCanvasGroup.alpha = 0f;
            }
            else
            {
                feedbackUI.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Updates the ASK Points UI display.
    /// </summary>
    private void UpdateAskPointsUI()
    {
        if (askPointsText != null)
        {
            askPointsText.text = $"ASK Points: {shopliftersDeterred}";
        }
    }

    /// <summary>
    /// Plays the counter feedback animation.
    /// </summary>
    private void PlayCounterFeedback()
    {
        if (feedbackUI != null)
        {
            StartCoroutine(CounterFeedbackAnimation());
        }
    }

    /// <summary>
    /// Coroutine that handles the fade in/out and move up animation.
    /// </summary>
    private IEnumerator CounterFeedbackAnimation()
    {
        // Reset position and make visible
        feedbackUI.transform.localPosition = feedbackOriginalPosition;

        if (feedbackCanvasGroup != null)
        {
            feedbackCanvasGroup.alpha = 1f;
        }
        else
        {
            feedbackUI.SetActive(true);
        }

        // Update feedback text
        if (feedbackText != null)
        {
            feedbackText.text = "+1";
        }

        float elapsedTime = 0f;
        Vector3 startPosition = feedbackOriginalPosition;
        Vector3 endPosition = startPosition + Vector3.up * moveUpDistance;

        // Animation loop
        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;

            // Move up
            feedbackUI.transform.localPosition = Vector3.Lerp(startPosition, endPosition, t);

            // Fade out (fade in for first 20%, then fade out)
            if (feedbackCanvasGroup != null)
            {
                if (t < 0.2f)
                {
                    // Fade in quickly
                    feedbackCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.2f);
                }
                else
                {
                    // Fade out
                    feedbackCanvasGroup.alpha = Mathf.Lerp(1f, 0f, (t - 0.2f) / 0.8f);
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it's fully invisible at the end
        if (feedbackCanvasGroup != null)
        {
            feedbackCanvasGroup.alpha = 0f;
        }
        else
        {
            feedbackUI.SetActive(false);
        }

        // Reset position
        feedbackUI.transform.localPosition = feedbackOriginalPosition;
    }

    /// <summary>
    /// Updates the countdown timer.
    /// </summary>
    private void HandleTimer()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;

            // Clamp to 0 so it never goes negative
            if (currentTime <= 0)
            {
                currentTime = 0;
                Debug.Log("Time's up!");
                EndGame();
            }

            UpdateTimerUI();
        }
    }

    private void ResetGame()
    {
        currentTime = totalTime;
        gameEnded = false;
        shoplifterEscaped = 0;
        shopliftersDeterred = 0;
        totalTime = 300;
        ScheduleNextSpawn();
        UpdateTimerUI();
        UpdateAskPointsUI();

        if (triggerZone != null)
            triggerZone.SetGameManager(this);
        if (deterZone != null)
            deterZone.SetGameManager(this);

        if (winPanel != null)
            winPanel.SetActive(false);
        if (losePanel != null)
            losePanel.SetActive(false);

        // Restore camera and cursor when resetting game
        RestoreCameraAndCursor();
    }

    /// <summary>
    /// Updates the timer display in MM:SS format.
    /// </summary>
    private void UpdateTimerUI()
    {
        float clampedTime = Mathf.Max(currentTime, 0); // ensures no negatives
        int minutes = Mathf.FloorToInt(clampedTime / 60f);
        int seconds = Mathf.FloorToInt(clampedTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    /// <summary>
    /// Handles spawning of shoplifters during the countdown.
    /// </summary>
    private void HandleSpawning()
    {
        if (currentTime > 0 && Time.time >= nextSpawnTime)
        {
            SpawnShoplifter();
            ScheduleNextSpawn();
        }
    }

    /// <summary>
    /// Schedules the next shoplifter spawn at a random interval.
    /// </summary>
    private void ScheduleNextSpawn()
    {
        float delay = Random.Range(spawnIntervalMin, spawnIntervalMax);
        nextSpawnTime = Time.time + delay;
    }

    /// <summary>
    /// Instantiates a shoplifter at a random spawn point using a random prefab variant.
    /// </summary>
    private void SpawnShoplifter()
    {
        if (shoplifterPrefabs.Count > 0 && spawnPoints.Length > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject selectedPrefab = shoplifterPrefabs[Random.Range(0, shoplifterPrefabs.Count)];
            Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }

    /// <summary>
    /// Increases the shoplifter escape count by 1.
    /// </summary>
    public void ShoplifterEscaped()
    {
        shoplifterEscaped++;
        Debug.Log("Shoplifter escaped! Total: " + shoplifterEscaped);
    }

    /// <summary>
    /// Increases the shoplifter deterred count by 1 and triggers feedback animation.
    /// </summary>
    public void ShoplifterDeterred()
    {
        shopliftersDeterred++;
        Debug.Log("Shoplifter deterred! Total: " + shopliftersDeterred);

        // Update UI and play feedback animation
        UpdateAskPointsUI();
        PlayCounterFeedback();
    }

    /// <summary>
    /// Ends the game and shows appropriate win/lose screen.
    /// </summary>
    private void EndGame()
    {
        gameEnded = true;

        // Freeze camera and unlock cursor when game ends
        FreezeCameraAndUnlockCursor();

        int askPoints = shopliftersDeterred;
        bool passed = askPoints >= 3;
        string status = passed ? "PASS" : "FAIL";

        if (passed)
        {
            ShowWinScreen(askPoints, status);
        }
        else
        {
            ShowLoseScreen(askPoints, status);
        }
    }

    /// <summary>
    /// Shows the win screen with player stats.
    /// </summary>
    private void ShowWinScreen(int askPoints, string status)
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            winTxtAskPoints.text = $"ASK Points: {askPoints}";
            winTxtStatus.text = $"Status: {status}";
        }
    }

    /// <summary>
    /// Shows the lose screen with player stats.
    /// </summary>
    private void ShowLoseScreen(int askPoints, string status)
    {
        if (losePanel != null)
        {
            losePanel.SetActive(true);
            loseTxtAskPoints.text = $"ASK Points: {askPoints}";
            loseTxtStatus.text = $"Status: {status}";
        }
    }

    /// <summary>
    /// Public method to restart the game (can be called from UI buttons).
    /// </summary>
    public void RestartGame()
    {
        ResetGame();
    }
}