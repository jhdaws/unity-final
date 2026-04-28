using UnityEngine;

public class SpotlightSweep : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform sweepPivot;
    [SerializeField] private SpotlightTracking spotlightTracking;

    [Header("Sweep")]
    [SerializeField] private float rotationAngle = 45f;
    [SerializeField] private float sweepSpeed = 45f;
    [SerializeField] private bool fullRotationMode;
    [SerializeField] private float fullRotationDegreesPerSecond = 90f;

    [Header("Tracking")]
    [SerializeField] private float trackingDegreesPerSecond = 180f;

    [Header("Debug")]
    [SerializeField] private bool debugSweepLogs;

    private Quaternion baseLocalRotation;
    private float currentYaw;
    private float sweepDirection = 1f;

    private void Awake()
    {
        if (sweepPivot == null)
        {
            sweepPivot = transform;
        }

        if (spotlightTracking == null)
        {
            spotlightTracking = GetComponent<SpotlightTracking>();
        }

        if (spotlightTracking == null)
        {
            spotlightTracking = GetComponentInParent<SpotlightTracking>();
        }
    }

    private void Start()
    {
        baseLocalRotation = sweepPivot.localRotation;

        if (debugSweepLogs)
        {
            Debug.Log($"{name}: sweep started on {sweepPivot.name}.");
        }
    }

    private void Update()
    {
        if (sweepPivot == null)
        {
            return;
        }

        if (spotlightTracking != null && spotlightTracking.HasTargetLock)
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
        Vector3 toTarget = spotlightTracking.CurrentAimPoint - sweepPivot.position;

        if (toTarget.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion parentRotation = GetParentRotation();
        Vector3 parentLocalDirection = Quaternion.Inverse(parentRotation) * toTarget;
        parentLocalDirection.y = 0f;

        if (parentLocalDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Vector3 baseForward = baseLocalRotation * Vector3.forward;
        baseForward.y = 0f;

        float baseYaw = baseForward.sqrMagnitude > 0.0001f
            ? Mathf.Atan2(baseForward.x, baseForward.z) * Mathf.Rad2Deg
            : 0f;

        float targetYaw = Mathf.Atan2(parentLocalDirection.x, parentLocalDirection.z) * Mathf.Rad2Deg - baseYaw;

        currentYaw = Mathf.MoveTowardsAngle(
            currentYaw,
            targetYaw,
            trackingDegreesPerSecond * Time.deltaTime);

        ApplyYaw();
    }

    private void ApplyYaw()
    {
        Quaternion parentRotation = GetParentRotation();
        Vector3 sweepAxis = parentRotation * Vector3.up;
        Quaternion baseWorldRotation = parentRotation * baseLocalRotation;
        Quaternion targetWorldRotation = Quaternion.AngleAxis(currentYaw, sweepAxis) * baseWorldRotation;

        sweepPivot.localRotation = Quaternion.Inverse(parentRotation) * targetWorldRotation;
    }

    private Quaternion GetParentRotation()
    {
        return sweepPivot.parent != null ? sweepPivot.parent.rotation : Quaternion.identity;
    }
}
