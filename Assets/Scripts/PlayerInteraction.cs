using UnityEngine;
using StarterAssets;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    /// <summary>
    /// Reference to the StarterAssetsInputs component, which is used to detect player input for interactions. This allows the script to check when the player presses the interact button and respond accordingly by triggering interactions with NPCs.
    /// </summary>
    private StarterAssetsInputs _input;
    /// <summary>
    /// Bool state to determine if the player is currently interacting with an NPC. This can be used to prevent multiple interactions at once or to trigger specific animations or UI elements during interaction.
    /// </summary>
    public bool isBusy = false;
    /// <summary>
    /// Reference to the closest NPC GameObject that the player can interact with. This is updated based on the player's proximity to NPCs and is used to determine which NPC the player will interact with when they press the interact button.
    /// </summary>
    public GameObject closestNPC;
    /// <summary>
    /// Float value representing the distance from the player to the closest NPC. This is used to determine if the player is within interaction range of the NPC and can be used for UI feedback (e.g., showing an interaction prompt when close enough).
    /// </summary>
    public float closestNPCDistance;
    /// <summary>
    /// Reference to the mobile controls UI
    /// </summary>
    GameObject mobileControls;
    /// <summary>
    /// dialogueUI is a UI canvas that is used to display dialogue options and text when the player interacts with an NPC. It can be activated or deactivated based on the player's interaction state.
    /// </summary>
    VisualElement mobileControlsUI;
    [SerializeField] UnityEngine.UI.Button interactButton;
    NPCFocusCamera npcFocusCamera;
    GameObject optionsButton;
    [SerializeField] Transform spawnPoint;
    [SerializeField] float interactDistance = 4f;

    void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();
        closestNPCDistance = Mathf.Infinity;
        mobileControls = GameObject.FindGameObjectWithTag("MobileControls");
        mobileControlsUI = mobileControls.GetComponent<UIDocument>().rootVisualElement;

        npcFocusCamera = GameObject.FindGameObjectWithTag("NPCFocusCamera").GetComponent<NPCFocusCamera>();
        optionsButton = GameObject.FindGameObjectWithTag("OptionsButton");

    }

    // Update is called once per frame
    void Update()
    {
        Raycast();
        CheckMobileControls();
        if (isBusy)
        {
            interactButton.gameObject.SetActive(false);
        }

    }
    public void Interact()
    {
        if (!isBusy && closestNPC != null)
        {
            SoundManager.Instance.PlaySound(Resources.Load<AudioClip>("pop"));
            closestNPC.GetComponent<NPCBehavior>().Interact();
            gameObject.GetComponent<PlayerDialogue>().StartDialogue(closestNPC);
            isBusy = true;
            Debug.Log("Interact");
            npcFocusCamera.FocusOnNPC(closestNPC.transform);
        }

    }
    void Raycast()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(spawnPoint.position, spawnPoint.forward, out hitInfo, interactDistance))
        {
            //show a line in the scene view for debugging purposes
            Debug.DrawLine(spawnPoint.position, hitInfo.point, Color.red);
            GameObject hitObject = hitInfo.collider.gameObject;
            if (hitObject.CompareTag("NPC"))
            {

                if (hitObject.GetComponent<NPCBehavior>().interactedWith)
                {
                    interactButton.gameObject.SetActive(false);
                    return;
                }
                interactButton.gameObject.SetActive(true);
                NPCProximity(hitObject);

            }
            else
            {
                interactButton.gameObject.SetActive(false);
                if (closestNPC != null)
                {
                    closestNPC.GetComponent<NPCBehavior>().OutlineNPC(false);
                }
                closestNPC = null;
            }
        }
        else
        {
            interactButton.gameObject.SetActive(false);
            if (closestNPC != null)
            {
                closestNPC.GetComponent<NPCBehavior>().OutlineNPC(false);
            }
            closestNPC = null;
        }

    }
    public void NPCProximity(GameObject npc)
    {
        float distance = Vector3.Distance(transform.position, npc.transform.position);

        if (closestNPC != null)
        {
            closestNPC.GetComponent<NPCBehavior>().OutlineNPC(false);
        }
        closestNPC = npc;
        closestNPC.GetComponent<NPCBehavior>().OutlineNPC(true);
        closestNPCDistance = distance;

    }
    void CheckMobileControls()
    {
        if (mobileControlsUI == null) return;
        if (isBusy)
        {
            mobileControlsUI.style.display = DisplayStyle.None;
            optionsButton.SetActive(false);
        }
        else
        {
            mobileControlsUI.style.display = DisplayStyle.Flex;
            optionsButton.SetActive(true);

        }
    }
}
