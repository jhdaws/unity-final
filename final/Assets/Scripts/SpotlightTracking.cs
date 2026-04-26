using UnityEngine;

public class SpotlightTracking : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpotlightDetection spotlightDetection;
    [SerializeField] private EnemyAudio enemyAudio;

    [Header("Lock")]
    [SerializeField] private float lockAcquireDelay = 0.02f;
    [SerializeField] private float lockHoldDuration = 1.5f;
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

        if (enemyAudio == null)
        {
            enemyAudio = GetComponent<EnemyAudio>();
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
            if (hasTargetLock)
            {
                enemyAudio?.PlayAlert();
            }
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
