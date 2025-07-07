using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CambioSceneVideoSimple : MonoBehaviour
{
    [Header("Nombre de la escena a cargar")]
    public string sceneName;

    [Header("Video Opcional")]
    public VideoPlayer videoPlayer;
    public RawImage videoImage;

    [Header("Jugador (para bloquear movimiento)")]
    public GameObject player;

    [Header("Referencias de sonido")]
    public GameObject sonidoAmbiente;
    public GameObject logoAmbiente;

    public float fadeTime = 1f; // cuánto tarda el fade, independiente de la duración del video

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
        // Inicialización original
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

        // --- Prompt de click ---
        mainCamera = Camera.main;
        if (clickCanvas != null)
            clickCanvas.SetActive(false);
    }

    private void OnMouseEnter()
    {
        // Si estamos cerca y mirando el collider, activa el prompt
        if (mainCamera != null && clickCanvas != null &&
            Vector3.Distance(mainCamera.transform.position, transform.position) <= pickupRange)
        {
            clickCanvas.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        // Al salir del collider, oculta el prompt
        if (clickCanvas != null)
            clickCanvas.SetActive(false);
    }

    void OnMouseDown()
    {
        // Oculta el prompt al hacer click
        if (clickCanvas != null)
            clickCanvas.SetActive(false);

        if (clicked) return;
        clicked = true;

        // Deshabilita controles del player
        if (fpsController != null) fpsController.enabled = false;
        if (starterInputs != null) starterInputs.enabled = false;
        if (playerInput != null) playerInput.enabled = false;

        // Fade out de ambientes
        if (AmbientManager.Instance != null)
            AmbientManager.Instance.StopAllAmbients();

        if (sonidoAmbienteSource != null)
            StartCoroutine(FadeOutAudio(sonidoAmbienteSource, fadeTime));

        if (logoAmbienteSource != null)
            StartCoroutine(FadeOutAudio(logoAmbienteSource, fadeTime));

        // Reproducción de video o carga de escena
        if (videoPlayer != null && videoImage != null)
        {
            videoImage.gameObject.SetActive(true);
            videoPlayer.gameObject.SetActive(true);
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoFinished;
            StartCoroutine(FadeVideoInDuringPlayback());
        }
        else
        {
            LoadScene();
        }
    }

    System.Collections.IEnumerator FadeVideoInDuringPlayback()
    {
        float timer = 0f;
        while (timer < fadeTime)
        {
            float alpha = timer / fadeTime;
            Color c = videoImage.color;
            videoImage.color = new Color(c.r, c.g, c.b, alpha);
            timer += Time.deltaTime;
            yield return null;
        }
        Color finalColor = videoImage.color;
        videoImage.color = new Color(finalColor.r, finalColor.g, finalColor.b, 1f);
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
