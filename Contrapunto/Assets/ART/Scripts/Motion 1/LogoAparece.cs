using UnityEngine;

public class LogoAparece : MonoBehaviour
{
    [Header("Narraci�n al ver la esfera (solo 1 vez)")]
    public AudioClip audioMili4;
    private AudioSource audioSource;

    [Header("C�mara principal (opcional: arr�strala aqu�)")]
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
            Debug.LogError("LogoAparece: no se encontr� AudioSource en el hijo 'AudioMili4'.");
            enabled = false;
            return;
        }

        // Si no arrastraste la c�mara en el Inspector, intentamos Camera.main
        if (mainCamera == null)
            mainCamera = Camera.main;

        // �ltimo recurso: buscamos cualquier c�mara en la escena
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();

        if (mainCamera == null)
        {
            Debug.LogError("LogoAparece: no se encontr� ninguna c�mara en escena. " +
                           "Arrastra tu c�mara al campo 'mainCamera' o etiqueta tu c�mara como MainCamera.");
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
