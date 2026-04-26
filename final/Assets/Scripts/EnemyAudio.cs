using UnityEngine;

public class EnemyAudio : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private AudioSource humSource;
    [SerializeField] private AudioSource moveSource;
    [SerializeField] private AudioSource alertSource;

    [Header("Clips")]
    [SerializeField] private AudioClip humClip;
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip alertClip;

    [Header("Tuning")]
    [SerializeField] private float humVolume = 0.5f;
    [SerializeField] private float moveVolume = 0.6f;
    [SerializeField] private float alertVolume = 1.0f;

    private void Awake()
    {
        ConfigureLoopingSource(humSource, humClip, humVolume);
        ConfigureLoopingSource(moveSource, moveClip, moveVolume);
    }

    private void Start()
    {
        if (humSource != null && humClip != null)
        {
            humSource.Play();
        }
    }

    public void SetMoving(bool isMoving)
    {
        if (moveSource == null || moveClip == null)
        {
            return;
        }

        if (isMoving)
        {
            if (!moveSource.isPlaying)
            {
                moveSource.Play();
            }
        }
        else if (moveSource.isPlaying)
        {
            moveSource.Stop();
        }
    }

    public void PlayAlert()
    {
        if (alertSource != null && alertClip != null)
        {
            alertSource.PlayOneShot(alertClip, alertVolume);
        }
    }

    private static void ConfigureLoopingSource(AudioSource source, AudioClip clip, float volume)
    {
        if (source == null)
        {
            return;
        }

        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = volume;
    }
}
