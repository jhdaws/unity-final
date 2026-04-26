using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private AudioSource loopingSource;
    [SerializeField] private AudioSource oneShotSource;

    [Header("Clips")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip crouchClip;
    [SerializeField] private AudioClip hurtClip;

    [Header("Tuning")]
    [SerializeField] private float footstepInterval = 0.45f;
    [SerializeField] private float crouchVolume = 0.8f;
    [SerializeField] private float jumpVolume = 0.9f;
    [SerializeField] private float hurtVolume = 1.0f;
    [SerializeField] private float hurtCooldown = 0.25f;

    private float footstepTimer;
    private float hurtTimer;
    private int lastFootstepIndex = -1;

    private void Awake()
    {
        if (loopingSource == null)
        {
            loopingSource = GetComponent<AudioSource>();
        }

        if (oneShotSource == null)
        {
            oneShotSource = loopingSource;
        }
    }

    private void Update()
    {
        if (hurtTimer > 0f)
        {
            hurtTimer -= Time.deltaTime;
        }
    }

    public void UpdateFootsteps(bool isMoving, bool isGrounded)
    {
        if (!isMoving || !isGrounded || footstepClips == null || footstepClips.Length == 0 || oneShotSource == null)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer -= Time.deltaTime;
        if (footstepTimer > 0f)
        {
            return;
        }

        int clipIndex = GetNextFootstepIndex();
        oneShotSource.PlayOneShot(footstepClips[clipIndex]);
        footstepTimer = footstepInterval;
    }

    public void PlayJump()
    {
        if (jumpClip != null && oneShotSource != null)
        {
            oneShotSource.PlayOneShot(jumpClip, jumpVolume);
        }
    }

    public void PlayCrouch()
    {
        if (crouchClip != null && oneShotSource != null)
        {
            oneShotSource.PlayOneShot(crouchClip, crouchVolume);
        }
    }

    public void PlayHurt()
    {
        if (hurtClip == null || oneShotSource == null || hurtTimer > 0f)
        {
            return;
        }

        oneShotSource.PlayOneShot(hurtClip, hurtVolume);
        hurtTimer = hurtCooldown;
    }

    private int GetNextFootstepIndex()
    {
        if (footstepClips.Length == 1)
        {
            return 0;
        }

        int clipIndex = lastFootstepIndex;
        while (clipIndex == lastFootstepIndex)
        {
            clipIndex = Random.Range(0, footstepClips.Length);
        }

        lastFootstepIndex = clipIndex;
        return clipIndex;
    }
}
