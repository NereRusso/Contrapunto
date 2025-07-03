using UnityEngine;
using UnityEngine.Video;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    public float fadeTime = 1f;

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
            videoImage.color = new Color(1f, 1f, 1f, 0f);

        if (segundoVideoImage != null)
            segundoVideoImage.color = new Color(1f, 1f, 1f, 0f);
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

        if (sonidoAmbienteSource != null)
            StartCoroutine(FadeOutAudio(sonidoAmbienteSource, fadeTime));

        if (logoAmbienteSource != null)
            StartCoroutine(FadeOutAudio(logoAmbienteSource, fadeTime));

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

    System.Collections.IEnumerator CambiarAVideoFinal()
    {
        if (segundoVideoPlayer != null && segundoVideoImage != null)
        {
            segundoVideoImage.gameObject.SetActive(true);
            segundoVideoPlayer.gameObject.SetActive(true);
            segundoVideoPlayer.Play();

            yield return StartCoroutine(FadeVideoIn(segundoVideoImage));

            // luego de mostrar el segundo video, desactivamos el primero
            videoPlayer.gameObject.SetActive(false);
            videoImage.gameObject.SetActive(false);
        }
    }

    System.Collections.IEnumerator FadeVideoIn(RawImage image)
    {
        float timer = 0f;
        while (timer < fadeTime)
        {
            float alpha = timer / fadeTime;
            Color c = image.color;
            image.color = new Color(c.r, c.g, c.b, alpha);
            timer += Time.deltaTime;
            yield return null;
        }
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
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
}
