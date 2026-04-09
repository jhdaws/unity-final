using UnityEngine;

public class SpotlightTracking : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpotlightDetection spotlightDetection;

    [Header("Lock")]
    [SerializeField] private float lockAcquireDelay = 0.1f;
    [SerializeField] private float lockHoldDuration = 1.0f;
    [SerializeField] private bool debugStateLogs;

    private bool hasTargetLock;
    private bool hadTargetLock;
    private float visibleTimer;
    private float lockTimer;
    private Vector3 trackedAimPoint;

    public bool HasTargetLock => hasTargetLock;
    public Vector3 CurrentAimPoint => trackedAimPoint;

    private void Awake()
    {
        if (spotlightDetection == null)
        {
            spotlightDetection = GetComponent<SpotlightDetection>();
        }

        if (spotlightDetection == null)
        {
            spotlightDetection = GetComponentInParent<SpotlightDetection>();
        }
    }

    private void Start()
    {
        if (spotlightDetection != null && spotlightDetection.PlayerTransform != null)
        {
            trackedAimPoint = spotlightDetection.CurrentVisibleAimPoint;
        }
    }

    private void Update()
    {
        if (spotlightDetection == null)
        {
            return;
        }

        if (spotlightDetection.PlayerVisible)
        {
            visibleTimer += Time.deltaTime;
            trackedAimPoint = spotlightDetection.CurrentVisibleAimPoint;

            if (visibleTimer >= lockAcquireDelay)
            {
                hasTargetLock = true;
                lockTimer = lockHoldDuration;
            }
        }
        else
        {
            visibleTimer = 0f;

            if (hasTargetLock)
            {
                lockTimer -= Time.deltaTime;
                if (lockTimer <= 0f)
                {
                    hasTargetLock = false;
                }
            }
        }

        if (debugStateLogs && hasTargetLock != hadTargetLock)
        {
            hadTargetLock = hasTargetLock;
            Debug.Log(hasTargetLock
                ? $"{name}: lock acquired"
                : $"{name}: lock released");
        }
    }

    public bool TryGetTrackedLookRotation(Vector3 pivotPosition, bool trackPitch, out Quaternion rotation)
    {
        rotation = Quaternion.identity;
        if (!hasTargetLock)
        {
            return false;
        }

        Vector3 direction = trackedAimPoint - pivotPosition;
        if (!trackPitch)
        {
            direction.y = 0f;
        }

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return false;
        }

        rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        return true;
    }
}
