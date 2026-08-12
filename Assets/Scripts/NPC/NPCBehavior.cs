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
    Coroutine turningCoroutine;
    public string currentState;
    NavMeshAgent agent;
    [SerializeField] Transform[] patrolPoints;
    int currentPatrolIndex = 0;
    [SerializeField] Vector2 idleWaitTime;
    FacialExpressions facialExpressions;
    [SerializeField] public bool isShoplifter = false;
    public bool interactedWith = false;
    [SerializeField] public bool isLevel2NPC = false;

    Transform exit;
    void Start()
    {
        NPCOutline = gameObject.GetComponentInChildren<Outline>();
        npcAnimator = gameObject.GetComponent<NPCAnimator>();
        agent = gameObject.GetComponent<NavMeshAgent>();
        facialExpressions = gameObject.GetComponent<FacialExpressions>();
        exit = GameObject.FindGameObjectWithTag("Exit").transform;
        if (NPCOutline != null)
        {
            NPCOutline.enabled = false;
        }
        if (isLevel2NPC) //Hard coded cos we ran out of time. Sorry!
        {

            ChangeState("Walking");

        }
        else
        {
            ChangeState("Idle");
        }
    }
    void Update()
    {
        CheckPlayerProximity();
    }
    void CheckPlayerProximity()
    {
        if (interactedWith)
        {
            return; // Skip proximity checks if the NPC has already been interacted with
        }
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerDistance = Vector3.Distance(player.transform.position, gameObject.transform.position);
        if (isShoplifter)
        {
            if (playerDistance < DangerDistance)
            {

                int randomMistakeIndex = Random.Range(0, 1);
                if (randomMistakeIndex == 0)
                {
                    GameManager.Instance.AddMistake(4);
                }
                else if (randomMistakeIndex == 1)
                {
                    GameManager.Instance.AddMistake(6);
                }
            }

            if (playerDistance < 1)
            {
                GameManager.Instance.AddMistake(10);
            }
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
        if (turningCoroutine != null)
        {
            StopCoroutine(turningCoroutine);
            turningCoroutine = null;
        }
        currentState = newState;
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(newState); // Start the new animation
    }
    IEnumerator Walking()
    {
        agent.updateRotation = true; // Re-enable automatic rotation
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

        agent.updateRotation = true; // Re-enable automatic rotation
        agent.isStopped = true;
        npcAnimator.Animate("Idle");
        facialExpressions.IsNeutral();
        while (currentState == "Idle")
        {
            float waitTime = Random.Range(idleWaitTime.x, idleWaitTime.y);
            yield return new WaitForSeconds(waitTime);
            if (patrolPoints.Length > 0 && !isLevel2NPC)
            {
                ChangeState("Walking");
            }

        }
    }
    IEnumerator Talking()
    {
        agent.updateRotation = false; // Disable automatic rotation
        agent.isStopped = true;
        turningCoroutine = StartCoroutine(TurnToPlayer());
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
    IEnumerator Leaving()
    {
        agent.updateRotation = true; // Re-enable automatic rotation
        agent.isStopped = false;
        gameObject.GetComponent<NavMeshAgent>().speed = 1.7f; // Increase speed for leaving
        npcAnimator.Animate("Walking");

        while (currentState == "Leaving")
        {
            agent.SetDestination(exit.position);
            if ((Mathf.Floor(transform.position.x) == Mathf.Floor(exit.position.x) && Mathf.Floor(transform.position.z) == Mathf.Floor(exit.position.z)))
            {
                Destroy(gameObject);
                yield break;
            }
            yield return null;
        }
    }
}
