using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundClue : ProximityInteractableBase
{
    [Header("Audio Settings")]
    [SerializeField] private bool loopSoundWhileNearby = true;
    [SerializeField] private AudioClip clip;
    private AudioSource audioSource;

    protected override void Start() {
        base.Start();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    protected override void OnPlayerEnteredRange()
    {
        if (!audioSource || !clip) return;

        if (loopSoundWhileNearby)
        {
            audioSource.loop = true;
            if (audioSource.clip != clip) audioSource.clip = clip;
            if (!audioSource.isPlaying) audioSource.Play();
        }
        else
        {
            audioSource.loop = false;
            audioSource.PlayOneShot(clip);
        }
    }

    protected override void OnPlayerExitedRange()
    {
        if (!audioSource) return;
        if (loopSoundWhileNearby) audioSource.Stop();
    }
}
