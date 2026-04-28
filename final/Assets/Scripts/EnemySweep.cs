using UnityEngine;

public class EnemySweep : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rotationRoot;
    [SerializeField] private EnemyLockOn lockOn;

    [Header("Sweep")]
    [SerializeField] private float rotationAngle = 45f;
    [SerializeField] private float sweepSpeed = 45f;
    [SerializeField] private bool fullRotationMode;
    [SerializeField] private float fullRotationDegreesPerSecond = 90f;

    [Header("Tracking")]
    [SerializeField] private float trackingDegreesPerSecond = 180f;

    [Header("Debug")]
    [SerializeField] private bool debugSweepLogs;

    private float currentYaw;
    private float sweepDirection = 1f;
    private Quaternion baseRotation;

    private void Awake()
    {
        if (lockOn == null)
        {
            lockOn = GetComponent<EnemyLockOn>();
        }

        if (lockOn == null)
        {
            lockOn = GetComponentInParent<EnemyLockOn>();
        }

        if (lockOn != null && rotationRoot != null && rotationRoot != lockOn.transform && rotationRoot.IsChildOf(lockOn.transform))
        {
            rotationRoot = lockOn.transform;
        }

        if (rotationRoot == null)
        {
            rotationRoot = lockOn != null ? lockOn.transform : transform;
        }
    }

    private void Start()
    {
        baseRotation = rotationRoot.rotation;

        if (debugSweepLogs)
        {
            Debug.Log($"{name}: sweep started on {rotationRoot.name}.");
        }
    }

    private void Update()
    {
        if (rotationRoot == null)
        {
            return;
        }

        if (lockOn != null && lockOn.HasTargetLock)
        {
            TrackTarget();
            return;
        }

        Sweep();
    }

    private void Sweep()
    {
        if (fullRotationMode || rotationAngle >= 179.9f)
        {
            currentYaw += fullRotationDegreesPerSecond * Time.deltaTime;
            currentYaw = Mathf.Repeat(currentYaw, 360f);
        }
        else
        {
            currentYaw += sweepDirection * sweepSpeed * Time.deltaTime;

            if (currentYaw >= rotationAngle)
            {
                currentYaw = rotationAngle;
                sweepDirection = -1f;
            }
            else if (currentYaw <= -rotationAngle)
            {
                currentYaw = -rotationAngle;
                sweepDirection = 1f;
            }
        }

        ApplyYaw();
    }

    private void TrackTarget()
    {
        if (!lockOn.TryGetFlatDirection(rotationRoot.position, out Vector3 direction))
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        float targetYaw = Mathf.DeltaAngle(baseRotation.eulerAngles.y, targetRotation.eulerAngles.y);

        currentYaw = Mathf.MoveTowardsAngle(
            currentYaw,
            targetYaw,
            trackingDegreesPerSecond * Time.deltaTime);

        ApplyYaw();
    }

    private void ApplyYaw()
    {
        rotationRoot.rotation = Quaternion.Euler(0f, baseRotation.eulerAngles.y + currentYaw, 0f);
    }
}
