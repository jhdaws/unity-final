using UnityEngine;

public class SceneAudioLoop : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip loopClip;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null || loopClip == null)
        {
            return;
        }

        audioSource.clip = loopClip;
        audioSource.loop = true;
        audioSource.playOnAwake = true;
    }

    private void Start()
    {
        if (audioSource != null && loopClip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
