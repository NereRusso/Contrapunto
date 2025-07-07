using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CambioSceneAudio : MonoBehaviour
{
    [Header("Nombre de la escena a cargar")]
    public string sceneName;

    [Header("Video Opcional")]
    public VideoPlayer videoPlayer;
    public RawImage videoImage; // RawImage que muestra el video

    [Header("Jugador (para bloquear movimiento)")]
    public GameObject player;

    [Header("Referencias de sonido")]
    public GameObject sonidoAmbiente;
    public GameObject logoAmbiente;

    [Header("Narración")]
    public AudioSource audioNarracion;
    public float fadeDuration = 1f; // FADE SOLO DURA EL PRIMER SEGUNDO DEL VIDEO

    [Header("Prompt de click")]
    [Tooltip("Arrastrá acá tu Canvas (o GameObject) con el texto “click”")]
    public GameObject clickCanvas;
    [Tooltip("Distancia máxima para que aparezca el prompt")]
    public float pickupRange = 3f;

    private FirstPersonController fpsController;
    private StarterAssetsInputs starterInputs;
    private PlayerInput playerInput;

    private AudioSource sonidoAmbienteSource;
    private AudioSource logoAmbienteSource;

    private Camera mainCamera;
    private bool clicked = false;

    void Start()
    {
        // --- Lógica original ---
        if (player != null)
        {
            Transform playerCapsule = player.transform.Find("PlayerCapsule");
            if (playerCapsule != null)
            {
                fpsController = playerCapsule.GetComponent<FirstPersonController>();
                starterInputs = playerCapsule.GetComponent<StarterAssetsInputs>();
                playerInput = playerCapsule.GetComponent<PlayerInput>();
            }
        }

        if (sonidoAmbiente != null)
            sonidoAmbienteSource = sonidoAmbiente.GetComponent<AudioSource>();

        if (logoAmbiente != null)
            logoAmbienteSource = logoAmbiente.GetComponent<AudioSource>();

        if (videoImage != null)
        {
            Color c = videoImage.color;
            videoImage.color = new Color(c.r, c.g, c.b, 0f); // empieza invisible
        }

        // --- Inicialización prompt de click ---
        mainCamera = Camera.main;
        if (clickCanvas != null)
            clickCanvas.SetActive(false);
    }

    private void OnMouseEnter()
    {
        if (mainCamera != null && clickCanvas != null &&
            Vector3.Distance(mainCamera.transform.position, transform.position) <= pickupRange)
        {
            clickCanvas.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        if (clickCanvas != null)
            clickCanvas.SetActive(false);
    }

    void OnMouseDown()
    {
        // Ocultamos prompt al clickear
        if (clickCanvas != null)
            clickCanvas.SetActive(false);

        if (clicked) return;
        clicked = true;

        if (fpsController != null) fpsController.enabled = false;
        if (starterInputs != null) starterInputs.enabled = false;
        if (playerInput != null) playerInput.enabled = false;

        if (AmbientManager.Instance != null)
            AmbientManager.Instance.StopAllAmbients();

        if (sonidoAmbienteSource != null)
            StartCoroutine(FadeOutAudio(sonidoAmbienteSource, fadeDuration));

        if (logoAmbienteSource != null)
            StartCoroutine(FadeOutAudio(logoAmbienteSource, fadeDuration));

        if (videoPlayer != null && videoImage != null)
        {
            videoImage.gameObject.SetActive(true);
            videoPlayer.gameObject.SetActive(true);
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoFinished;

            if (audioNarracion != null)
            {
                audioNarracion.volume = 1f; // asegurarse de que empiece en volumen alto
                audioNarracion.Play();
            }

            StartCoroutine(FadeInWhilePlaying());
        }

        else
        {
            LoadScene();
        }
    }

    System.Collections.IEnumerator FadeInWhilePlaying()
    {
        float audioFadeDuration = (float)videoPlayer.length;
        float timer = 0f;

        while (timer < audioFadeDuration)
        {
            timer += Time.deltaTime;

            // Fade de video
            if (timer <= fadeDuration)
            {
                float tVideo = timer / fadeDuration;
                var c = videoImage.color;
                videoImage.color = new Color(c.r, c.g, c.b, tVideo);
            }
            else
            {
                var c = videoImage.color;
                videoImage.color = new Color(c.r, c.g, c.b, 1f);
            }

            // Fade de narración
            if (audioNarracion != null)
            {
                float tAudio = Mathf.Clamp01(timer / audioFadeDuration);
                audioNarracion.volume = Mathf.Lerp(1f, 0f, tAudio);
            }

            yield return null;
        }
    }

    System.Collections.IEnumerator FadeOutAudio(AudioSource audioSource, float duration)
    {
        if (audioSource == null || !audioSource.isPlaying) yield break;

        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        LoadScene();
    }

    void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }
}
