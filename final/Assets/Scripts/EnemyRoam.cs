using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyRoam : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PatrolRoute patrolRoute;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private EnemyLockOn lockOn;
    [SerializeField] private EnemyAudio enemyAudio;

    [Header("Roaming")]
    [SerializeField] private bool startAtClosestWaypoint = true;
    [SerializeField] private float waitAtWaypointSeconds = 0.75f;
    [SerializeField] private float waypointSampleRadius = 1.0f;
    [SerializeField] private bool pauseWhileTracking = true;

    [Header("Lock Rotation")]
    [SerializeField] private float lockTurnSpeed = 180f;

    private int currentWaypointIndex = -1;
    private int travelDirection = 1;
    private float waitTimer;
    private bool isPausedForTracking;
    private bool defaultUpdateRotation;

    private void Awake()
    {
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        if (lockOn == null)
        {
            lockOn = GetComponent<EnemyLockOn>();
        }

        if (enemyAudio == null)
        {
            enemyAudio = GetComponent<EnemyAudio>();
        }

        if (navMeshAgent != null)
        {
            defaultUpdateRotation = navMeshAgent.updateRotation;
        }
    }

    private void Start()
    {
        if (patrolRoute == null || patrolRoute.Count == 0 || navMeshAgent == null)
        {
            return;
        }

        currentWaypointIndex = startAtClosestWaypoint
            ? patrolRoute.GetClosestWaypointIndex(transform.position)
            : 0;

        if (currentWaypointIndex < 0)
        {
            currentWaypointIndex = 0;
        }

        MoveToCurrentWaypoint();
    }

    private void Update()
    {
        if (navMeshAgent == null)
        {
            return;
        }

        if (pauseWhileTracking && lockOn != null && lockOn.HasTargetLock)
        {
            if (!isPausedForTracking)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.updateRotation = false;
                isPausedForTracking = true;
                enemyAudio?.SetMoving(false);
            }
            RotateTowardLockTarget();
            return;
        }

        if (isPausedForTracking)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.updateRotation = defaultUpdateRotation;
            isPausedForTracking = false;
        }

        if (patrolRoute == null || patrolRoute.Count == 0)
        {
            return;
        }

        enemyAudio?.SetMoving(navMeshAgent.velocity.sqrMagnitude > 0.04f);

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                AdvanceWaypoint();
                MoveToCurrentWaypoint();
            }
            return;
        }

        if (navMeshAgent.pathPending)
        {
            return;
        }

        if (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            return;
        }

        if (navMeshAgent.hasPath && navMeshAgent.velocity.sqrMagnitude > 0.01f)
        {
            return;
        }

        waitTimer = waitAtWaypointSeconds;
        enemyAudio?.SetMoving(false);
    }

    public void SetPatrolRoute(PatrolRoute newRoute, bool restartFromClosestWaypoint = true)
    {
        patrolRoute = newRoute;

        if (patrolRoute == null || patrolRoute.Count == 0 || navMeshAgent == null)
        {
            return;
        }

        currentWaypointIndex = restartFromClosestWaypoint
            ? patrolRoute.GetClosestWaypointIndex(transform.position)
            : 0;

        if (currentWaypointIndex < 0)
        {
            currentWaypointIndex = 0;
        }

        waitTimer = 0f;
        MoveToCurrentWaypoint();
    }

    private void MoveToCurrentWaypoint()
    {
        Transform waypoint = patrolRoute.GetWaypoint(currentWaypointIndex);
        if (waypoint == null)
        {
            return;
        }

        Vector3 destination = waypoint.position;
        if (NavMesh.SamplePosition(destination, out NavMeshHit navMeshHit, waypointSampleRadius, NavMesh.AllAreas))
        {
            destination = navMeshHit.position;
        }

        navMeshAgent.SetDestination(destination);
    }

    private void AdvanceWaypoint()
    {
        if (patrolRoute.Count <= 1)
        {
            return;
        }

        currentWaypointIndex += travelDirection;

        if (currentWaypointIndex >= patrolRoute.Count)
        {
            travelDirection = -1;
            currentWaypointIndex = patrolRoute.Count - 2;
        }
        else if (currentWaypointIndex < 0)
        {
            travelDirection = 1;
            currentWaypointIndex = 1;
        }
    }

    private void RotateTowardLockTarget()
    {
        if (lockOn == null || !lockOn.TryGetFlatDirection(transform.position, out Vector3 direction))
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            lockTurnSpeed * Time.deltaTime);
    }
}
