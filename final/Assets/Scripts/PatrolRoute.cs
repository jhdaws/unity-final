using UnityEngine;

public class PatrolRoute : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private bool collectChildrenOnAwake = true;
    [SerializeField] private bool drawLoopLine = true;
    [SerializeField] private Color gizmoColor = new Color(0.2f, 0.9f, 1.0f, 1.0f);

    public int Count => waypoints != null ? waypoints.Length : 0;

    private void Awake()
    {
        if (collectChildrenOnAwake)
        {
            CacheChildWaypoints();
        }
    }

    public Transform GetWaypoint(int index)
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return null;
        }

        if (index < 0 || index >= waypoints.Length)
        {
            return null;
        }

        return waypoints[index];
    }

    public int GetClosestWaypointIndex(Vector3 worldPosition)
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return -1;
        }

        float closestSqrDistance = float.MaxValue;
        int closestIndex = -1;

        for (int i = 0; i < waypoints.Length; i++)
        {
            Transform waypoint = waypoints[i];
            if (waypoint == null)
            {
                continue;
            }

            float sqrDistance = (waypoint.position - worldPosition).sqrMagnitude;
            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    [ContextMenu("Collect Child Waypoints")]
    private void CacheChildWaypoints()
    {
        int childCount = transform.childCount;
        waypoints = new Transform[childCount];

        for (int i = 0; i < childCount; i++)
        {
            waypoints[i] = transform.GetChild(i);
        }
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return;
        }

        Gizmos.color = gizmoColor;

        for (int i = 0; i < waypoints.Length; i++)
        {
            Transform waypoint = waypoints[i];
            if (waypoint == null)
            {
                continue;
            }

            Gizmos.DrawSphere(waypoint.position, 0.2f);

            Transform nextWaypoint = GetNextWaypointForGizmo(i);
            if (nextWaypoint != null)
            {
                Gizmos.DrawLine(waypoint.position, nextWaypoint.position);
            }
        }
    }

    private Transform GetNextWaypointForGizmo(int currentIndex)
    {
        int nextIndex = currentIndex + 1;
        if (nextIndex < waypoints.Length)
        {
            return waypoints[nextIndex];
        }

        if (drawLoopLine && waypoints.Length > 1)
        {
            return waypoints[0];
        }

        return null;
    }
}
