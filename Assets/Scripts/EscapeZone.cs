
using UnityEngine;

public class EscapeZone : MonoBehaviour
{
    private GameManager gameManager;

    /// <summary>
    /// Sets a reference to the GameManager.
    /// </summary>
    public void SetGameManager(GameManager manager)
    {
        gameManager = manager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Shopper"))
        {
            gameManager?.ShoplifterEscaped();
            Debug.Log("✅ Detected a shopper!");
        }
    }
}