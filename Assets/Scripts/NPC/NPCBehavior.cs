using UnityEngine;
using System.Collections;
public class NPCBehavior : MonoBehaviour
{
    public float playerDistance;
    [SerializeField]
    public float interactDistance = 3f;
    Outline NPCOutline;
    NPCAnimator npcAnimator;
    Coroutine currentCoroutine;
    public string currentState;
    void Start()
    {
        NPCOutline = gameObject.GetComponentInChildren<Outline>();
        npcAnimator = gameObject.GetComponent<NPCAnimator>();
        if (NPCOutline != null)
        {
            NPCOutline.enabled = false;
        }
        ChangeState("Idle");
    }
    void Update()
    {
        CheckPlayerProximity();
    }
    void CheckPlayerProximity()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerDistance = Vector3.Distance(player.transform.position, gameObject.transform.position);
        if (playerDistance <= interactDistance)
        {
            player.GetComponent<PlayerInteraction>().NPCInProximity(gameObject);
        }
    }
    public void Interact()
    {
        //Placeholder for interaction logic
        Debug.Log("Interacted with " + gameObject.name);
        ChangeState("Talking");
    }
    public void OutlineNPC(bool outline)
    {
        if (NPCOutline != null)
        {
            NPCOutline.enabled = outline;
        }
    }
    public void ChangeState(string newState)
    {
        currentState = newState;
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(newState); // Start the new animation
    }
    IEnumerator Walking()
    {
        npcAnimator.Animate("Walking");
        while (currentState == "Walking")
        {

            yield return null;
        }
    }
    IEnumerator Suspicious()
    {
        npcAnimator.Animate("Suspicious");
        while (currentState == "Suspicious")
        {

            yield return null;
        }
    }
    IEnumerator Idle()
    {
        npcAnimator.Animate("Idle");
        while (currentState == "Idle")
        {
            Debug.Log("NPC is idle.");
            yield return new WaitForSeconds(1f); // Wait for 1 second before checking again
        }
    }
    IEnumerator Talking()
    {
        npcAnimator.Animate("InteractedWith");
        while (currentState == "Talking")
        {
            Debug.Log("NPC is talking.");
            yield return new WaitForSeconds(1f); // Wait for 1 second before checking again
        }
    }
}
