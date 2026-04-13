using UnityEngine;

public class SearchlightEnemy : MonoBehaviour
{
    [Header("Detection Settings")]
    public float searchRange = 15f;
    public float spotAngle = 30f;
    public LayerMask obstacleMask;

    [Header("Damage Settings")]
    public float damagePerSecond = 20f;

    private Transform player;
    private PlayerHealth playerHealth;
    private float sqrRange;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerHealth = p.GetComponent<PlayerHealth>();
        }
        sqrRange = searchRange * searchRange;
    }

    void Update()
    {
        if (player == null || playerHealth == null) return;

        Vector3 dirToPlayer = (player.position - transform.position);
        
        // 1. Check Range
        if (dirToPlayer.sqrMagnitude < sqrRange)
        {
            // 2. Check Angle
            float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer.normalized);

            if (angleToPlayer < spotAngle / 2f) 
            {
                // Check Line of Sight 
                if (!Physics.Raycast(transform.position, dirToPlayer.normalized, dirToPlayer.magnitude, obstacleMask))
                {
                    ApplyLightDamage();
                }
            }
        }
    }

    void ApplyLightDamage()
    {
        // Damage scaled by frame rate
        playerHealth.TakeDamage(damagePerSecond * Time.deltaTime);
        Debug.Log("Searing light is damaging the player!");
    }
}