using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CambioScenes: MonoBehaviour
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

    public float fadeDuration = 2f;

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
            videoImage.color = new Color(c.r, c.g, c.b, 0f); // opacidad 0
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

        StartCoroutine(FadeInVideo());
    }

    System.Collections.IEnumerator FadeInVideo()
    {
        float timer = 0f;

        if (videoPlayer != null && videoImage != null)
        {
            videoPlayer.gameObject.SetActive(true);
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoFinished;

            while (timer < fadeDuration)
            {
                float t = timer / fadeDuration;

                // Fade del RawImage del video
                Color c = videoImage.color;
                videoImage.color = new Color(c.r, c.g, c.b, t);

                timer += Time.deltaTime;
                yield return null;
            }

            // Asegurar visibilidad total al final
            Color finalColor = videoImage.color;
            videoImage.color = new Color(finalColor.r, finalColor.g, finalColor.b, 1f);
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
