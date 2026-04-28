using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    private enum PatrolMode
    {
        Loop,
        PingPong
    }

    [Header("References")]
    [SerializeField] private PatrolRoute patrolRoute;
    [SerializeField] private Transform movementRoot;
    [SerializeField] private EnemyLockOn lockOn;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float turnSpeed = 240f;
    [SerializeField] private float waypointReachDistance = 0.2f;
    [SerializeField] private PatrolMode patrolMode = PatrolMode.Loop;
    [SerializeField] private bool pauseWhileTracking = true;

    [Header("Stops")]
    [SerializeField] private float waitAtWaypointSeconds = 0.5f;
    [SerializeField] private bool faceMovementDirection = true;

    private int currentWaypointIndex;
    private int travelDirection = 1;
    private float waitTimer;

    private void Awake()
    {
        if (movementRoot == null)
        {
            movementRoot = transform;
        }

        if (lockOn == null)
        {
            lockOn = GetComponent<EnemyLockOn>();
        }
    }

    private void Start()
    {
        SnapToFirstWaypointIfNeeded();
    }

    private void Update()
    {
        if (patrolRoute == null || patrolRoute.Count == 0 || movementRoot == null)
        {
            return;
        }

        if (pauseWhileTracking && lockOn != null && lockOn.HasTargetLock)
        {
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        Transform waypoint = patrolRoute.GetWaypoint(currentWaypointIndex);
        if (waypoint == null)
        {
            return;
        }

        Vector3 toWaypoint = waypoint.position - movementRoot.position;
        float distanceToWaypoint = toWaypoint.magnitude;

        if (distanceToWaypoint <= waypointReachDistance)
        {
            AdvanceWaypoint();
            waitTimer = waitAtWaypointSeconds;
            return;
        }

        Vector3 moveDirection = toWaypoint / distanceToWaypoint;
        movementRoot.position = Vector3.MoveTowards(
            movementRoot.position,
            waypoint.position,
            moveSpeed * Time.deltaTime);

        if (faceMovementDirection)
        {
            RotateTowards(moveDirection);
        }
    }

    private void RotateTowards(Vector3 moveDirection)
    {
        moveDirection.y = 0f;
        if (moveDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized, Vector3.up);
        movementRoot.rotation = Quaternion.RotateTowards(
            movementRoot.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime);
    }

    private void SnapToFirstWaypointIfNeeded()
    {
        Transform firstWaypoint = patrolRoute != null ? patrolRoute.GetWaypoint(0) : null;
        if (firstWaypoint == null || movementRoot == null)
        {
            return;
        }

        if ((movementRoot.position - firstWaypoint.position).sqrMagnitude > 0.0001f)
        {
            return;
        }

        currentWaypointIndex = 0;
    }

    private void AdvanceWaypoint()
    {
        if (patrolRoute.Count <= 1)
        {
            return;
        }

        if (patrolMode == PatrolMode.Loop)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolRoute.Count;
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
}
