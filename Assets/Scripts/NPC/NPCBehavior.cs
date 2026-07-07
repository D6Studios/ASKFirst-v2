using UnityEngine;
using System.Collections;
using UnityEngine.AI;
public class NPCBehavior : MonoBehaviour
{
    public float playerDistance;
    [SerializeField]
    public float interactDistance = 3f;
    Outline NPCOutline;
    NPCAnimator npcAnimator;
    Coroutine currentCoroutine;
    public string currentState;
    NavMeshAgent agent;
    [SerializeField] int idleWaitTime;
    [SerializeField] Transform[] patrolPoints;
    int currentPatrolIndex = 0;
    void Start()
    {
        NPCOutline = gameObject.GetComponentInChildren<Outline>();
        npcAnimator = gameObject.GetComponent<NPCAnimator>();
        agent = gameObject.GetComponent<NavMeshAgent>();
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
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            if ((Mathf.Floor(transform.position.x) == Mathf.Floor(patrolPoints[currentPatrolIndex].position.x) && Mathf.Floor(transform.position.z) == Mathf.Floor(patrolPoints[currentPatrolIndex].position.z)))
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length; // Move to the next patrol point
                ChangeState("Idle");
                yield break;
            }
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
            yield return new WaitForSeconds(idleWaitTime);
            ChangeState("Walking");
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
