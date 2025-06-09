using UnityEngine;

public class NarrationManager : MonoBehaviour
{
    public static NarrationManager Instance;

    [Header("Narration")]
    public AudioSource narrationSource;
    private AudioClip lastNarrationClip;
    private bool isPlayingNarration;

    private void Awake()
    {
        // Singleton para acceso global
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        // Verifica si termin� de reproducirse
        if (isPlayingNarration && !narrationSource.isPlaying)
        {
            isPlayingNarration = false;
        }

        // Repetir �ltima narraci�n con E
        if (Input.GetKeyDown(KeyCode.E) && !isPlayingNarration && lastNarrationClip != null)
        {
            PlayNarration(lastNarrationClip);
        }
    }

    /// <summary>
    /// Llama esto cada vez que se reproduzca una nueva narraci�n
    /// </summary>
    public void PlayNarration(AudioClip clip)
    {
        if (clip == null) return;

        lastNarrationClip = clip;
        narrationSource.Stop();
        narrationSource.clip = clip;
        narrationSource.Play();
        isPlayingNarration = true;
    }
}
