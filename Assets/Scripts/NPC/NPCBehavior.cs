using UnityEngine;
using System.Collections;
using UnityEngine.AI;
public class NPCBehavior : MonoBehaviour
{
    public float playerDistance;
    [SerializeField]
    public float DangerDistance = 2f;
    Outline NPCOutline;
    NPCAnimator npcAnimator;
    Coroutine currentCoroutine;
    public string currentState;
    NavMeshAgent agent;
    [SerializeField] Transform[] patrolPoints;
    int currentPatrolIndex = 0;
    [SerializeField] Vector2 idleWaitTime;
    FacialExpressions facialExpressions;
    void Start()
    {
        NPCOutline = gameObject.GetComponentInChildren<Outline>();
        npcAnimator = gameObject.GetComponent<NPCAnimator>();
        agent = gameObject.GetComponent<NavMeshAgent>();
        facialExpressions = gameObject.GetComponent<FacialExpressions>();
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
        if (playerDistance < DangerDistance)
        {
            Debug.Log("Player is too close! NPC is suspicious.");
            //@TODO: Implement behavior when player is too close
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
        agent.isStopped = false;
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
        agent.isStopped = true;
        npcAnimator.Animate("Idle");
        facialExpressions.IsNeutral();
        while (currentState == "Idle")
        {
            float waitTime = Random.Range(idleWaitTime.x, idleWaitTime.y);
            yield return new WaitForSeconds(waitTime);
            ChangeState("Walking");
        }
    }
    IEnumerator Talking()
    {
        agent.isStopped = true;
        StartCoroutine(TurnToPlayer());
        npcAnimator.Animate("InteractedWith");
        facialExpressions.IsThinking();
        while (currentState == "Talking")
        {
            Debug.Log("NPC is talking.");
            yield return new WaitForSeconds(1f); // Wait for 1 second before checking again
        }
    }
    IEnumerator TurnToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
        while (transform.rotation != lookRotation)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            yield return null;
        }
    }
}
