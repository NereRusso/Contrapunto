using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource sfxSource;
    public AudioClip pickupClip;
    public AudioClip errorClip;
    public AudioClip successClip;

    [Header("Narración de error")]
    public AudioClip audio32Reni;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayPickupSound()
    {
        if (pickupClip != null)
            sfxSource.PlayOneShot(pickupClip);
    }

    public void PlayErrorSound()
    {
        if (errorClip != null)
            sfxSource.PlayOneShot(errorClip);
    }

    public void PlaySuccessSound()
    {
        if (successClip != null)
            sfxSource.PlayOneShot(successClip);
    }

    public void PlayErrorWithNarration()
    {
        PlayErrorSound();

        if (audio32Reni != null && NarrationManager.Instance != null)
        {
            NarrationManager.Instance.PlayNarration(audio32Reni);
        }
    }
}
