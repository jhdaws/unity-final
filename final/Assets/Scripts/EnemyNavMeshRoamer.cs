using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavMeshRoamer : MonoBehaviour
{
    private enum RoamMode
    {
        Loop,
        PingPong,
        Random
    }

    [Header("References")]
    [SerializeField] private PatrolRoute patrolRoute;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private SpotlightTracking spotlightTracking;

    [Header("Roaming")]
    [SerializeField] private RoamMode roamMode = RoamMode.Random;
    [SerializeField] private bool startAtClosestWaypoint = true;
    [SerializeField] private float waitAtWaypointSeconds = 0.75f;
    [SerializeField] private float waypointSampleRadius = 1.0f;
    [SerializeField] private bool pauseWhileTracking = true;

    private int currentWaypointIndex = -1;
    private int travelDirection = 1;
    private float waitTimer;
    private bool isPausedForTracking;

    private void Awake()
    {
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        if (spotlightTracking == null)
        {
            spotlightTracking = GetComponent<SpotlightTracking>();
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
        if (patrolRoute == null || patrolRoute.Count == 0 || navMeshAgent == null)
        {
            return;
        }

        if (pauseWhileTracking && spotlightTracking != null && spotlightTracking.HasTargetLock)
        {
            if (!isPausedForTracking)
            {
                navMeshAgent.isStopped = true;
                isPausedForTracking = true;
            }
            return;
        }

        if (isPausedForTracking)
        {
            navMeshAgent.isStopped = false;
            isPausedForTracking = false;
        }

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

        switch (roamMode)
        {
            case RoamMode.Loop:
                currentWaypointIndex = (currentWaypointIndex + 1) % patrolRoute.Count;
                break;

            case RoamMode.PingPong:
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
                break;

            case RoamMode.Random:
                int nextIndex = currentWaypointIndex;
                while (nextIndex == currentWaypointIndex && patrolRoute.Count > 1)
                {
                    nextIndex = Random.Range(0, patrolRoute.Count);
                }
                currentWaypointIndex = nextIndex;
                break;
        }
    }
}
