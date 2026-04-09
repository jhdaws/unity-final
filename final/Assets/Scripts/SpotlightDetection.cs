using UnityEngine;

public class SpotlightDetection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light spotLight;

    [Header("Detection")]
    [SerializeField] private LayerMask obstacleMask = ~0;
    [SerializeField] private float targetHeightOffset = 1.0f;
    [SerializeField] private bool debugVisibilityLogs;

    [Header("Damage")]
    [SerializeField] private float damagePerSecond = 20f;

    private Transform playerTransform;
    private PlayerHealth playerHealth;
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
        aimPoint = playerTransform.position + (Vector3.up * targetHeightOffset);

        Vector3 origin = spotLight.transform.position + (spotLight.transform.forward * 0.05f);
        Vector3 toTarget = aimPoint - origin;
        float sqrDistance = toTarget.sqrMagnitude;

        if (sqrDistance > spotLight.range * spotLight.range)
        {
            return false;
        }

        float halfAngle = spotLight.spotAngle * 0.5f;
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
        if (hits.Length > 0)
        {
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
        }

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
        visibleAimPoint = playerTransform.position + (Vector3.up * targetHeightOffset);

        int playerLayerMask = 1 << playerObject.layer;
        visibilityMask = obstacleMask | playerLayerMask;
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
