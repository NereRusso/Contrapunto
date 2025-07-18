using UnityEngine;

public class LogoAparece : MonoBehaviour
{
    [Header("Narración al ver la esfera (solo 1 vez)")]
    public AudioClip audioMili4;
    private AudioSource audioSource;

    [Header("Cámara principal (opcional: arrástrala aquí)")]
    public Camera mainCamera;

    [Header("Delay")]
    public float delayBeforeSound = 0.5f;

    private bool hasPlayed = false;
    private bool isWaiting = false;

    void Start()
    {
        // Buscamos el AudioSource en un hijo llamado "AudioMili4"
        Transform narrationTransform = transform.Find("AudioMili4");
        if (narrationTransform != null)
            audioSource = narrationTransform.GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("LogoAparece: no se encontró AudioSource en el hijo 'AudioMili4'.");
            enabled = false;
            return;
        }

        // Si no arrastraste la cámara en el Inspector, intentamos Camera.main
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Último recurso: buscamos cualquier cámara en la escena
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();

        if (mainCamera == null)
        {
            Debug.LogError("LogoAparece: no se encontró ninguna cámara en escena. " +
                           "Arrastra tu cámara al campo 'mainCamera' o etiqueta tu cámara como MainCamera.");
            enabled = false;
            return;
        }
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
