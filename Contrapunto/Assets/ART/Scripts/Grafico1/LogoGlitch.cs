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

    private FirstPersonController fpsController;
    private StarterAssetsInputs starterInputs;
    private PlayerInput playerInput;
    private AudioSource sonidoAmbienteSource;
    private AudioSource logoAmbienteSource;
    private bool clicked = false;

    void Start()
    {
        if (player != null)
        {
            fpsController = player.GetComponent<FirstPersonController>();
            starterInputs = player.GetComponent<StarterAssetsInputs>();
            playerInput = player.GetComponent<PlayerInput>();
        }

        if (sonidoAmbiente != null) sonidoAmbienteSource = sonidoAmbiente.GetComponent<AudioSource>();
        if (logoAmbiente != null) logoAmbienteSource = logoAmbiente.GetComponent<AudioSource>();

        if (videoCanvas != null) videoCanvas.SetActive(false);
    }

    void OnMouseDown()
    {
        if (clicked) return;
        clicked = true;
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
