
using UnityEngine;

/// <summary>
/// Ensures the game time is running when the scene starts.
/// Attach this to a GameObject in your scene (e.g., GameManager).
/// </summary>
public class UnfreezeTimeOnStart : MonoBehaviour
{
    private void Awake()
    {
        // Reset time scale to normal speed
        if (Time.timeScale == 0f)
        {
            Debug.Log("Time was frozen. Unfreezing...");
        }

        Time.timeScale = 1f;
    }
}
