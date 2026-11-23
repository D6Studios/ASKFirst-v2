using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.Serialization;

public enum NPCState { Idle, Walking, Suspicious, Talking, Leaving }

public class NPCBehavior : MonoBehaviour
{
    public float playerDistance;
    [SerializeField]
    public float DangerDistance = 2f;
    Outline NPCOutline;
    NPCAnimator npcAnimator;
    Coroutine currentCoroutine;
    Coroutine turningCoroutine;
    public NPCState currentState { get; private set; }
    NavMeshAgent agent;
    [SerializeField] Transform[] patrolPoints;
    int currentPatrolIndex = 0;
    [SerializeField] Vector2 idleWaitTime;
    FacialExpressions facialExpressions;
    [SerializeField] public bool isShoplifter = false;
    public bool interactedWith = false;
    [Tooltip("Walk to the first patrol point on start, then stay there (instead of patrolling between points).")]
    [FormerlySerializedAs("isLevel2NPC")]
    [SerializeField] public bool walkOnceThenStay = false;
    [SerializeField] SafeDistanceIndicator safeDistanceIndicator;
    public bool safeDistanceBreached = false;
    [Tooltip("Player must get closer than this to be penalised in the score. Breaching DangerDistance alone only shows the warning.")]
    [SerializeField] public float PenaltyDistance = 1f;
    public bool penaltyDistanceBreached = false;
    bool playerTooClose = false;
    bool encounterActive = true;
    bool watchDistance; // shoplifters and background customers show the safe-distance ring

    Transform exit;
    Transform player;
    void Start()
    {
        NPCOutline = gameObject.GetComponentInChildren<Outline>();
        npcAnimator = gameObject.GetComponent<NPCAnimator>();
        agent = gameObject.GetComponent<NavMeshAgent>();
        facialExpressions = gameObject.GetComponent<FacialExpressions>();
        exit = GameObject.FindGameObjectWithTag("Exit").transform;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        watchDistance = isShoplifter || GetComponent<AmbientNPC>() != null;
        if (safeDistanceIndicator != null)
        {
            safeDistanceIndicator.SetRadius(DangerDistance);
            safeDistanceIndicator.gameObject.SetActive(watchDistance); // fades itself in/out based on player distance
        }
        if (NPCOutline != null)
        {
            NPCOutline.enabled = false;
        }
        SetupNavigation();
        ChangeState(walkOnceThenStay ? NPCState.Walking : NPCState.Idle);
    }
    void Update()
    {
        CheckPlayerProximity();
    }
    void CheckPlayerProximity()
    {
        if (!watchDistance || !encounterActive)
        {
            return; // Safe-distance monitoring only applies to shoplifters and background customers, and only for the duration of the encounter (approach through end of conversation)
        }
        playerDistance = Vector3.Distance(player.position, gameObject.transform.position);
        DangerDirectionHUD.Report(transform, playerDistance, DangerDistance);
        bool tooClose = playerDistance < DangerDistance;
        if (isShoplifter && playerDistance < PenaltyDistance)
        {
            penaltyDistanceBreached = true; // Only this affects the final score; background customers never cost points
        }
        if (tooClose && !playerTooClose)
        {
            safeDistanceBreached = true;
            if (SafeDistanceWarningUI.Instance != null) SafeDistanceWarningUI.Instance.ShowWarning();
            if (safeDistanceIndicator != null) safeDistanceIndicator.SetBreached(true);
        }
        else if (!tooClose && playerTooClose)
        {
            if (safeDistanceIndicator != null) safeDistanceIndicator.SetBreached(false);
        }
        playerTooClose = tooClose;
    }

    // Called by PlayerDialogue.EndDialogue once the conversation with this NPC is over.
    public void EndEncounter()
    {
        encounterActive = false;
        if (safeDistanceIndicator != null) safeDistanceIndicator.Dismiss();
    }

