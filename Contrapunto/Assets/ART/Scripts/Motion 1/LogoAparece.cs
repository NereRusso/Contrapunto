using UnityEngine;

public class LogoAparece : MonoBehaviour
{
    [Header("Narración al ver la esfera (solo 1 vez)")]
    public AudioClip audioMili4;
    private AudioSource audioSource;
    private Camera mainCamera;

    [Header("Delay")]
    public float delayBeforeSound = 0.5f; // cuanto tarda antes de sonar

    private bool hasPlayed = false;
    private bool isWaiting = false;

    void Start()
    {
        Transform narrationTransform = transform.Find("AudioMili4");
        if (narrationTransform != null)
        {
            audioSource = narrationTransform.GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            Debug.LogError("No se encontró AudioSource en NarrationSound.");
            return;
        }

        mainCamera = Camera.main;
    }

    void Update()
    {
        if (hasPlayed || audioMili4 == null || audioSource == null)
            return;

        Vector3 vp = mainCamera.WorldToViewportPoint(transform.position);
        bool inFront = vp.z > 0;
        bool inView = vp.x >= 0 && vp.x <= 1 && vp.y >= 0 && vp.y <= 1;

        if (inFront && inView && !isWaiting)
        {
            isWaiting = true;
            Invoke(nameof(PlayNarration), delayBeforeSound);
        }
    }

    void PlayNarration()
    {
        if (hasPlayed) return;

        NarrationManager.Instance.PlayNarration(audioMili4);
        hasPlayed = true;
    }
}
