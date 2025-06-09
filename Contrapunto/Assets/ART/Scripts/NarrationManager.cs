using UnityEngine;

public class NarrationManager : MonoBehaviour
{
    public static NarrationManager Instance;

    [Header("Narration")]
    public AudioSource narrationSource;
    private AudioClip lastNarrationClip;
    private bool isPlayingNarration;

    // — ¡Nuevo flag! habilita/disables el E
    [HideInInspector]
    public bool repeatEnabled = true;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        // Detectar fin de reproducción
        if (isPlayingNarration && !narrationSource.isPlaying)
            isPlayingNarration = false;

        // Repetir última narración con E, solo si repeatEnabled == true
        if (repeatEnabled &&
            Input.GetKeyDown(KeyCode.E) &&
            !isPlayingNarration &&
            lastNarrationClip != null)
        {
            PlayNarration(lastNarrationClip);
        }
    }

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

