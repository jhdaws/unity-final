using UnityEngine;

public class SpotlightDetection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform visionOrigin;
    [SerializeField] private Light spotLight;

    [Header("Detection")]
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float targetHeightOffset = 1.0f;
    [SerializeField] private float hitRadius = 0.15f;

    [Header("Damage")]
    [SerializeField] private float damagePerSecond = 20f;

    private Transform playerTransform;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        if (visionOrigin == null)
        {
            visionOrigin = transform;
        }

        if (spotLight == null)
        {
            spotLight = GetComponentInChildren<Light>();
        }
    }

    private void Reset()
    {
        if (visionOrigin == null)
        {
            visionOrigin = transform;
        }

        if (spotLight == null)
        {
            spotLight = GetComponentInChildren<Light>();
        }

        if (playerMask == 0)
        {
            int layer = LayerMask.NameToLayer("Player");
            if (layer >= 0) playerMask = 1 << layer;
        }

        if (obstacleMask == 0)
        {
            obstacleMask = ~0;
        }
    }

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            playerTransform = p.transform;
            playerHealth = p.GetComponent<PlayerHealth>();
        }
    }

    private void Update()
    {
        if (playerTransform == null || playerHealth == null || spotLight == null || visionOrigin == null) return;

        if (CanSeePlayer())
        {
            ApplyLightDamage();
        }        
    }

    private bool CanSeePlayer()
    {
        Vector3 origin = visionOrigin.position;
        Vector3 target = playerTransform.position + (Vector3.up * targetHeightOffset);
        Vector3 toTarget = target - origin;

        float range = spotLight.range;
        float sqrDistance = toTarget.sqrMagnitude;
        if (sqrDistance > range * range)
        {
            return false;
        }

        float halfAngle = spotLight.spotAngle * 0.5f;
        float angle = Vector3.Angle(visionOrigin.forward, toTarget);
        if (angle > halfAngle)
        {
            return false;
        }

        float distance = Mathf.Sqrt(sqrDistance);
        Vector3 direction = toTarget / distance;
        int mask = playerMask | obstacleMask;

        if (Physics.SphereCast(origin, hitRadius, direction, out RaycastHit hit, distance, mask, QueryTriggerInteraction.Ignore))
        {
            return ((1 << hit.collider.gameObject.layer) & playerMask) != 0;
        }

        return false;
    }

    private void ApplyLightDamage()
    {
        playerHealth.TakeDamage(damagePerSecond * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (visionOrigin == null || spotLight == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(visionOrigin.position, visionOrigin.forward * spotLight.range);
    }
}
