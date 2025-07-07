using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using UnityEngine.UI;

public class LogoGlitch : MonoBehaviour
{
    [Header("Nombre de la escena a cargar")]
    public string sceneName;

    [Header("Jugador (para bloquear movimiento)")]
    public GameObject player;

    [Header("Referencias de sonido")]
    public GameObject sonidoAmbiente;
    public GameObject logoAmbiente;

    [Header("Narración")]
    public AudioClip audio4Marti;

    [Header("Video")]
    public GameObject videoCanvas;           // Canvas con RawImage y VideoPlayer
    public VideoPlayer videoPlayer;          // VideoPlayer que usa RenderTexture
    public float delayBeforeVideo = 2f;

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
        // Setup del jugador y audio
        if (player != null)
        {
            fpsController = player.GetComponent<FirstPersonController>();
            starterInputs = player.GetComponent<StarterAssetsInputs>();
            playerInput = player.GetComponent<PlayerInput>();
        }

        if (sonidoAmbiente != null) sonidoAmbienteSource = sonidoAmbiente.GetComponent<AudioSource>();
        if (logoAmbiente != null) logoAmbienteSource = logoAmbiente.GetComponent<AudioSource>();

        // Desactivamos el video al inicio
        if (videoCanvas != null) videoCanvas.SetActive(false);

        // Inicialización del prompt de click
        mainCamera = Camera.main;
        if (clickCanvas != null)
            clickCanvas.SetActive(false);
    }

    private void OnMouseEnter()
    {
        // No mostramos si ya clickeamos
        if (clicked) return;

        // Si estamos cerca, activamos el prompt
        if (mainCamera != null && clickCanvas != null &&
            Vector3.Distance(mainCamera.transform.position, transform.position) <= pickupRange)
        {
            clickCanvas.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        // No ocultamos si ya clickeamos
        if (clicked) return;

        if (clickCanvas != null)
            clickCanvas.SetActive(false);
    }

    void OnMouseDown()
    {
        if (clicked) return;
        clicked = true;

        // Ocultamos el prompt al clickear
        if (clickCanvas != null)
            clickCanvas.SetActive(false);

        StartCoroutine(DelayedExecution());
    }

    IEnumerator DelayedExecution()
    {
        yield return new WaitForSeconds(1f);

        if (sonidoAmbienteSource != null && sonidoAmbienteSource.isPlaying)
            sonidoAmbienteSource.Stop();
        if (logoAmbienteSource != null && logoAmbienteSource.isPlaying)
            logoAmbienteSource.Stop();

        if (audio4Marti != null)
            NarrationManager.Instance.PlayNarration(audio4Marti);

        yield return new WaitForSeconds(delayBeforeVideo);

        if (videoCanvas != null) videoCanvas.SetActive(true);

        if (videoPlayer != null)
        {
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        vp.loopPointReached -= OnVideoFinished;
        LoadScene();
    }

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }
}
