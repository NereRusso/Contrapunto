using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using StarterAssets;
using UnityEngine.InputSystem;

public class CambioScene : MonoBehaviour
{
    [Header("Nombre de la escena a cargar")]
    public string sceneName;

    [Header("Video Opcional")]
    public VideoPlayer videoPlayer;

    [Header("Jugador (para bloquear movimiento)")]
    public GameObject player;

    [Header("Referencias de sonido")]
    public GameObject sonidoAmbiente;    // sonido ambiente general (opcional)
    public GameObject logoAmbiente;      // sonido ambiente del prefab (opcional)

    private FirstPersonController fpsController;
    private StarterAssetsInputs starterInputs;
    private PlayerInput playerInput;

    private AudioSource sonidoAmbienteSource;
    private AudioSource logoAmbienteSource;

    private bool clicked = false;

    void Start()
    {
        // Obtener referencias al controlador del jugador
        if (player != null)
        {
            Transform playerCapsule = player.transform.Find("PlayerCapsule");
            if (playerCapsule != null)
            {
                fpsController = playerCapsule.GetComponent<FirstPersonController>();
                starterInputs = playerCapsule.GetComponent<StarterAssetsInputs>();
                playerInput = playerCapsule.GetComponent<PlayerInput>();
            }
            else
            {
                Debug.LogWarning("No se encontró PlayerCapsule dentro del objeto player.");
            }
        }

        // Obtener AudioSource del sonido ambiente general (en escena)
        if (sonidoAmbiente != null)
        {
            sonidoAmbienteSource = sonidoAmbiente.GetComponent<AudioSource>();
            if (sonidoAmbienteSource == null)
            {
                sonidoAmbienteSource = sonidoAmbiente.GetComponentInChildren<AudioSource>();
            }
        }

        // Obtener AudioSource del logoAmbiente (dentro del prefab)
        if (logoAmbiente != null)
        {
            logoAmbienteSource = logoAmbiente.GetComponent<AudioSource>();
        }
        else
        {
            // Búsqueda automática interna
            Transform internalLogo = transform.Find("AudioAmbiente");
            if (internalLogo != null)
            {
                logoAmbienteSource = internalLogo.GetComponent<AudioSource>();
            }
        }
    }

    void OnMouseDown()
    {
        if (clicked) return;
        clicked = true;

        // Bloquear controles del jugador
        if (fpsController != null) fpsController.enabled = false;
        if (starterInputs != null) starterInputs.enabled = false;
        if (playerInput != null) playerInput.enabled = false;

        // Detener sonidos del AmbientManager
        if (AmbientManager.Instance != null)
        {
            AmbientManager.Instance.StopAllAmbients();
            Debug.Log("Deteniendo todos los sonidos del AmbientManager");
        }

        // Detener sonido ambiente adicional (si está fuera del AmbientManager)
        if (sonidoAmbienteSource != null && sonidoAmbienteSource.isPlaying)
        {
            sonidoAmbienteSource.Stop();
        }

        // Detener sonido del logoAmbiente
        if (logoAmbienteSource != null && logoAmbienteSource.isPlaying)
        {
            logoAmbienteSource.Stop();
        }

        // Reproducir video si está configurado
        if (videoPlayer != null)
        {
            videoPlayer.gameObject.SetActive(true);
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoFinished;
        }
        else
        {
            LoadScene();
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        LoadScene();
    }

    void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
