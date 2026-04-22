
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class GameManagerCashier : MonoBehaviour
{
    [Header("Tracking Stats")]
    public int shopliftersDeterred = 0;
    public int shoppersFalselyAccused = 0;

    [Header("Timer Settings")]
    public float gameDuration = 300f;
    public TextMeshProUGUI timerText;

    [Header("Spawning Settings")]
    public List<GameObject> scannerNPCPrefabs; // 🆕 List of NPC types
    public Transform[] spawnPoints;
    public float spawnInterval = 5f;
    public int maxNPCs = 5;

    [Header("UI Elements")]
    public TextMeshProUGUI askPointsText; // New UI element for displaying ASK Points
    public GameObject feedbackUI; // UI GameObject for counter feedback animation
    public CanvasGroup feedbackCanvasGroup; // Canvas Group for fade animation
    public TextMeshProUGUI feedbackText; // Text component in the feedback UI

    [Header("Animation Settings")]
    public float animationDuration = 0.5f;
    public float moveUpDistance = 30f; // How much the feedback UI moves up
    public Color positiveColor = Color.green; // Color for +1 (deterred)
    public Color negativeColor = Color.red; // Color for -1 (falsely accused)

    [Header("Win Screen UI")]
    public GameObject winPanel;
    public TextMeshProUGUI winTxtDeterred;
    public TextMeshProUGUI winTxtFalselyAccused;
    public TextMeshProUGUI winTxtAskPoints;
    public TextMeshProUGUI winTxtStatus;

    [Header("Lose Screen UI")]
    public GameObject losePanel;
    public TextMeshProUGUI loseTxtDeterred;
    public TextMeshProUGUI loseTxtFalselyAccused;
    public TextMeshProUGUI loseTxtAskPoints;
    public TextMeshProUGUI loseTxtStatus;

    [Header("Camera Control")]
    [Tooltip("Reference to the main camera or camera controller to freeze when game ends.")]
    public MonoBehaviour cameraController; // Generic reference to any camera controller script

    private float remainingTime;
    private float spawnTimer;
    private bool gameEnded = false;
    private List<GameObject> activeNPCs = new List<GameObject>();
    private Vector3 feedbackOriginalPosition;
    private CursorLockMode originalCursorLockMode;
    private bool originalCursorVisible;

    void Start()
    {
        remainingTime = gameDuration;
        spawnTimer = spawnInterval;

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

    void Update()
    {
        if (!gameEnded)
        {
            HandleTimer();
            HandleSpawning();
            CleanupNPCList();
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
            int askPoints = shopliftersDeterred - shoppersFalselyAccused;
            askPoints = Mathf.Max(0, askPoints); // Clamp to 0, never negative
            askPointsText.text = $"ASK Points: {askPoints}";
            Debug.Log($"Updated ASK Points UI to: {askPoints}");
        }
        else
        {
            Debug.LogWarning("askPointsText is null! Make sure to assign it in the inspector.");
        }
    }

    /// <summary>
    /// Plays the counter feedback animation for positive actions (shoplifter deterred).
    /// </summary>
    private void PlayPositiveFeedback()
    {
        Debug.Log("PlayPositiveFeedback() called");
        if (feedbackUI != null)
        {
            Debug.Log("feedbackUI is not null, starting coroutine");
            StartCoroutine(CounterFeedbackAnimation("+1", positiveColor));
        }
        else
        {
            Debug.LogWarning("feedbackUI is null! Make sure to assign it in the inspector.");
        }
    }

    /// <summary>
    /// Plays the counter feedback animation for negative actions (falsely accused).
    /// </summary>
    private void PlayNegativeFeedback()
    {
        Debug.Log("PlayNegativeFeedback() called");
        if (feedbackUI != null)
        {
            Debug.Log("feedbackUI is not null, starting coroutine");
            StartCoroutine(CounterFeedbackAnimation("-1", negativeColor));
        }
        else
        {
            Debug.LogWarning("feedbackUI is null! Make sure to assign it in the inspector.");
        }
    }

    /// <summary>
    /// Coroutine that handles the fade in/out and move up animation.
    /// </summary>
    private IEnumerator CounterFeedbackAnimation(string text, Color color)
    {
        Debug.Log($"CounterFeedbackAnimation started with text: {text}");

        // Reset position and make visible
        feedbackUI.transform.localPosition = feedbackOriginalPosition;
        Debug.Log($"Reset position to: {feedbackOriginalPosition}");

        if (feedbackCanvasGroup != null)
        {
            feedbackCanvasGroup.alpha = 1f;
            Debug.Log("Set CanvasGroup alpha to 1");
        }
        else
        {
            feedbackUI.SetActive(true);
            Debug.Log("Set feedbackUI active to true (no CanvasGroup)");
        }

        // Update feedback text and color
        if (feedbackText != null)
        {
            feedbackText.text = text;
            feedbackText.color = color;
            Debug.Log($"Set feedback text to: {text} with color: {color}");
        }
        else
        {
            Debug.LogWarning("feedbackText is null! Make sure to assign it in the inspector.");
        }

        float elapsedTime = 0f;
        Vector3 startPosition = feedbackOriginalPosition;
        Vector3 endPosition = startPosition + Vector3.up * moveUpDistance;
        Debug.Log($"Animation will move from {startPosition} to {endPosition}");

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

        Debug.Log("Animation completed");

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
        Debug.Log("Animation finished and UI reset");
    }

    /// <summary>
    /// Call this when a shoplifter is successfully deterred.
    /// </summary>
    public void ShoplifterDeterred()
    {
        shopliftersDeterred++;
        Debug.Log("ShoplifterDeterred() called! Total: " + shopliftersDeterred);

        // Update UI and play positive feedback animation
        UpdateAskPointsUI();
        Debug.Log("About to play positive feedback animation");
        PlayPositiveFeedback();
    }

    /// <summary>
    /// Call this when a shopper is falsely accused.
    /// </summary>
    public void ShopperFalselyAccused()
    {
        shoppersFalselyAccused++;
        Debug.Log("ShopperFalselyAccused() called! Total: " + shoppersFalselyAccused);

        // Update UI and play negative feedback animation
        UpdateAskPointsUI();
        Debug.Log("About to play negative feedback animation");
        PlayNegativeFeedback();
    }

    void HandleTimer()
    {
        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0)
            {
                remainingTime = 0; // Clamp to 0
                timerText.text = "00:00";
                if (!gameEnded)
                    EndGame();
            }
            else
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
        }
    }

    void HandleSpawning()
    {
        if (remainingTime <= 0) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f && activeNPCs.Count < maxNPCs)
        {
            SpawnScannerNPC();
            spawnTimer = spawnInterval;
        }
    }

    void SpawnScannerNPC()
    {
        // Guard: no prefabs or spawn points
        if (scannerNPCPrefabs == null || scannerNPCPrefabs.Count == 0)
        {
            Debug.LogWarning("SpawnScannerNPC aborted: No scanner NPC prefabs assigned.");
            return;
        }
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("SpawnScannerNPC aborted: No spawn points assigned.");
            return;
        }

        // Pick a spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Filter prefabs list for valid (not destroyed) entries
        List<GameObject> validPrefabs = scannerNPCPrefabs.FindAll(p => p != null);
        if (validPrefabs.Count == 0)
        {
            Debug.LogWarning("SpawnScannerNPC aborted: All prefabs are null or destroyed.");
            return;
        }

        // Pick a prefab safely
        GameObject selectedPrefab = validPrefabs[Random.Range(0, validPrefabs.Count)];

        if (selectedPrefab == null)
        {
            Debug.LogWarning("SpawnScannerNPC aborted: Selected prefab is null.");
            return;
        }

        // Instantiate safely
        GameObject npc = null;
        try
        {
            npc = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"SpawnScannerNPC failed to instantiate prefab '{selectedPrefab.name}': {ex.Message}");
            return;
        }

        // Attach GameManager reference
        ScannerNPC scanner = npc.GetComponent<ScannerNPC>();
        if (scanner != null)
        {
            scanner.gameManager = this;
        }
        else
        {
            Debug.LogWarning($"Spawned NPC '{npc.name}' has no ScannerNPC component.");
        }

        activeNPCs.Add(npc);
    }

    void CleanupNPCList()
    {
        activeNPCs.RemoveAll(npc => npc == null);
    }

    void EndGame()
    {
        gameEnded = true;

        // Freeze camera and unlock cursor when game ends
        FreezeCameraAndUnlockCursor();

        // Clean up active NPCs
        foreach (var npc in activeNPCs)
        {
            if (npc != null)
                Destroy(npc);
        }
        activeNPCs.Clear();

        // Calculate results
        int askPoints = Mathf.Max(0, shopliftersDeterred - shoppersFalselyAccused);
        bool passed = askPoints >= 3;
        string status = passed ? "PASS" : "FAIL";

        // Show appropriate screen
        if (passed)
        {
            ShowWinScreen(askPoints, status);
        }
        else
        {
            ShowLoseScreen(askPoints, status);
        }
    }

    void ShowWinScreen(int askPoints, string status)
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            winTxtDeterred.text = $"Shoplifters Deterred: {shopliftersDeterred}";
            winTxtFalselyAccused.text = $"Falsely Accused: {shoppersFalselyAccused}";
            winTxtAskPoints.text = $"ASK Points: {askPoints}";
            winTxtStatus.text = $"Status: {status}";
        }
    }

    void ShowLoseScreen(int askPoints, string status)
    {
        if (losePanel != null)
        {
            losePanel.SetActive(true);
            loseTxtDeterred.text = $"Shoplifters Deterred: {shopliftersDeterred}";
            loseTxtFalselyAccused.text = $"Falsely Accused: {shoppersFalselyAccused}";
            loseTxtAskPoints.text = $"ASK Points: {askPoints}";
            loseTxtStatus.text = $"Status: {status}";
        }
    }

    /// <summary>
    /// Public method to restart the game (can be called from UI buttons).
    /// </summary>
    public void RestartGame()
    {
        // Reset game state
        gameEnded = false;
        shopliftersDeterred = 0;
        shoppersFalselyAccused = 0;
        remainingTime = gameDuration;
        spawnTimer = spawnInterval;

        // Clean up any existing NPCs
        foreach (var npc in activeNPCs)
        {
            if (npc != null)
                Destroy(npc);
        }
        activeNPCs.Clear();

        // Hide end game panels
        if (winPanel != null)
            winPanel.SetActive(false);
        if (losePanel != null)
            losePanel.SetActive(false);

        // Update UI
        UpdateAskPointsUI();

        // Restore camera and cursor for gameplay
        RestoreCameraAndCursor();
    }
}