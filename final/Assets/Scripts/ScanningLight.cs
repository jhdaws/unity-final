using UnityEngine;

public class ScanningLight : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float rotationAngle = 45f;
    public float sweepSpeed = 1.5f;

    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.rotation;
    }

    void Update()
    {
        // Use a Sine wave to create a smooth back-and-forth motion
        float angle = Mathf.Sin(Time.time * sweepSpeed) * rotationAngle;
        transform.rotation = initialRotation * Quaternion.Euler(0, angle, 0);
    }
}