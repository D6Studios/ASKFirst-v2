using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Switches the crosshair to its "can talk" look (shrunk, coloured outline) while the player is aiming at an NPC.
/// </summary>
[RequireComponent(typeof(Image))]
public class Crosshair : MonoBehaviour
{
    [SerializeField] Sprite targetSprite;
    [SerializeField] float targetScale = 0.8f;
    Image image;
    Sprite defaultSprite;
    PlayerInteraction player;

    void Awake()
    {
        image = GetComponent<Image>();
        defaultSprite = image.sprite;
    }

    void Start()
    {
        player = FindAnyObjectByType<PlayerInteraction>();
    }

    void Update()
    {
        // Same target that shows the interact button.
        bool targeting = player != null && player.closestNPC != null && !player.isBusy;
        image.sprite = targeting ? targetSprite : defaultSprite;
        transform.localScale = Vector3.one * (targeting ? targetScale : 1f);
    }
}
