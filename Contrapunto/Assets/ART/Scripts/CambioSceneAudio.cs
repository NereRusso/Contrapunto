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
    }

    void OnMouseDown()
    {
        if (clicked) return;
        clicked = true;

        if (fpsController != null) fpsController.enabled = false;
        if (starterInputs != null) starterInputs.enabled = false;
        if (playerInput != null) playerInput.enabled = false;

        if (AmbientManager.Instance != null)
            AmbientManager.Instance.StopAllAmbients();

        if (sonidoAmbienteSource != null) sonidoAmbienteSource.Stop();
        if (logoAmbienteSource != null) logoAmbienteSource.Stop();

        if (videoPlayer != null && videoImage != null)
        {
            videoImage.gameObject.SetActive(true);
            videoPlayer.gameObject.SetActive(true);
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoFinished;

            StartCoroutine(FadeInWhilePlaying());
        }
        else
        {
            LoadScene();
        }
    }

    System.Collections.IEnumerator FadeInWhilePlaying()
    {
        // Duración total del audio fade = duración del vídeo
        float audioFadeDuration = (float)videoPlayer.length;

        float timer = 0f;
        while (timer < audioFadeDuration)
        {
            // Avanzar tiempo
            timer += Time.deltaTime;

            // 1) Video fade en el primer 'videoFadeDuration' segundos
            if (timer <= fadeDuration)
            {
                float tVideo = timer / fadeDuration;
                var c = videoImage.color;
                videoImage.color = new Color(c.r, c.g, c.b, tVideo);
            }
            else
            {
                // Aseguramos opacidad total después del fade de vídeo
                var c = videoImage.color;
                videoImage.color = new Color(c.r, c.g, c.b, 1f);
            }

            // 2) Audio fade durante todo el audioFadeDuration
            if (audioNarracion != null)
            {
                float tAudio = Mathf.Clamp01(timer / audioFadeDuration);
                audioNarracion.volume = Mathf.Lerp(1f, 0f, tAudio);
            }

            yield return null;
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