    public void Interact()
    {
        //Placeholder for interaction logic
        Debug.Log("Interacted with " + gameObject.name);
        ChangeState(NPCState.Talking);
    }
    public void OutlineNPC(bool outline)
    {
        if (NPCOutline != null)
        {
            NPCOutline.enabled = outline;
        }
    }
    public void ChangeState(NPCState newState)
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
        currentCoroutine = StartCoroutine(StateRoutine(newState));
    }
    IEnumerator StateRoutine(NPCState state)
    {
        switch (state)
        {
            case NPCState.Walking: return Walking();
            case NPCState.Suspicious: return Suspicious();
            case NPCState.Talking: return Talking();
            case NPCState.Leaving: return Leaving();
            default: return Idle();
        }
    }
    // --- Movement -------------------------------------------------------------------------
    // Standing NPCs switch from NavMeshAgent to a carving NavMeshObstacle, so walkers plan paths
    // around them instead of pushing into an agent that never steps aside. Walkers use the agent.

    const float StuckSpeed = 0.15f;  // m/s; slower than this while trying to walk counts as blocked
    const float StuckTime = 1.5f;    // seconds blocked before giving up on the current destination
    const float ArriveDistance = 0.3f;

    NavMeshObstacle obstacle;

    void SetupNavigation()
    {
        agent.avoidancePriority = Random.Range(40, 61); // unequal priorities stop two walkers mirroring each other head-on
        obstacle = GetComponent<NavMeshObstacle>();
        if (obstacle == null) obstacle = gameObject.AddComponent<NavMeshObstacle>();
        obstacle.shape = NavMeshObstacleShape.Capsule;
        obstacle.center = new Vector3(0f, agent.height / 2f, 0f);
        obstacle.height = agent.height;
        obstacle.radius = agent.radius * 0.6f; // carving also grows by the agent radius; keep aisles passable
        obstacle.carving = true;
        obstacle.carveOnlyStationary = false;
        obstacle.enabled = false;
        EnsurePlayerObstacle(player);
    }

    // NPCs avoid the player: while the player stands still they plan around them, while moving they steer around them.
    static void EnsurePlayerObstacle(Transform player)
    {
        if (player.GetComponent<NavMeshObstacle>() != null) return;
        var cc = player.GetComponent<CharacterController>();
        var o = player.gameObject.AddComponent<NavMeshObstacle>();
        o.shape = NavMeshObstacleShape.Capsule;
        o.center = cc != null ? cc.center : new Vector3(0f, 1f, 0f);
        o.height = cc != null ? cc.height : 2f;
        o.radius = cc != null ? cc.radius : 0.5f;
        o.carving = true;
        o.carveOnlyStationary = true;
        o.carvingMoveThreshold = 0.1f;
        o.carvingTimeToStationary = 0.4f;
    }

    void BecomeObstacle()
    {
        if (agent.enabled) agent.enabled = false;
        obstacle.enabled = true;
    }

    // Lift our own carve first; re-enabling the agent the same frame would place it beside its own hole.
    IEnumerator BecomeAgent()
    {
        if (obstacle.enabled)
        {
            obstacle.enabled = false;
            yield return null;
        }
        agent.enabled = true;
        agent.isStopped = false;
        agent.updateRotation = true;
    }

    bool Arrived()
    {
        if (agent.pathPending) return false;
        // remainingDistance reads 0 before a path exists, so fall back to the straight-line distance.
        if (agent.hasPath) return agent.remainingDistance <= ArriveDistance;
        Vector3 d = agent.destination - transform.position; d.y = 0f;
        return d.magnitude <= ArriveDistance;
    }

    IEnumerator Walking()
    {
        yield return BecomeAgent();
        npcAnimator.Animate(NPCAnimation.Walking);
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        float blocked = 0f;
        while (currentState == NPCState.Walking)
        {
            if (Arrived())
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                ChangeState(NPCState.Idle);
                yield break;
            }
            blocked = !agent.pathPending && agent.velocity.magnitude < StuckSpeed ? blocked + Time.deltaTime : 0f;
            if (blocked > StuckTime || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                if (walkOnceThenStay)
                {
                    // Must reach its spot: re-plan, the way may have cleared.
                    agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                    blocked = 0f;
                }
                else
                {
                    // Blocked: pause here and head for the next spot afterwards.
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                    ChangeState(NPCState.Idle);
                    yield break;
                }
            }
            yield return null;
        }
    }
    IEnumerator Suspicious()
    {
        BecomeObstacle();
        npcAnimator.Animate(NPCAnimation.Suspicious);
        while (currentState == NPCState.Suspicious)
        {
            yield return null;
        }
    }
    IEnumerator Idle()
    {
        BecomeObstacle();
        npcAnimator.Animate(NPCAnimation.Idle);
        facialExpressions.IsNeutral();
        while (currentState == NPCState.Idle)
        {
            float waitTime = Random.Range(idleWaitTime.x, idleWaitTime.y);
            yield return new WaitForSeconds(waitTime);
            if (patrolPoints.Length > 0 && !walkOnceThenStay)
            {
                ChangeState(NPCState.Walking);
            }
        }
    }
    IEnumerator Talking()
    {
        BecomeObstacle();
        turningCoroutine = StartCoroutine(TurnToPlayer());
        npcAnimator.Animate(NPCAnimation.InteractedWith);
        facialExpressions.IsThinking();
        while (currentState == NPCState.Talking)
        {
            yield return null;
        }
    }
    IEnumerator TurnToPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
        while (transform.rotation != lookRotation)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            yield return null;
        }
    }
    IEnumerator Leaving()
    {
        yield return BecomeAgent();
        agent.speed = 1.7f; // Increase speed for leaving
        npcAnimator.Animate(NPCAnimation.Walking);
        agent.SetDestination(exit.position);
        float blocked = 0f;
        while (currentState == NPCState.Leaving)
        {
            Vector3 toExit = exit.position - transform.position; toExit.y = 0f;
            // The exit may sit just off the NavMesh, so reaching the end of a complete path also counts.
            if (toExit.magnitude < 1f || (Arrived() && agent.pathStatus == NavMeshPathStatus.PathComplete))
            {
                Destroy(gameObject);
                yield break;
            }
            blocked = !agent.pathPending && agent.velocity.magnitude < StuckSpeed ? blocked + Time.deltaTime : 0f;
            if (blocked > StuckTime)
            {
                agent.SetDestination(exit.position); // re-plan around whatever is in the way
                blocked = 0f;
            }
            yield return null;
        }
    }
}
