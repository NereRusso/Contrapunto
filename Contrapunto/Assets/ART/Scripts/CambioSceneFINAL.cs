using UnityEngine;
using UnityEngine.Video;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class CambioSceneFINAL : MonoBehaviour
{
    [Header("Primer Video")]
    public VideoPlayer videoPlayer;
    public RawImage videoImage;

    [Header("Segundo Video")]
    public VideoPlayer segundoVideoPlayer;
    public RawImage segundoVideoImage;

    [Header("Jugador (para bloquear movimiento)")]
    public GameObject player;

    [Header("Referencias de sonido")]
    public GameObject sonidoAmbiente;
    public GameObject logoAmbiente;
    public GameObject logoFinal;
    public GameObject fondoNegro;

    public float fadeTime = 1f;

    [Header("Prompt de click")]
    [Tooltip("Arrastrá acá tu Canvas (o GameObject) con el texto “click”")]
    public GameObject clickCanvas;
    [Tooltip("Distancia máxima para que aparezca el prompt")]
    public float pickupRange = 3f;

    [Header("Pantalla Final")]
    [Tooltip("Canvas con fondo negro y un Button para redirigir")]
    public GameObject finalScreenCanvas;
    [Tooltip("Botón dentro de finalScreenCanvas")]
    public Button redirectButton;
    [Tooltip("URL externa a la que quieres ir")]
    public string redirectURL;

    // Nuevo: CanvasGroup para controlar alpha e interactividad
    private CanvasGroup finalScreenCanvasGroup;

    private FirstPersonController fpsController;
    private StarterAssetsInputs starterInputs;
    private PlayerInput playerInput;

    private AudioSource sonidoAmbienteSource;
    private AudioSource logoAmbienteSource;
    private AudioSource logoFinalSource;

    private Camera mainCamera;
    private bool clicked = false;

    void Start()
    {
        // Init player controls
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

        // Audio sources
        if (sonidoAmbiente != null) sonidoAmbienteSource = sonidoAmbiente.GetComponent<AudioSource>();
        if (logoAmbiente != null) logoAmbienteSource = logoAmbiente.GetComponent<AudioSource>();
        if (logoFinal != null) logoFinalSource = logoFinal.GetComponent<AudioSource>();

        // Hide video images
        if (videoImage != null) videoImage.color = new Color(1, 1, 1, 0);
        if (segundoVideoImage != null) segundoVideoImage.color = new Color(1, 1, 1, 0);

        // Prompt setup
        mainCamera = Camera.main;
        if (clickCanvas != null) clickCanvas.SetActive(false);

        // Final screen setup con CanvasGroup
        if (finalScreenCanvas != null)
        {
            // Asegurarse de que esté activo (pero invisible) para que CanvasGroup funcione
            finalScreenCanvas.SetActive(true);

            finalScreenCanvasGroup = finalScreenCanvas.GetComponent<CanvasGroup>();
            if (finalScreenCanvasGroup == null)
                finalScreenCanvasGroup = finalScreenCanvas.AddComponent<CanvasGroup>();

            finalScreenCanvasGroup.alpha = 0f;
            finalScreenCanvasGroup.interactable = false;
            finalScreenCanvasGroup.blocksRaycasts = false;
        }
    }

    void Update()
    {
        if (clicked || mainCamera == null || clickCanvas == null)
            return;

        float d = Vector3.Distance(mainCamera.transform.position, transform.position);
        if (d <= pickupRange)
        {
            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange) && hit.transform == transform)
                clickCanvas.SetActive(true);
            else
                clickCanvas.SetActive(false);
        }
        else
        {
            clickCanvas.SetActive(false);
        }
    }

    void OnMouseDown()
    {
        if (clickCanvas != null)
            clickCanvas.SetActive(false);

        if (clicked) return;
        clicked = true;

        // Deshabilitar controles del jugador
        if (fpsController != null) fpsController.enabled = false;
        if (starterInputs != null) starterInputs.enabled = false;
        if (playerInput != null) playerInput.enabled = false;

        // Fade out de ambientes
        if (AmbientManager.Instance != null)
            AmbientManager.Instance.StopAllAmbients();
        if (sonidoAmbienteSource != null) StartCoroutine(FadeOutAudio(sonidoAmbienteSource, fadeTime));
        if (logoAmbienteSource != null) StartCoroutine(FadeOutAudio(logoAmbienteSource, fadeTime));
        if (logoFinalSource != null) StartCoroutine(FadeOutAudio(logoFinalSource, fadeTime));

        // Reproducir primer video
        if (videoPlayer != null && videoImage != null)
        {
            videoImage.gameObject.SetActive(true);
            videoPlayer.gameObject.SetActive(true);
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoFinished;
            StartCoroutine(FadeVideoIn(videoImage));
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        StartCoroutine(CambiarAVideoFinal());
    }

    IEnumerator CambiarAVideoFinal()
    {
        if (segundoVideoPlayer != null && segundoVideoImage != null)
        {
            segundoVideoImage.gameObject.SetActive(true);

            // Subscribir para detectar el fin (solo para ocultar el video)
            segundoVideoPlayer.loopPointReached += OnFinalVideoEnded;

            segundoVideoPlayer.gameObject.SetActive(true);
            segundoVideoPlayer.Play();

            yield return StartCoroutine(FadeVideoIn(segundoVideoImage));

            // Desactivar primer video
            videoPlayer.gameObject.SetActive(false);
            videoImage.gameObject.SetActive(false);

            // Nuevo: empezar conteo para mostrar canvas final 7s antes
            StartCoroutine(ShowFinalScreenBeforeEnd());
        }
    }

    // Nuevo: espera hasta (duración - 7s) y lanza el fade del canvas
    IEnumerator ShowFinalScreenBeforeEnd()
    {
        double totalDuration = segundoVideoPlayer.length;
        double waitTime = totalDuration - 7.0;
        if (waitTime > 0)
            yield return new WaitForSeconds((float)waitTime);

        yield return StartCoroutine(FadeFinalScreenIn());
    }

    // Nuevo: fade-in del finalScreenCanvas usando CanvasGroup
    IEnumerator FadeFinalScreenIn()
    {
        float timer = 0f;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            finalScreenCanvasGroup.alpha = timer / fadeTime;
            yield return null;
        }
        finalScreenCanvasGroup.alpha = 1f;
        finalScreenCanvasGroup.interactable = true;
        finalScreenCanvasGroup.blocksRaycasts = true;

        // Habilitar cursor y botón
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        fondoNegro.SetActive(true);
        if (redirectButton != null && !string.IsNullOrEmpty(redirectURL))
        {
            redirectButton.onClick.AddListener(() =>
            {
                Application.OpenURL(redirectURL);
                Application.Quit();
            });
        }
    }

    // Solo oculta el video al terminar; no muestra más el canvas aquí
    void OnFinalVideoEnded(VideoPlayer vp)
    {
        vp.loopPointReached -= OnFinalVideoEnded;
        if (segundoVideoImage != null) segundoVideoImage.gameObject.SetActive(false);
        if (segundoVideoPlayer != null) segundoVideoPlayer.gameObject.SetActive(false);
    }

    IEnumerator FadeVideoIn(RawImage image)
    {
        float timer = 0f;
        while (timer < fadeTime)
        {
            float alpha = timer / fadeTime;
            image.color = new Color(1f, 1f, 1f, alpha);
            timer += Time.deltaTime;
            yield return null;
        }
        image.color = new Color(1f, 1f, 1f, 1f);
    }

    IEnumerator FadeOutAudio(AudioSource audioSrc, float duration)
    {
        if (audioSrc == null || !audioSrc.isPlaying) yield break;
        float startVol = audioSrc.volume;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSrc.volume = Mathf.Lerp(startVol, 0f, timer / duration);
            yield return null;
        }
        audioSrc.Stop();
        audioSrc.volume = startVol;
    }
}
