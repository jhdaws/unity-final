using UnityEngine;

public class SpotlightDetection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light spotLight;

    [Header("Detection")]
    [SerializeField] private LayerMask obstacleMask = ~0;
    [SerializeField] private bool debugVisibilityLogs;
    [SerializeField] private float edgeAnglePadding = 4f;

    [Header("Damage")]
    [SerializeField] private float damagePerSecond = 20f;

    private Transform playerTransform;
    private PlayerHealth playerHealth;
    private Collider playerCollider;
    private bool playerVisible;
    private bool wasPlayerVisible;
    private Vector3 visibleAimPoint;
    private int visibilityMask;

    public bool PlayerVisible => playerVisible;
    public Vector3 CurrentVisibleAimPoint => visibleAimPoint;
    public Transform PlayerTransform => playerTransform;

    private void Awake()
    {
        if (spotLight == null)
        {
            spotLight = GetComponentInChildren<Light>();
        }
    }

    private void Start()
    {
        ResolvePlayer();
    }

    private void Update()
    {
        if (playerTransform == null || playerHealth == null || spotLight == null)
        {
            return;
        }

        playerVisible = ComputeVisibility(out Vector3 detectedAimPoint);
        if (playerVisible)
        {
            visibleAimPoint = detectedAimPoint;
            playerHealth.TakeDamage(damagePerSecond * Time.deltaTime);
        }

        if (debugVisibilityLogs && playerVisible != wasPlayerVisible)
        {
            wasPlayerVisible = playerVisible;
            Debug.Log(playerVisible
                ? $"{name}: player visible"
                : $"{name}: player not visible");
        }
    }

    private bool ComputeVisibility(out Vector3 aimPoint)
    {
        Vector3 origin = spotLight.transform.position + (spotLight.transform.forward * 0.05f);
        Vector3[] samplePoints = GetAimPoints();

        for (int i = 0; i < samplePoints.Length; i++)
        {
            Vector3 samplePoint = samplePoints[i];
            if (SamplePointVisible(origin, samplePoint))
            {
                aimPoint = samplePoint;
                return true;
            }
        }

        aimPoint = GetAimPoint();
        return false;
    }

    private void ResolvePlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            return;
        }

        playerTransform = playerObject.transform;
        playerHealth = playerObject.GetComponent<PlayerHealth>();
        playerCollider =
            playerObject.GetComponent<Collider>() ??
            playerObject.GetComponentInChildren<Collider>();

        visibleAimPoint = GetAimPoint();

        int playerLayerMask = 1 << playerObject.layer;
        visibilityMask = obstacleMask | playerLayerMask;
    }

    private Vector3 GetAimPoint()
    {
        if (playerCollider != null)
        {
            return playerCollider.bounds.center;
        }

        return playerTransform.position;
    }

    private Vector3[] GetAimPoints()
    {
        Vector3 center = GetAimPoint();
        if (playerCollider == null)
        {
            return new[] { center };
        }

        Bounds bounds = playerCollider.bounds;
        Vector3 extents = bounds.extents;

        return new[]
        {
            center,
            center + Vector3.up * extents.y,
            center - Vector3.up * extents.y,
            center + Vector3.right * extents.x,
            center - Vector3.right * extents.x,
            center + Vector3.forward * extents.z,
            center - Vector3.forward * extents.z
        };
    }

    private bool SamplePointVisible(Vector3 origin, Vector3 samplePoint)
    {
        Vector3 toTarget = samplePoint - origin;
        float sqrDistance = toTarget.sqrMagnitude;
        if (sqrDistance > spotLight.range * spotLight.range)
        {
            return false;
        }

        float halfAngle = (spotLight.spotAngle * 0.5f) + edgeAnglePadding;
        float angleToTarget = Vector3.Angle(spotLight.transform.forward, toTarget);
        if (angleToTarget > halfAngle)
        {
            return false;
        }

        float distance = Mathf.Sqrt(sqrDistance);
        if (distance <= 0.001f)
        {
            return true;
        }

        Vector3 direction = toTarget / distance;
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance, visibilityMask, QueryTriggerInteraction.Ignore);
        if (hits.Length == 0)
        {
            return false;
        }

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;
            if (hitCollider == null)
            {
                continue;
            }

            Transform hitTransform = hitCollider.transform;
            if (hitTransform == transform || hitTransform.IsChildOf(transform))
            {
                continue;
            }

            return hitTransform == playerTransform || hitTransform.IsChildOf(playerTransform);
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (spotLight == null)
        {
            return;
        }

        Gizmos.color = playerVisible ? Color.red : Color.yellow;
        Gizmos.DrawRay(spotLight.transform.position, spotLight.transform.forward * spotLight.range);
    }
}
