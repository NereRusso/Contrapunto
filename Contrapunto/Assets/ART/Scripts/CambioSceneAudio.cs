using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class CambioSceneAudio : MonoBehaviour
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

    [Header("Narración")]
    public AudioSource audioNarracion;
    public float fadeDuration = 1f;

    private FirstPersonController fpsController;
    private StarterAssetsInputs starterInputs;
    private PlayerInput playerInput;
    private AudioSource sonidoAmbienteSource;
    private AudioSource logoAmbienteSource;
    private bool clicked = false;

    void Start()
    {
        // Inicializaciones (idem tu versión)
        if (player != null)
        {
            var pc = player.transform.Find("PlayerCapsule");
            if (pc != null)
            {
                fpsController = pc.GetComponent<FirstPersonController>();
                starterInputs = pc.GetComponent<StarterAssetsInputs>();
                playerInput = pc.GetComponent<PlayerInput>();
            }
        }
        sonidoAmbienteSource = sonidoAmbiente ? sonidoAmbiente.GetComponent<AudioSource>() : null;
        logoAmbienteSource = logoAmbiente ? logoAmbiente.GetComponent<AudioSource>() : null;

        // Empieza invisible
        if (videoImage != null)
        {
            var c = videoImage.color;
            videoImage.color = new Color(c.r, c.g, c.b, 0f);
        }
    }

    void OnMouseDown()
    {
        if (clicked) return;
        clicked = true;

        // ?? Deshabilitar controles y audio ambiente
        if (fpsController != null) fpsController.enabled = false;
        if (starterInputs != null) starterInputs.enabled = false;
        if (playerInput != null) playerInput.enabled = false;
        if (AmbientManager.Instance != null) AmbientManager.Instance.StopAllAmbients();
        if (sonidoAmbienteSource != null) sonidoAmbienteSource.Stop();
        if (logoAmbienteSource != null) logoAmbienteSource.Stop();

        // ?? Si hay vídeo, lo activamos como overlay persistente…
        if (videoPlayer != null && videoImage != null)
        {
            // 1) Activar UI
            videoImage.gameObject.SetActive(true);
            videoPlayer.gameObject.SetActive(true);

            // 2) Mantenerlo vivo al cambiar de escena
            DontDestroyOnLoad(videoImage.gameObject);
            DontDestroyOnLoad(videoPlayer.gameObject);

            // 3) Reproducir y suscribir callback
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Play();

            // 4) Fade-in del RawImage
            StartCoroutine(FadeInWhilePlaying());
        }

        if (audioNarracion != null)
        {
            audioNarracion.volume = 1f;
            audioNarracion.Play();
        }
        else
        {
            // Si no hay vídeo, solo cargamos la escena
            StartCoroutine(LoadSceneAsync());
        }
    }

    private IEnumerator FadeInWhilePlaying()
    {
        float timer = 0f;
        

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            // Fade del RawImage
            var c = videoImage.color;
            videoImage.color = new Color(c.r, c.g, c.b, t);

            // Fade del volumen de la narración
            if (audioNarracion != null)
                audioNarracion.volume = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        // Asegurar visibilidad total y parar narración
        videoImage.color = new Color(videoImage.color.r, videoImage.color.g, videoImage.color.b, 1f);
        if (audioNarracion != null) audioNarracion.Stop();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // Al terminar el vídeo, arrancamos la carga asíncrona
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        var op = SceneManager.LoadSceneAsync(sceneName);
        // Lo dejamos activar automáticamente:
        yield return op;
        // NO destruimos nada aquí (lo hará el script de la escena B)
    }
}
