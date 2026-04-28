using UnityEngine;

public class EnemyLockOn : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Enemy enemy;
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
        if (enemy == null)
        {
            enemy = GetComponent<Enemy>();
        }

        if (enemy == null)
        {
            enemy = GetComponentInParent<Enemy>();
        }

        if (enemyAudio == null)
        {
            enemyAudio = GetComponent<EnemyAudio>();
        }
    }

    private void Start()
    {
        if (enemy != null && enemy.PlayerTransform != null)
        {
            trackedAimPoint = enemy.CurrentVisibleAimPoint;
        }
    }

    private void Update()
    {
        if (enemy == null)
        {
            return;
        }

        if (enemy.PlayerVisible)
        {
            visibleTimer += Time.deltaTime;
            trackedAimPoint = enemy.CurrentVisibleAimPoint;

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

        if (hasTargetLock != hadTargetLock)
        {
            hadTargetLock = hasTargetLock;
            if (hasTargetLock)
            {
                enemyAudio?.PlayAlert();
            }

            if (debugStateLogs)
            {
                Debug.Log(hasTargetLock
                    ? $"{name}: lock acquired"
                    : $"{name}: lock released");
            }
        }
    }

    public bool TryGetFlatDirection(Vector3 origin, out Vector3 direction)
    {
        direction = Vector3.zero;
        if (!hasTargetLock)
        {
            return false;
        }

        direction = trackedAimPoint - origin;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return false;
        }

        direction.Normalize();
        return true;
    }
}
