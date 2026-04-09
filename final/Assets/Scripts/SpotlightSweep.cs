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
    [SerializeField] private bool trackPitch;

    private Quaternion initialLocalRotation;
    private float currentSweepAngle;
    private float sweepDirection = 1f;
    private bool wasTrackingLastFrame;

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
        initialLocalRotation = sweepPivot.localRotation;
        currentSweepAngle = 0f;
    }

    private void Update()
    {
        if (sweepPivot == null)
        {
            return;
        }

        Quaternion targetRotation = sweepPivot.rotation;
        bool isTracking = false;
        if (spotlightTracking != null)
        {
            isTracking = spotlightTracking.TryGetTrackedLookRotation(
                sweepPivot.position,
                trackPitch,
                out targetRotation);
        }

        if (isTracking)
        {
            sweepPivot.rotation = Quaternion.RotateTowards(
                sweepPivot.rotation,
                targetRotation,
                trackingDegreesPerSecond * Time.deltaTime);
        }
        else
        {
            if (wasTrackingLastFrame)
            {
                currentSweepAngle = GetCurrentLocalYawOffset();
            }

            if (fullRotationMode || rotationAngle >= 179.9f)
            {
                currentSweepAngle += fullRotationDegreesPerSecond * Time.deltaTime;
                currentSweepAngle = Mathf.Repeat(currentSweepAngle, 360f);
            }
            else
            {
                currentSweepAngle += sweepDirection * sweepSpeed * Time.deltaTime;

                if (currentSweepAngle > rotationAngle)
                {
                    currentSweepAngle = rotationAngle;
                    sweepDirection = -1f;
                }
                else if (currentSweepAngle < -rotationAngle)
                {
                    currentSweepAngle = -rotationAngle;
                    sweepDirection = 1f;
                }
            }

            sweepPivot.localRotation = initialLocalRotation * Quaternion.Euler(0f, currentSweepAngle, 0f);
        }

        wasTrackingLastFrame = isTracking;
    }

    private float GetCurrentLocalYawOffset()
    {
        Quaternion delta = Quaternion.Inverse(initialLocalRotation) * sweepPivot.localRotation;
        float yaw = NormalizeAngle(delta.eulerAngles.y);

        if (fullRotationMode || rotationAngle >= 179.9f)
        {
            return Mathf.Repeat(yaw + 360f, 360f);
        }

        return Mathf.Clamp(yaw, -rotationAngle, rotationAngle);
    }

    private static float NormalizeAngle(float angle)
    {
        if (angle > 180f)
        {
            angle -= 360f;
        }
        return angle;
    }
}
